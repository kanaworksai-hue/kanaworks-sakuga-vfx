import json
import struct
import zlib

import pytest
from PIL import Image

from conftest import ROOT, sheet
from sakugakit import images
from sakugakit.catalog import add, png_extra_chunks, sync, validate
from sakugakit.common import FORMS, ID_RE, format_catalog, load_catalog, published, read_json, sidecar

SKILL_FIELDS = {'grid': list, 'cell': list, 'frames': int, 'bottom_inset': float, 'source': str}


def test_repository_catalog_validates_cleanly():
    errors, warnings = validate(ROOT)
    assert errors == []
    assert [w for w in warnings if 'preview' in w] == [], 'run: python tools/sakuga.py build'


def test_catalog_is_one_block_per_asset_and_ids_follow_the_naming_rule():
    text = (ROOT / 'catalog.json').read_text(encoding='utf-8')
    cat = load_catalog(ROOT / 'catalog.json')
    assert format_catalog(cat) == text, 'run: python tools/sakuga.py sync'
    assets = published(cat)
    assert len(assets) >= 7
    for a in assets:
        m = ID_RE.match(a['id'])
        assert m and m.group(1) == a['element'] and m.group(2) == a['form'], a['id']
        assert text.count(f'"id": "{a["id"]}"') == 1


def test_sidecars_stay_compatible_with_the_effekseer_vfx_skill_and_add_playback_fields():
    for a in published(load_catalog(ROOT / 'catalog.json')):
        s = read_json(ROOT / 'assets' / a['element'] / a['id'] / f"{a['id']}.flipbook.json")
        for k, t in SKILL_FIELDS.items():
            assert isinstance(s[k], t), (a['id'], k)
        assert s['frames'] <= s['grid'][0] * s['grid'][1]
        assert s['source'] == f"{a['id']}.png"
        assert s['fps'] == 30 and s['blend'] in ('alpha', 'additive') and s['credit'].startswith('VFX: KanaWorks_AI')
        if a['form'] in ('projectile', 'shield', 'aura'):
            assert s['loop'] is True
        if a['form'] in ('charge', 'slash') or (a['form'] in ('impact', 'column', 'strike') and a['style'] == 'classic'):
            assert s['loop'] is False
        if FORMS[a['form']][1] and a['orientation'] == 'upright':
            assert s['bottom_inset'] > 0 and s['pivot'] == [0.5, round(1 - s['bottom_inset'], 4)]
        else:
            assert s['pivot'] == [0.5, 0.5]


def test_known_sheet_measurements():
    cat = {a['id']: a for a in load_catalog(ROOT / 'catalog.json')['assets']}
    col = cat['water_column_01']
    assert col['grid'] == [8, 7] and col['cell'] == [224, 400] and col['frames'] == 56
    assert 0.05 < col['bottom_inset'] < 0.15
    assert cat['fire_projectile_01']['grid'] == [8, 5] and cat['fire_projectile_01']['loop'] is True


def test_bottom_inset_is_measured_from_alpha():
    im = sheet(cell=(32, 64), rooted=True)
    assert images.bottom_inset(im, 4, 2) == pytest.approx(8 / 64, abs=1e-4)


def test_add_copies_strips_metadata_and_writes_catalog_and_sidecar(library, tmp_path):
    src = tmp_path / 'in.png'
    info = __import__('PIL.PngImagePlugin', fromlist=['PngInfo']).PngInfo()
    info.add_text('Comment', 'private note')
    sheet().save(src, pnginfo=info)
    assert 'tEXt' in png_extra_chunks(src.read_bytes())
    add(src, 'thunder_impact_01', (4, 2), root=library, added='2026-01-02')
    png = library / 'assets/thunder/thunder_impact_01/thunder_impact_01.png'
    assert png_extra_chunks(png.read_bytes()) == []
    cat = load_catalog(library / 'catalog.json')
    entry = cat['assets'][0]
    assert entry['cell'] == [64, 64] and entry['frames'] == 8 and entry['loop'] is False
    assert read_json(png.with_name('thunder_impact_01.flipbook.json')) == sidecar(entry, cat['defaults'])
    errors, _ = validate(library)
    assert errors == []
    with pytest.raises(ValueError):
        add(src, 'thunder_impact_01', (4, 2), root=library)            # already there
    with pytest.raises(ValueError):
        add(src, 'Thunder-Hit', (4, 2), root=library)                  # bad id
    with pytest.raises(ValueError):
        add(src, 'ice_impact_01', (3, 2), root=library)                # 256 px does not divide into 3 columns


def test_a_hand_written_catalog_line_is_completed_by_sync(library):
    png = library / 'assets/wind/wind_vortex_01/wind_vortex_01.png'
    png.parent.mkdir(parents=True)
    sheet().save(png)
    cat = load_catalog(library / 'catalog.json')
    cat['assets'].append({'id': 'wind_vortex_01', 'element': 'wind', 'form': 'vortex', 'grid': [4, 2],
                          'added': '2026-01-02', 'version': '1.0.0'})
    (library / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    assert any('sync' in e for e in validate(library)[0])
    sync(library)
    assert validate(library)[0] == []


def _break(library, what):
    a = 'assets/fire/fire_impact_01/'
    if what == 'stale sidecar':
        p = library / a / 'fire_impact_01.flipbook.json'
        p.write_text(p.read_text(encoding='utf-8').replace('"fps": 30', '"fps": 24'), encoding='utf-8')
    elif what == 'metadata':
        p = library / a / 'fire_impact_01.png'
        data = p.read_bytes()
        chunk = b'tEXtAuthor\x00someone'
        extra = struct.pack('>I', len(chunk) - 4) + chunk + struct.pack('>I', zlib.crc32(chunk))
        p.write_bytes(data[:33] + extra + data[33:])
    elif what == 'orphan folder':
        (library / 'assets/ice/ice_impact_01').mkdir(parents=True)
    elif what == 'stray file':
        (library / a / 'notes.txt').write_text('x', encoding='utf-8')
    elif what == 'wrong grid':
        cat = load_catalog(library / 'catalog.json')
        cat['assets'][0]['grid'] = [3, 2]
        (library / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    elif what == 'element mismatch':
        cat = load_catalog(library / 'catalog.json')
        cat['assets'][0]['element'] = 'water'
        (library / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    elif what == 'duplicate':
        cat = load_catalog(library / 'catalog.json')
        cat['assets'].append(dict(cat['assets'][0]))
        (library / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    elif what == 'not rgba':
        p = library / a / 'fire_impact_01.png'
        Image.open(p).convert('RGB').save(p)


@pytest.mark.parametrize('what', ['stale sidecar', 'metadata', 'orphan folder', 'stray file', 'wrong grid',
                                  'element mismatch', 'duplicate', 'not rgba'])
def test_validate_catches(two_assets, what):
    assert validate(two_assets)[0] == []
    _break(two_assets, what)
    assert validate(two_assets)[0], what
