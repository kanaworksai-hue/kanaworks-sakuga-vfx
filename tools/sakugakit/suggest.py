"""`suggest`: rank catalog effects for a plain-English request such as "ice arrow hitting a slime".

Simple keyword scoring over the generated recipes (element words, category words, styles, tags and use cases), so an
agent or a person can go from a skill description to effect ids without reading the whole catalog.
"""
import re

from .common import published
from .recipes import CATEGORIES, ELEMENT_WORDS

CATEGORY_WORDS = {
    'projectile': ['projectile', 'arrow', 'bolt', 'bullet', 'shot', 'shoot', 'shooting', 'ball', 'orb', 'missile',
                   'throw', 'fireball', 'lance', 'spit', 'dart', 'shard', 'snowball', 'cannonball', 'fly', 'flying',
                   'ranged', 'fire at', 'launch'],
    'impact': ['impact', 'hit', 'hits', 'hitting', 'explosion', 'explode', 'burst', 'splash', 'crash', 'damage',
               'crit', 'critical', 'onhit', 'land', 'lands', 'smash', 'pop', 'shatter', 'bomb'],
    'column': ['pillar', 'column', 'geyser', 'eruption', 'erupt', 'spike', 'spikes', 'rise', 'rising', 'fountain',
               'spout'],
    'strike': ['strike', 'smite', 'thunderbolt', 'sky', 'above', 'call', 'barrage', 'rain'],
    'vortex': ['vortex', 'tornado', 'whirlwind', 'swirl', 'pull', 'spin', 'cyclone', 'twister'],
    'area': ['aoe', 'area', 'field', 'zone', 'puddle', 'pool', 'circle', 'nova', 'cloud', 'trap', 'hazard',
             'ground', 'floor', 'crater', 'fissure', 'sigil', 'root', 'stun', 'debuff'],
    'beam': ['beam', 'laser', 'ray', 'chain', 'stream', 'zap'],
    'buff': ['buff', 'heal', 'healing', 'pickup', 'powerup', 'potion', 'gain', 'restore'],
    'shield': ['shield', 'barrier', 'protect', 'protection', 'block', 'guard', 'dome', 'bubble', 'ward'],
    'cast': ['cast', 'casting', 'charge', 'charging', 'channel', 'muzzle', 'gather', 'windup', 'summon'],
    'slash': ['slash', 'sword', 'swing', 'melee', 'claw', 'cut', 'blade', 'katana', 'combo', 'attack'],
    'aura': ['aura', 'enrage', 'status', 'powered', 'rage', 'glow', 'loop'],
}
STOP = {'a', 'an', 'the', 'of', 'on', 'at', 'to', 'for', 'and', 'with', 'into', 'from', 'my', 'that', 'is', 'in', 'by'}


def tokens(text):
    out = []
    for w in re.findall(r'[a-z0-9]+', text.lower()):
        if w in STOP:
            continue
        out.append(w)
        if len(w) > 4 and w.endswith('s'):
            out.append(w[:-1])
    return out


def _words(text):
    return set(re.findall(r'[a-z0-9]+', text.lower()))


def intents(query_tokens):
    """Categories the request asks for, strongest first."""
    hits = {c: sum(1 for t in query_tokens if t in words) for c, words in CATEGORY_WORDS.items()}
    return [c for c, n in sorted(hits.items(), key=lambda kv: -kv[1]) if n > 0]


def score(asset, query_tokens, wanted):
    q = set(query_tokens)
    s = 0.0
    element = asset['element']
    if element in q:
        s += 6
    elif q & set(ELEMENT_WORDS.get(element, ())):
        s += 5
    cat = asset.get('category', asset.get('form'))
    if cat in wanted:
        s += 4 - min(wanted.index(cat), 2)
    style = asset.get('style', 'classic')
    if style != 'classic' and style in q:
        s += 4
    tag_words = set()
    for t in asset.get('tags', []):
        tag_words |= _words(t)
    s += 1.0 * len(q & tag_words - {element, cat})
    use_words = set()
    for u in asset.get('use_cases', []):
        use_words |= _words(u)
    s += 1.5 * len(q & use_words - {element})
    for u in asset.get('use_cases', []):                     # a whole use case named in the request
        if len(u.split()) > 1 and u.lower() in ' '.join(query_tokens):
            s += 3
    if style == 'classic':
        s += 0.25                                            # the plain version first when nothing else decides
    return s


def suggest(catalog, text, limit=5):
    """[(score, asset)] best first, plus a suggested chain (cast -> projectile -> impact) when one fits."""
    q = tokens(text)
    wanted = intents(q)
    ranked = sorted(((score(a, q, wanted), a) for a in published(catalog)), key=lambda x: (-x[0], x[1]['id']))
    ranked = [(s, a) for s, a in ranked if s > 0.5][:limit]
    chain = []
    if ranked:
        by_id = {a['id']: a for a in published(catalog)}
        top = ranked[0][1]
        pairs = top.get('pairs') or {}
        if top.get('category') == 'impact' and pairs.get('projectile') and 'projectile' in wanted:
            proj = by_id.get(pairs['projectile'])
            chain = [x for x in ((proj or {}).get('pairs', {}).get('cast'), pairs['projectile'], top['id']) if x]
        elif top.get('category') in ('projectile', 'beam') and pairs.get('impact'):
            chain = [x for x in (pairs.get('cast'), top['id'], pairs['impact']) if x]
        elif pairs.get('cast') and top.get('category') in CATEGORIES:
            chain = [pairs['cast'], top['id']]
    return ranked, chain
