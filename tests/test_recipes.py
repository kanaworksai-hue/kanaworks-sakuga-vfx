"""Recipes (layered effect data), assets/_shared and `suggest`."""
import json
import re
import shutil
import struct
import subprocess
import sys
import wave

import pytest

from conftest import ROOT, sheet
from sakugakit import recipes, shared
from sakugakit.catalog import add, sync, validate
from sakugakit.common import format_catalog, load_catalog, published, read_json, sidecar
from sakugakit.suggest import suggest

HEX = re.compile(r'^#[0-9a-f]{6}$')


def _colours(obj):
    if isinstance(obj, dict):
        for k, v in obj.items():
            if k in ('color', 'tint'):
                yield v
            else:
                yield from _colours(v)
    elif isinstance(obj, list):
        for v in obj:
            yield from _colours(v)


def test_every_repository_asset_has_a_complete_recipe():
    cat = load_catalog(ROOT / 'catalog.json')
    ids = {a['id'] for a in published(cat)}
    for a in published(cat):
        assert recipes.check(a) == [], a['id']
        assert a['category'] in recipes.CATEGORIES and a['style'] in recipes.STYLES
        assert a['world_size_m'] > 0 and a['duration_s'] == round(a['frames'] / a['fps'], 2)
        assert 0 <= a['contact_frame'] < a['frames']
        assert a['use_cases'] and a['tags'] and a['element'] in a['tags']
        assert all(v is None or v in ids for v in a['pairs'].values()), a['id']
        for c in _colours(a['fx']):
            assert HEX.match(c), (a['id'], c)
        fx = a['fx']
        assert fx['light']['color'] == (recipes.STYLES[a['style']]['color'] or recipes.ELEMENT_COLORS[a['element']])
        assert all(s is None or s.endswith('.wav') for s in fx['sfx'].values())
        if a['category'] == 'impact' and a['style'] not in ('spring', 'blessing'):
            assert fx['flash'] and fx['ring'] and fx['sparks'] and fx['decal'] and fx['shake'] and fx['hit_stop_s'] > 0
        if a['category'] == 'projectile':
            assert fx['trail'] and a['pairs']['impact'] and a['pairs']['cast'] and a['loop_range'] == [0, a['frames'] - 1]
        if a['anchor'] == 'ground':
            assert a['category'] in ('column', 'strike', 'area', 'aura', 'vortex')
    assert {a['category'] for a in published(cat)} == set(recipes.CATEGORIES)
    assert len(published(cat)) >= 90


def test_variants_and_category_sheets_are_wired():
    cat = {a['id']: a for a in load_catalog(ROOT / 'catalog.json')['assets']}
    assert cat['fire_projectile_02']['pairs'] == {'cast': 'fire_charge_01', 'projectile': None, 'impact': 'fire_impact_02'}
    assert cat['fire_charge_01']['category'] == 'cast' and cat['fire_charge_01']['pairs']['projectile'] == 'fire_projectile_01'
    assert cat['poison_impact_02']['loop_range'] == [40, 63] and cat['poison_impact_03']['loop_range'] == [30, 63]
    assert cat['dark_impact_03']['orientation'] == 'ground' and cat['ice_projectile_02']['orientation'] == 'travel'
    assert cat['thunder_strike_03']['category'] == 'beam' and cat['thunder_strike_03']['anchor'] == 'center'
    assert cat['water_column_02']['cell'] == [320, 400] and cat['rock_column_03']['cell'] == [384, 256]
    for el in recipes.ELEMENT_COLORS:
        assert cat[f'{el}_shield_01']['loop'] and cat[f'{el}_aura_01']['anchor'] == 'ground'
        assert cat[f'{el}_slash_01']['category'] == 'slash' and cat[f'{el}_slash_01']['fx']['sfx']['impact'] == 'slash.wav'


def test_element_layers_differ_where_the_brief_says():
    cat = {a['id']: a for a in load_catalog(ROOT / 'catalog.json')['assets']}
    assert cat['rock_impact_01']['fx']['debris'] and cat['rock_impact_01']['fx']['smoke']
    assert cat['ice_impact_01']['fx']['debris'] and not cat['ice_impact_01']['fx']['smoke']
    assert cat['fire_impact_01']['fx']['smoke'] and cat['poison_impact_01']['fx']['smoke']
    assert not cat['water_impact_01']['fx']['debris']
    assert cat['thunder_strike_01']['fx']['shake']['amplitude'] > cat['wind_impact_01']['fx']['shake']['amplitude']
    assert cat['fire_projectile_01']['pairs']['impact'] == 'fire_impact_01'
    assert cat['fire_impact_01']['pairs']['projectile'] == 'fire_projectile_01'
    assert cat['rock_impact_01']['contact_frame'] == 0 and cat['light_column_01']['contact_frame'] > 2


