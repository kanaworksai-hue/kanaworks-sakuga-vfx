"""Effect recipes: how each flipbook becomes a ready-to-use layered skill effect.

A recipe says what an effect is for (category, use cases, tags, style, pairs) and how an engine should stage it
(anchor, orientation, world size, contact frame, loop range) plus the secondary layers that make a hit read as a hit:
flash, ring, sparks, debris, smoke, ground decal, light, camera shake, hit stop and sound.

Recipes are generated, never hand-written: per-element defaults, then per-category scaling, then per-style
overrides, then (rarely) per-asset overrides. `python tools/sakuga.py sync` writes them into catalog.json and into
every sidecar, so a new sheet added with `sakuga.py add` gets a complete recipe without any extra step. Engines only
read the generated data; to tune an effect, change the tables here and run sync.

Units: metres, seconds, frames at the sheet's fps. Colours are sRGB hex. Every layer is optional (null = off).
"""
import re

from .common import RECIPE_FIELDS  # noqa: F401  (re-exported: the order recipe fields are written in)

RECIPE_VERSION = 1

CATEGORIES = ('projectile', 'impact', 'area', 'column', 'strike', 'beam', 'vortex', 'buff', 'shield', 'cast',
              'slash', 'aura')
ANCHORS = ('center', 'ground')
ORIENTATIONS = ('billboard', 'upright', 'ground', 'travel')
SIZE_PRESETS = {'S': 0.6, 'M': 1.0, 'L': 1.6}

FX_LAYERS = ('flash', 'ring', 'sparks', 'debris', 'smoke', 'decal', 'light', 'trail', 'shake', 'hit_stop_s', 'sfx')

ELEMENT_COLORS = {
    'water': '#4fb6ff', 'fire': '#ff7d2e', 'thunder': '#b583ff', 'wind': '#4fe6c4', 'poison': '#93e24d',
    'ice': '#8fe3ff', 'rock': '#d9a441', 'light': '#ffe08a', 'dark': '#c35ad8',
}

# Per element: secondary motion that makes the element read (see the README "Layered effects" section).
# sparks: count, speed (m/s), gravity (x9.81), drag (1/s), size (m), life (s)
ELEMENT_FX = {
    'water': dict(sparks=(16, 5.0, 1.0, 2.0, 0.08, 0.55), debris=None, smoke=None,
                  light=2.5, shake=0.08, hit_stop=0.05),
    'fire': dict(sparks=(22, 6.0, -0.15, 2.5, 0.06, 0.75), debris=None,
                 smoke=dict(count=5, color='#4b4141', size=1.0, rise=0.9, life=1.4),
                 light=4.0, shake=0.12, hit_stop=0.03),
    'thunder': dict(sparks=(26, 9.0, 0.3, 4.0, 0.05, 0.3), debris=None, smoke=None,
                    light=6.0, shake=0.16, hit_stop=0.08),
    'wind': dict(sparks=(14, 7.0, 0.0, 3.0, 0.07, 0.45), debris=None, smoke=None,
                 light=1.2, shake=0.06, hit_stop=0.03),
    'poison': dict(sparks=(12, 3.2, 1.5, 1.0, 0.09, 0.7), debris=None,
                   smoke=dict(count=4, color='#5d7a2a', size=1.2, rise=0.3, life=1.6),
                   light=1.8, shake=0.05, hit_stop=0.04),
    'ice': dict(sparks=(16, 5.0, 1.2, 2.0, 0.06, 0.5),
                debris=dict(count=8, speed=4.0, gravity=1.0, size=0.18, life=0.9),
                smoke=None, light=2.5, shake=0.10, hit_stop=0.06),
    'rock': dict(sparks=(10, 4.0, 1.0, 1.0, 0.06, 0.5),
                 debris=dict(count=12, speed=5.0, gravity=1.0, size=0.25, life=1.1),
                 smoke=dict(count=5, color='#b89a72', size=1.3, rise=0.4, life=1.5),
                 light=1.5, shake=0.20, hit_stop=0.08),
    'light': dict(sparks=(20, 5.0, -0.1, 2.0, 0.06, 0.7), debris=None, smoke=None,
                  light=5.0, shake=0.06, hit_stop=0.04),
    'dark': dict(sparks=(16, 5.0, 0.0, 3.0, 0.07, 0.6), debris=None, smoke=None,
                 light=3.0, shake=0.10, hit_stop=0.06),
}