def test_recipes_are_in_the_sidecars_and_catalog_blocks():
    cat = load_catalog(ROOT / 'catalog.json')
    text = (ROOT / 'catalog.json').read_text(encoding='utf-8')
    assert format_catalog(cat) == text
    for a in published(cat):
        s = read_json(ROOT / 'assets' / a['element'] / a['id'] / f"{a['id']}.flipbook.json")
        assert s == sidecar(a, cat['defaults'])
        for k in recipes.RECIPE_FIELDS:
            assert s[k] == a[k], (a['id'], k)


def test_contact_frame_rule():
    assert recipes.contact_from_coverage([0.02, 0.05, 0.2, 0.3, 0.3, 0.2]) == 2
    assert recipes.contact_from_coverage([0.3, 0.2, 0.1, 0.05]) == 0          # starts big: contact at once
    assert recipes.contact_from_coverage([0.1, 0.2, 0.3], loop=True) == 0
    assert recipes.contact_from_coverage([]) == 0


def test_mix_and_size_presets():
    assert recipes.mix('#000000', '#ffffff', 0.5) == '#808080'
    assert recipes.mix('#ff7d2e', '#ff7d2e', 0.3) == '#ff7d2e'
    assert recipes.SIZE_PRESETS == {'S': 0.6, 'M': 1.0, 'L': 1.6}


@pytest.mark.parametrize('aid,grid,loop,category,style,orientation', [
    ('fire_impact_03', (4, 2), None, 'impact', 'meteor', 'billboard'),
    ('ice_projectile_02', (4, 2), None, 'projectile', 'glacier', 'travel'),
    ('dark_impact_03', (4, 2), True, 'area', 'crimson', 'ground'),
    ('thunder_strike_03', (4, 2), True, 'beam', 'plasma', 'travel'),
    ('water_shield_01', (4, 2), None, 'shield', 'classic', 'billboard'),
    ('fire_cast_01', (4, 2), None, 'cast', 'classic', 'billboard'),
    ('wind_slash_01', (4, 2), None, 'slash', 'classic', 'billboard'),
    ('light_aura_01', (4, 2), None, 'aura', 'classic', 'upright'),
])
def test_new_sheets_get_recipes_automatically(library, tmp_path, aid, grid, loop, category, style, orientation):
    src = tmp_path / 'x.png'
    sheet(rooted=orientation == 'upright').save(src)
    add(src, aid, grid, root=library, added='2026-01-02', loop=loop)
    a = load_catalog(library / 'catalog.json')['assets'][0]
    assert (a['category'], a['style'], a['orientation']) == (category, style, orientation)
    assert recipes.check(a) == []
    element = aid.split('_')[0]
    if style != 'classic':
        assert a['fx']['ring'] is None or a['fx']['ring']['color'] != recipes.mix(recipes.ELEMENT_COLORS[element],
                                                                                   '#ffffff', 0.35)
        assert a['base_id'] == aid[:-2] + '01'
    assert validate(library)[0] == []


def test_pairs_follow_the_library_as_it_grows(library, tmp_path):
    src = tmp_path / 'x.png'
    sheet().save(src)
    add(src, 'ice_projectile_01', (4, 2), root=library, added='2026-01-02')
    first = load_catalog(library / 'catalog.json')['assets'][0]
    assert first['pairs']['impact'] is None
    add(src, 'ice_impact_01', (4, 2), root=library, added='2026-01-02')
    add(src, 'ice_cast_01', (4, 2), root=library, added='2026-01-02')
    by = {a['id']: a for a in load_catalog(library / 'catalog.json')['assets']}
    assert by['ice_projectile_01']['pairs'] == {'cast': 'ice_cast_01', 'projectile': None, 'impact': 'ice_impact_01'}
    assert by['ice_impact_01']['pairs']['projectile'] == 'ice_projectile_01'
    assert validate(library)[0] == []


def test_validate_catches_a_hand_edited_recipe(two_assets):
    cat = load_catalog(two_assets / 'catalog.json')
    cat['assets'][0]['fx']['shake'] = {'amplitude': 9, 'duration': 9}
    (two_assets / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    assert any('recipe fields' in e for e in validate(two_assets)[0])
    sync(two_assets)
    assert validate(two_assets)[0] == []


def test_every_variant_and_style_is_consistent():
    for aid, (style, category, orient, use) in recipes.VARIANTS.items():
        element, form, n = recipes.id_parts(aid)
        assert element in recipes.ELEMENT_COLORS and n in (2, 3), aid
        assert style in recipes.STYLES and category in recipes.CATEGORIES and orient in recipes.ORIENTATIONS
        assert use and use.isascii()
    for name, st in recipes.STYLES.items():
        assert st['color'] is None or HEX.match(st['color']), name


# ------------------------------------------------------------------ shared
def _wav(path, seconds=0.1, tags=False):
    with wave.open(str(path), 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(8000)
        w.writeframes(b'\0\0' * int(8000 * seconds))
    if tags:
        data = path.read_bytes() + b'LIST' + struct.pack('<I', 4) + b'INFO'
        path.write_bytes(data[:4] + struct.pack('<I', len(data) - 8) + data[8:])


def test_shared_import_strips_tags_and_writes_the_manifest(library, tmp_path):
    src = tmp_path / 'src'
    (src / 'sfx').mkdir(parents=True)
    for n in shared.TEXTURES:
        sheet(1, 1).save(src / f'{n}.png')
    sheet(1, 1).save(src / 'decal_fire.png')
    _wav(src / 'sfx' / 'fire_impact.wav', tags=True)
    assert shared.wav_extra_chunks((src / 'sfx' / 'fire_impact.wav').read_bytes()) == ['LIST']
    done = shared.import_shared(src, library)
    assert 'sfx/fire_impact.wav' in done and 'decals/decal_fire.png' in done
    d = library / 'assets' / '_shared'
    assert shared.wav_extra_chunks((d / 'sfx' / 'fire_impact.wav').read_bytes()) == []
    m = read_json(d / 'shared.json')
    assert set(m['textures']) == set(shared.TEXTURES) and m['decals'] == {'decal_fire': 'decals/decal_fire.png'}
    assert m['sfx']['fire_impact.wav']['seconds'] == 0.1
    errors, warnings = shared.check(library)
    assert errors == [] and warnings                       # most sounds are missing: a warning, not an error
    assert validate(library)[0] == []                       # the _shared folder is not an asset folder
    (d / 'notes.txt').write_text('x', encoding='utf-8')
    assert shared.check(library)[0]


def test_repository_shared_folder_is_clean():
    errors, _ = shared.check(ROOT)
    assert errors == []
    m = read_json(ROOT / 'assets/_shared/shared.json')
    for name in ('T_Flash', 'T_Ring', 'T_Spark', 'T_Glow', 'T_Smoke_2x2', 'T_Debris_2x2'):
        assert name in m['textures']
    cat = load_catalog(ROOT / 'catalog.json')
    for a in published(cat):                                # every sound a recipe names is a known shared name
        for s in a['fx']['sfx'].values():
            assert s is None or s[:-4] in shared.SFX_NAMES


# ------------------------------------------------------------------ suggest
@pytest.mark.parametrize('text,tops,chain_has', [
    ('ice arrow hitting a slime', {'ice_projectile_01', 'ice_projectile_02'}, ['ice_charge_01', 'ice_impact_01']),
    ('fireball', {'fire_projectile_01'}, ['fire_charge_01', 'fire_projectile_01', 'fire_impact_01']),
    ('lightning from the sky', {'thunder_strike_01', 'thunder_strike_02'}, []),
    ('tornado that pulls enemies', {'wind_vortex_01', 'wind_vortex_02'}, []),
    ('big rock smash', {'rock_impact_01'}, []),
    ('meteor', {'fire_projectile_03'}, ['fire_impact_03']),
    ('heal the party', {'light_impact_02', 'light_column_02', 'water_impact_03'}, []),
    ('sword slash', {f'{e}_slash_01' for e in recipes.ELEMENT_COLORS}, []),
])
def test_suggest_ranks_the_obvious_effect_first(text, tops, chain_has):
    ranked, chain = suggest(load_catalog(ROOT / 'catalog.json'), text)
    assert ranked[0][1]['id'] in tops, [a['id'] for _, a in ranked]
    for x in chain_has:
        assert x in chain, chain


def test_suggest_cli_is_ascii():
    import os
    env = dict(os.environ, PYTHONIOENCODING='cp1252', PYTHONUTF8='0')
    r = subprocess.run([sys.executable, str(ROOT / 'tools/sakuga.py'), 'suggest', 'ice arrow hitting a slime'],
                       capture_output=True, env=env)
    assert r.returncode == 0
    out = r.stdout.decode('ascii')
    assert out.splitlines()[0].split()[1].startswith('ice_projectile_') and 'chain: ice_charge_01 -> ice_projectile_' in out
    r = subprocess.run([sys.executable, str(ROOT / 'tools/sakuga.py'), 'suggest', '--json', 'fireball'],
                       capture_output=True, env=env)
    assert json.loads(r.stdout.decode('ascii'))['results'][0]['id'] == 'fire_projectile_01'


def test_build_ships_shared_assets(tmp_path):
    from sakugakit.build import copy_shared
    copy_shared(ROOT, tmp_path / 's')
    assert (tmp_path / 's/textures/T_Flash.png').is_file() and (tmp_path / 's/shared.json').is_file()
    shutil.rmtree(tmp_path / 's')