ELEMENT_WORDS = {
    'water': ['water', 'aqua', 'splash', 'wave', 'hydro', 'bubble', 'rain', 'sea'],
    'fire': ['fire', 'flame', 'burn', 'fireball', 'blaze', 'ember', 'heat', 'pyro', 'inferno'],
    'thunder': ['thunder', 'lightning', 'electric', 'shock', 'zap', 'spark', 'storm', 'volt', 'bolt'],
    'wind': ['wind', 'air', 'gust', 'tornado', 'cyclone', 'whirlwind', 'breeze', 'aero'],
    'poison': ['poison', 'toxic', 'venom', 'acid', 'plague', 'miasma', 'corrosive', 'sludge'],
    'ice': ['ice', 'frost', 'freeze', 'frozen', 'snow', 'cold', 'cryo', 'glacier', 'chill'],
    'rock': ['rock', 'earth', 'stone', 'ground', 'quake', 'boulder', 'geo', 'terra', 'crystal'],
    'light': ['light', 'holy', 'divine', 'radiant', 'sacred', 'heal', 'sun', 'angel', 'bless'],
    'dark': ['dark', 'shadow', 'void', 'curse', 'abyss', 'necro', 'demon', 'evil', 'hex'],
}

# Per category: default world size (height of the main quad, m), anchor, orientation and how much of each
# secondary layer it gets (1 = full element default, 0 = off).
CATEGORY = {
    'projectile': dict(size=1.0, anchor='center', orient='billboard', flash=0, ring=0, sparks=0, debris=0, smoke=0,
                       decal=False, light=0.6, shake=0, hit=0, spark_dir='radial', ring_plane='view'),
    'impact': dict(size=2.0, anchor='center', orient='billboard', flash=1.0, ring=1.0, sparks=1.0, debris=1.0,
                   smoke=1.0, decal=True, light=1.0, shake=1.0, hit=1.0, spark_dir='radial', ring_plane='view'),
    'column': dict(size=3.2, anchor='ground', orient='upright', flash=0.6, ring=1.3, sparks=1.0, debris=1.2,
                   smoke=1.0, decal=True, light=1.3, shake=1.2, hit=0.0, spark_dir='up', ring_plane='ground'),
    'strike': dict(size=4.0, anchor='ground', orient='upright', flash=1.4, ring=1.3, sparks=1.3, debris=1.0,
                   smoke=0.8, decal=True, light=2.0, shake=1.6, hit=1.0, spark_dir='radial', ring_plane='ground'),
    'vortex': dict(size=2.8, anchor='center', orient='billboard', flash=0.0, ring=0.8, sparks=0.8, debris=0.5,
                   smoke=0.5, decal=True, light=0.8, shake=0.3, hit=0.0, spark_dir='swirl', ring_plane='ground'),
    'area': dict(size=3.5, anchor='ground', orient='ground', flash=0.4, ring=1.2, sparks=0.5, debris=0.5,
                 smoke=1.0, decal=True, light=0.8, shake=0.3, hit=0.0, spark_dir='up', ring_plane='ground'),
    'beam': dict(size=4.0, anchor='center', orient='travel', flash=0.5, ring=0.0, sparks=0.6, debris=0.0,
                 smoke=0.0, decal=False, light=1.2, shake=0.2, hit=0.0, spark_dir='forward', ring_plane='view'),
    'buff': dict(size=2.0, anchor='center', orient='billboard', flash=0.5, ring=0.8, sparks=0.6, debris=0.0,
                 smoke=0.0, decal=False, light=0.7, shake=0.0, hit=0.0, spark_dir='rise', ring_plane='ground'),
    'shield': dict(size=2.6, anchor='center', orient='billboard', flash=0.5, ring=1.0, sparks=0.3, debris=0.0,
                   smoke=0.0, decal=False, light=0.6, shake=0.0, hit=0.0, spark_dir='rise', ring_plane='ground'),
    'cast': dict(size=1.4, anchor='center', orient='billboard', flash=0.6, ring=0.6, sparks=0.5, debris=0.0,
                 smoke=0.0, decal=False, light=0.8, shake=0.0, hit=0.0, spark_dir='radial', ring_plane='view'),
    'slash': dict(size=2.4, anchor='center', orient='billboard', flash=0.8, ring=0.0, sparks=1.0, debris=0.0,
                  smoke=0.0, decal=False, light=0.7, shake=0.6, hit=1.0, spark_dir='forward', ring_plane='view'),
    'aura': dict(size=2.4, anchor='ground', orient='upright', flash=0.0, ring=0.6, sparks=0.6, debris=0.0,
                 smoke=0.0, decal=False, light=0.6, shake=0.0, hit=0.0, spark_dir='rise', ring_plane='ground'),
}

CATEGORY_USES = {
    'projectile': ['spell projectile', 'magic bolt', 'ranged attack', 'bullet', 'tower shot'],
    'impact': ['hit effect', 'projectile impact', 'melee hit', 'explosion', 'on-hit feedback'],
    'area': ['ground AoE', 'hazard zone', 'debuff field'],
    'column': ['ground eruption', 'AoE pillar', 'boss attack', 'summon'],
    'strike': ['strike from the sky', 'targeted AoE', 'ultimate'],
    'beam': ['continuous beam', 'chain attack', 'laser'],
    'vortex': ['whirlwind', 'pull AoE', 'crowd control'],
    'buff': ['buff gained', 'power up', 'pickup'],
    'shield': ['barrier', 'protection bubble', 'block', 'invulnerable'],
    'cast': ['cast', 'charge-up', 'muzzle flash', 'spell start'],
    'slash': ['melee swing', 'sword slash', 'claw attack', 'combo hit'],
    'aura': ['status aura', 'power-up loop', 'buff loop', 'enrage'],
}

# What each element/category pair is usually called in a game.
ELEMENT_USES = {
    ('water', 'impact'): ['splash hit'], ('water', 'column'): ['geyser', 'water pillar'],
    ('fire', 'projectile'): ['fireball'], ('fire', 'impact'): ['fire explosion', 'burn hit'],
    ('thunder', 'impact'): ['shock hit', 'stun hit'], ('thunder', 'strike'): ['lightning strike', 'thunderbolt'],
    ('wind', 'vortex'): ['tornado', 'whirlwind'], ('wind', 'impact'): ['wind slash hit', 'gust hit'],
    ('poison', 'projectile'): ['poison spit', 'venom bolt'], ('poison', 'impact'): ['poison splash', 'toxic hit'],
    ('ice', 'projectile'): ['ice shard', 'frost bolt', 'ice arrow'], ('ice', 'impact'): ['ice shatter', 'freeze hit'],
    ('rock', 'column'): ['earth spike', 'rock pillar'], ('rock', 'impact'): ['rock smash', 'ground pound'],
    ('light', 'column'): ['holy pillar', 'heal beam'], ('light', 'impact'): ['holy burst', 'smite'],
    ('dark', 'projectile'): ['shadow bolt', 'dark orb'], ('dark', 'impact'): ['shadow burst', 'curse hit'],
}

# Style B (_02) and C (_03) variants. color tints every secondary layer; fx entries replace layer settings.
STYLES = {
    'classic': dict(color=None, tags=[]),
    'tidal': dict(color='#2f9fd8', tags=['heavy', 'boss', 'wave']),
    'spring': dict(color='#7ff0d6', tags=['heal', 'support', 'bubble'], fx=dict(shake=0, hit_stop=0)),
    'soulfire': dict(color='#5c8cff', tags=['magic', 'undead', 'blue fire', 'spirit']),
    'meteor': dict(color='#ff5a1f', tags=['heavy', 'bomb', 'meteor', 'cannonball'],
                   fx=dict(debris=dict(count=10, speed=6.0, gravity=1.0, size=0.22, life=1.1, color='#3a2a26'),
                           smoke=dict(count=7, color='#3e3836', size=1.5, rise=1.0, life=1.8), shake_mul=1.5)),
    'raijin': dict(color='#ffd23f', tags=['gold', 'barrage', 'god']),
    'plasma': dict(color='#3fd0ff', tags=['stun', 'chain', 'electric field', 'sci-fi']),
    'tornado': dict(color='#3fcfae', tags=['tornado', 'multi hit', 'shuriken']),
    'sakura': dict(color='#ff8fc8', tags=['petal', 'cherry blossom', 'crit', 'elegant'],
                   fx=dict(sparks_override=dict(gravity=0.15, drag=1.5, size=0.09, life=1.2))),
    'acid': dict(color='#c8f03c', tags=['acid', 'puddle', 'corrosive', 'hazard'], fx=dict(smoke=None)),
    'miasma': dict(color='#c05ce0', tags=['curse', 'spore', 'cloud', 'debuff'],
                   fx=dict(smoke=dict(count=6, color='#6b3a7a', size=1.5, rise=0.25, life=2.0))),
    'glacier': dict(color='#5fb8ff', tags=['lance', 'nova', 'spike', 'heavy'],
                    fx=dict(debris=dict(count=12, speed=5.0, gravity=1.0, size=0.2, life=1.0))),
    'snow': dict(color='#d8d0ff', tags=['snow', 'slow', 'root', 'soft'],
                 fx=dict(sparks_override=dict(gravity=0.25, drag=1.2, size=0.08, life=1.1))),
    'magma': dict(color='#ff6a1a', tags=['lava', 'magma', 'ember', 'volcano'],
                  fx=dict(smoke=dict(count=5, color='#3a3030', size=1.3, rise=0.8, life=1.6),
                          sparks_override=dict(gravity=-0.1, life=1.0))),
    'stone': dict(color='#c8975a', tags=['fissure', 'line', 'crater', 'slam', 'heavy'], fx=dict(shake_mul=1.4)),
    'blessing': dict(color='#b8f0a0', tags=['heal', 'resurrect', 'support', 'blessing'],
                     fx=dict(shake=0, hit_stop=0, sparks_override=dict(gravity=-0.2, life=1.1))),
    'judgement': dict(color='#ffc23a', tags=['holy', 'ultimate', 'nova', 'judgement'], fx=dict(shake_mul=1.3)),
    'void': dict(color='#7a6cff', tags=['void', 'black hole', 'implosion', 'gravity'],
                 fx=dict(sparks_override=dict(speed=-4.0, drag=0.5, gravity=0.0))),
    'crimson': dict(color='#e0284a', tags=['blood', 'curse', 'crimson', 'debuff']),
}

# Variant sheets (id -> style, category, orientation, what it is for). Same naming rule: _02 = style B, _03 = C.
VARIANTS = {
    'water_column_02': ('tidal', 'column', 'upright', 'big AoE / boss geyser'),
    'water_impact_02': ('tidal', 'impact', 'billboard', 'wave crash / heavy water hit'),
    'water_column_03': ('spring', 'column', 'upright', 'heal-over-time fountain'),
    'water_impact_03': ('spring', 'buff', 'billboard', 'heal pickup / bubbles rise and pop'),
    'fire_projectile_02': ('soulfire', 'projectile', 'travel', 'magic fireball / undead fireball'),
    'fire_impact_02': ('soulfire', 'impact', 'billboard', 'magic fire hit / soul wisps rise'),
    'fire_projectile_03': ('meteor', 'projectile', 'travel', 'meteor / cannonball'),
    'fire_impact_03': ('meteor', 'impact', 'billboard', 'large explosion / bomb'),
    'thunder_strike_02': ('raijin', 'strike', 'upright', 'multi-bolt barrage AoE'),
    'thunder_impact_02': ('raijin', 'impact', 'billboard', 'shock hit / shockwave'),
    'thunder_strike_03': ('plasma', 'beam', 'travel', 'lightning beam / zap segment'),
    'thunder_impact_03': ('plasma', 'area', 'billboard', 'stun field'),
    'wind_vortex_02': ('tornado', 'vortex', 'upright', 'tornado / pull AoE'),
    'wind_impact_02': ('tornado', 'impact', 'billboard', 'multi-slash hit / shuriken'),
    'wind_vortex_03': ('sakura', 'cast', 'billboard', 'charge-up gather / buff'),
    'wind_impact_03': ('sakura', 'impact', 'billboard', 'crit hit / sword-style finisher'),
    'poison_projectile_02': ('acid', 'projectile', 'travel', 'acid spit'),
    'poison_impact_02': ('acid', 'area', 'ground', 'acid puddle hazard'),
    'poison_projectile_03': ('miasma', 'projectile', 'travel', 'spore bolt / curse bolt'),
    'poison_impact_03': ('miasma', 'area', 'billboard', 'toxic cloud debuff area'),
    'ice_projectile_02': ('glacier', 'projectile', 'travel', 'ice lance / ice arrow'),
    'ice_impact_02': ('glacier', 'area', 'billboard', 'frost nova / ring of ice spikes'),
    'ice_projectile_03': ('snow', 'projectile', 'billboard', 'snowball / slow bolt'),
    'ice_impact_03': ('snow', 'area', 'ground', 'freeze / root sigil'),
    'rock_column_02': ('magma', 'column', 'upright', 'lava spike'),
    'rock_impact_02': ('magma', 'impact', 'billboard', 'magma burst hit'),
    'rock_column_03': ('stone', 'area', 'upright', 'fissure / line AoE'),
    'rock_impact_03': ('stone', 'area', 'billboard', 'ground slam crater'),
    'light_column_02': ('blessing', 'column', 'upright', 'heal / resurrection beam'),
    'light_impact_02': ('blessing', 'buff', 'billboard', 'heal pickup / buff gain'),
    'light_column_03': ('judgement', 'column', 'upright', 'holy AoE / ultimate'),
    'light_impact_03': ('judgement', 'impact', 'billboard', 'radiant nova / crit'),
    'dark_projectile_02': ('void', 'projectile', 'billboard', 'black-hole orb'),
    'dark_impact_02': ('void', 'impact', 'billboard', 'implosion: pull, then burst'),
    'dark_projectile_03': ('crimson', 'projectile', 'travel', 'curse bolt'),
    'dark_impact_03': ('crimson', 'area', 'ground', 'curse circle debuff'),
}

# Forms that are not a category name themselves.
FORM_CATEGORY = {'charge': 'cast', 'line': 'area'}

# Rare hand tuning for one sheet: recipe fields or fx layers to replace after everything else.
ASSET_OVERRIDES = {
    'poison_impact_02': dict(loop_range=[40, 63]),           # splash once, then the puddle loops
    'poison_impact_03': dict(loop_range=[30, 63]),           # burst once, then the cloud loops
    'wind_vortex_01': dict(anchor='ground'),                 # a tornado stands on the floor
    'wind_vortex_02': dict(anchor='ground'),
}

# Shared sound effects (assets/_shared/sfx/). Engines must tolerate a missing file.
SFX_EXT = '.wav'


def sfx_name(stem):
    return stem + SFX_EXT


# ------------------------------------------------------------------ helpers
def _rgb(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def _hex(rgb):
    return '#' + ''.join(f'{max(0, min(255, round(c))):02x}' for c in rgb)


def mix(a, b, t):
    """Blend two hex colours (t = 0 -> a, 1 -> b)."""
    ca, cb = _rgb(a), _rgb(b)
    return _hex(tuple(x + (y - x) * t for x, y in zip(ca, cb)))


def _r(v, n=3):
    return round(float(v), n)


def id_parts(asset_id):
    m = re.match(r'^([a-z]+)_([a-z]+)_(\d{2})$', asset_id)
    return (m.group(1), m.group(2), int(m.group(3))) if m else (None, None, None)


def default_category(form):
    form = FORM_CATEGORY.get(form, form)
    return form if form in CATEGORIES else 'impact'


def category_of(asset_id):
    v = VARIANTS.get(asset_id)
    return v[1] if v else default_category(id_parts(asset_id)[1] or '')


def contact_from_coverage(coverage, loop=False):
    """The frame where the shape first reaches half its biggest size: the hit lands there.
    `coverage` is the opaque area per frame (images.coverage)."""
    if loop or not coverage:
        return 0
    peak = max(coverage[:max(3, int(len(coverage) * 0.75))])
    if peak <= 0:
        return 0
    return next(i for i, c in enumerate(coverage) if c >= 0.5 * peak)


# ------------------------------------------------------------------ recipe
def _identity(entry):
    aid = entry['id']
    element, form = entry['element'], entry['form']
    v = VARIANTS.get(aid)
    if v:
        style, category, orient, use = v
    else:
        style, category, orient, use = 'classic', default_category(form), None, None
    return element, form, style, category, orient, use


def _pairs(entry, category, style, ids):
    """Which effects go with this one: the cast (charge) in front of it, the projectile that carries it,
    the impact after it. Same style first, then the classic version of the same element."""
    element = entry['element']
    ids = sorted(set(ids or ()) - {entry['id']})

    def pick(cat):
        cands = [i for i in ids if id_parts(i)[0] == element and category_of(i) == cat]
        same = [i for i in cands if _style_of(i) == style]
        classic = [i for i in cands if _style_of(i) == 'classic']
        return (same or classic or [None])[0]

    wants = {
        'projectile': ('cast', 'impact'), 'beam': ('cast', 'impact'), 'impact': ('projectile',),
        'column': ('cast',), 'strike': ('cast',), 'vortex': ('cast',), 'area': ('cast',),
        'shield': ('cast',), 'aura': ('cast',), 'buff': ('cast',), 'cast': ('projectile',), 'slash': (),
    }[category]
    return {k: (pick(k) if k in wants else None) for k in ('cast', 'projectile', 'impact')}


def _style_of(aid):
    v = VARIANTS.get(aid)
    return v[0] if v else 'classic'


def _sfx(element, category):
    cast, loop, impact = sfx_name(f'{element}_cast'), sfx_name(f'{element}_travel_loop'), sfx_name(f'{element}_impact')
    return {
        'projectile': {'cast': cast, 'loop': loop, 'impact': None},
        'impact': {'cast': None, 'loop': None, 'impact': impact},
        'column': {'cast': cast, 'loop': None, 'impact': impact},
        'strike': {'cast': cast, 'loop': None, 'impact': impact},
        'vortex': {'cast': cast, 'loop': loop, 'impact': None},
        'area': {'cast': cast, 'loop': None, 'impact': impact},
        'beam': {'cast': cast, 'loop': loop, 'impact': None},
        'buff': {'cast': None, 'loop': None, 'impact': sfx_name('charge')},
        'shield': {'cast': sfx_name('charge'), 'loop': sfx_name('shield_loop'), 'impact': None},
        'cast': {'cast': sfx_name('charge'), 'loop': None, 'impact': None},
        'slash': {'cast': None, 'loop': None, 'impact': sfx_name('slash')},
        'aura': {'cast': sfx_name('charge'), 'loop': sfx_name('aura_loop'), 'impact': None},
    }[category]


def _fx(element, category, style, size, loop, duration):
    base = ELEMENT_FX[element]
    cat = CATEGORY[category]
    st = STYLES.get(style, STYLES['classic'])
    sfx_over = st.get('fx', {})
    color = st.get('color') or ELEMENT_COLORS[element]
    k = size / 2.0                                         # proportions were tuned on a 2 m impact
    fx = {}

    # flash: a 2-3 frame white-hot star at the contact point
    fx['flash'] = ({'color': mix(color, '#ffffff', 0.65), 'size': _r(size * 0.75 * min(cat['flash'], 1.4), 2),
                    'frames': 3 if cat['flash'] >= 1 else 2} if cat['flash'] > 0 else None)
    # ring: a thin shockwave, facing the camera for hits, flat on the floor for ground effects
    fx['ring'] = ({'color': mix(color, '#ffffff', 0.35), 'size': _r(size * 1.1 * cat['ring'], 2),
                   'duration': _r(0.3 + 0.1 * cat['ring'], 2), 'plane': cat['ring_plane']}
                  if cat['ring'] > 0 else None)
    # sparks: stretched streaks, launched outward, slowed by drag, pulled by gravity
    cnt, spd, grav, drag, ssz, life = base['sparks']
    so = dict(sfx_over.get('sparks_override', {}))
    if cat['sparks'] > 0:
        fx['sparks'] = {'count': max(1, round(cnt * cat['sparks'])), 'speed': _r(so.get('speed', spd) * k ** 0.5, 2),
                        'color': mix(color, '#ffffff', 0.2), 'gravity': _r(so.get('gravity', grav), 2),
                        'drag': _r(so.get('drag', drag), 2), 'size': _r(so.get('size', ssz) * k ** 0.5, 3),
                        'life': _r(so.get('life', life), 2), 'direction': cat['spark_dir']}
        if cat['spark_dir'] == 'rise':
            fx['sparks']['gravity'] = _r(-abs(fx['sparks']['gravity']) - 0.15, 2)
    else:
        fx['sparks'] = None
    # debris: chunky tumbling bits with real gravity (rock, ice, meteor)
    deb = sfx_over['debris'] if 'debris' in sfx_over else base['debris']
    if deb and cat['debris'] > 0:
        fx['debris'] = {'count': max(1, round(deb['count'] * cat['debris'])), 'speed': _r(deb['speed'] * k ** 0.5, 2),
                        'color': deb.get('color', mix(color, '#ffffff', 0.15)), 'gravity': _r(deb['gravity'], 2),
                        'size': _r(deb['size'] * k ** 0.5, 3), 'life': _r(deb['life'], 2)}
    else:
        fx['debris'] = None
    # smoke: a few cel puffs that rise and fade (fire, rock, poison)
    smk = sfx_over['smoke'] if 'smoke' in sfx_over else base['smoke']
    if smk and cat['smoke'] > 0:
        fx['smoke'] = {'count': max(1, round(smk['count'] * cat['smoke'])), 'color': smk['color'],
                       'size': _r(smk['size'] * k, 2), 'rise': _r(smk['rise'], 2), 'life': _r(smk['life'], 2),
                       'delay': 0.08}
    else:
        fx['smoke'] = None
    # ground decal: scorch / puddle / frost under the effect (the decal art is coloured already), fades out slowly
    fx['decal'] = ({'texture': f'decal_{element}.png', 'size': _r(size * (0.9 if category == 'impact' else 1.1), 2),
                    'life': 3.0 if category in ('impact', 'strike') else _r(max(3.0, duration + 1.5), 2),
                    'tint': '#ffffff' if style == 'classic' else mix('#ffffff', color, 0.3)}
                   if cat['decal'] else None)
    # light: element-coloured point light that lights the floor and the target
    li = base['light'] * cat['light']
    # curve: flat pairs (seconds after the contact frame, intensity multiplier), linear in between, clamped at
    # both ends (flat so every engine's JSON reader takes it; loops and projectiles hold their last value)
    if category in ('projectile', 'shield', 'aura') or loop:
        curve = [0.0, 1.0]
    elif category in ('column', 'area', 'vortex'):
        curve = [-0.1, 0.0, 0.0, 1.0, _r(duration * 0.5, 2), 0.6, _r(duration, 2), 0.0]
    else:
        curve = [0.0, 1.0, 0.08, 0.7, 0.35, 0.15, 0.6, 0.0]
    fx['light'] = {'color': color, 'intensity': _r(li, 2), 'range': _r(size * 2.2, 2), 'curve': curve}
    # trail: what a projectile leaves behind as it flies
    fx['trail'] = ({'color': mix(color, '#ffffff', 0.15), 'rate': 14, 'size': _r(size * 0.5, 2), 'life': 0.35}
                   if category in ('projectile',) else None)
    shake_mul = sfx_over.get('shake_mul', 1.0)
    amp = base['shake'] * cat['shake'] * shake_mul * k ** 0.5
    fx['shake'] = ({'amplitude': _r(amp, 3), 'duration': _r(0.18 + 0.6 * amp, 2)} if amp > 0.001 else None)
    fx['hit_stop_s'] = _r(base['hit_stop'] * cat['hit'], 3)
    if sfx_over.get('shake') == 0:
        fx['shake'] = None
    if sfx_over.get('hit_stop') == 0:
        fx['hit_stop_s'] = 0.0
    fx['sfx'] = _sfx(element, category)
    return fx


def recipe(entry, ids=(), contact=None):
    """Recipe fields for one catalog entry. `ids` are all published ids (for pairs); `contact` is the contact
    frame measured from the sheet (None keeps the entry's current value)."""
    element, form, style, category, orient, use = _identity(entry)
    cat = CATEGORY[category]
    frames, fps = int(entry.get('frames') or 1), float(entry.get('fps') or 30)
    loop = bool(entry.get('loop'))
    duration = _r(frames / fps, 2)
    size = cat['size']
    if style in ('tidal', 'meteor', 'judgement') and category in ('impact', 'column'):
        size *= 1.35
    if category == 'area' and orient == 'upright':           # a line of spikes reads wider than tall
        size = 3.2
    orientation = orient or cat['orient']
    anchor = 'ground' if orientation in ('upright', 'ground') else cat['anchor']
    if contact is None:
        contact = entry.get('contact_frame', 0)
    contact = 0 if category == 'projectile' else int(max(0, min(contact, frames - 1)))
    words = ELEMENT_WORDS[element]
    tags = sorted({element, category, form, style} | set(words[:3]) | set(STYLES.get(style, {}).get('tags', [])))
    uses = list(ELEMENT_USES.get((element, category), []))
    if use:
        uses = [u.strip() for u in use.split('/') if u.strip()] + uses
    uses += [u for u in CATEGORY_USES[category] if u not in uses]
    out = {
        'category': category,
        'style': style,
        'base_id': f'{element}_{form}_01',
        'anchor': anchor,
        'orientation': orientation,
        'world_size_m': _r(size, 2),
        'duration_s': duration,
        'contact_frame': contact,
        'loop_range': [0, frames - 1] if loop else None,
        'pairs': _pairs(entry, category, style, ids),
        'tags': tags,
        'use_cases': uses[:8],
        'fx': _fx(element, category, style, size, loop, duration),
    }
    for k, v in ASSET_OVERRIDES.get(entry['id'], {}).items():
        if k in out['fx']:
            out['fx'][k] = v
        else:
            out[k] = v
    return out


def check(entry):
    """Shape errors in an entry's recipe fields (values are checked against `recipe()` by validate)."""
    aid = entry.get('id', '?')
    errs = []
    if entry.get('category') not in CATEGORIES:
        errs.append(f'{aid}: category must be one of {", ".join(CATEGORIES)}')
    if entry.get('anchor') not in ANCHORS:
        errs.append(f'{aid}: anchor must be center or ground')
    if entry.get('orientation') not in ORIENTATIONS:
        errs.append(f'{aid}: orientation must be one of {", ".join(ORIENTATIONS)}')
    fx = entry.get('fx')
    if not isinstance(fx, dict) or set(fx) != set(FX_LAYERS):
        errs.append(f'{aid}: fx must have the layers {", ".join(FX_LAYERS)}')
    return errs
