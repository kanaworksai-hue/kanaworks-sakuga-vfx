import json
import re
import zipfile

import numpy as np
import pytest
from PIL import Image

from sakugakit import images
from sakugakit.build import atlas_json, build, spriteframes_tres
from sakugakit.catalog import png_extra_chunks
from sakugakit.common import load_catalog


@pytest.fixture(scope='module')
def built(tmp_path_factory):
    # module-scoped build of a two-asset library (one impact, one ground-rooted column)
    import conftest
    tmp = tmp_path_factory.mktemp('b')
    root = tmp / 'lib'
    root.mkdir()
    import shutil
    for d in conftest.CODE_DIRS:
        shutil.copytree(conftest.ROOT / d, root / d, ignore=shutil.ignore_patterns('.godot', 'flipbooks', '*.uid', '.DS_Store'))
    shutil.copyfile(conftest.ROOT / 'LICENSE.md', root / 'LICENSE.md')
    head = json.loads((conftest.ROOT / 'catalog.json').read_text(encoding='utf-8'))
    head['assets'] = []
    (root / 'catalog.json').write_text(json.dumps(head), encoding='utf-8')
    (root / 'README.md').write_text('x\n<!-- assets:start -->\nold\n<!-- assets:end -->\n', encoding='utf-8')
    from sakugakit.catalog import add
    conftest.sheet().save(tmp / 'a.png')
    conftest.sheet(cell=(32, 64), rooted=True, seed=1).save(tmp / 'b.png')
    add(tmp / 'a.png', 'fire_impact_01', (4, 2), root=root, added='2026-01-02')
    add(tmp / 'b.png', 'water_column_01', (4, 2), root=root, added='2026-01-02')
    res = build(root)
    return root, res


def test_build_writes_previews_overview_and_readme_table(built):
    root, _ = built
    for d in ('fire/fire_impact_01', 'water/water_column_01'):
        with Image.open(root / 'assets' / d / 'preview.webp') as im:
            total = 0
            for i in range(im.n_frames):                      # WebP merges identical frames, so sum the durations
                im.seek(i)
                im.load()
                total += im.info['duration']
            assert abs(total - (8 + 12) * 33) <= 12           # 8 frames + 0.4 s hold at 30 fps
    with Image.open(root / 'docs/assets/overview.webp') as im:
        assert im.n_frames > 8
    readme = (root / 'README.md').read_text(encoding='utf-8')
    assert 'old' not in readme and '`fire_impact_01`' in readme and 'preview.webp' in readme


def test_variants_have_the_right_sizes_and_modes(built):
    root, _ = built
    d = root / 'dist/stage/flipbooks/water/water_column_01'
    full, half, add = (Image.open(d / n) for n in ('water_column_01.png', 'water_column_01_1024.png', 'water_column_01_add.png'))
    assert full.size == (128, 128) and half.size == (64, 64) and add.mode == 'RGB' and full.mode == 'RGBA'
    s = json.loads((d / 'water_column_01_1024.flipbook.json').read_text(encoding='utf-8'))
    assert s['cell'] == [16, 32] and s['source'] == 'water_column_01_1024.png'
    assert json.loads((d / 'water_column_01_add.flipbook.json').read_text(encoding='utf-8'))['blend'] == 'additive'
    for p in d.glob('*.png'):
        assert png_extra_chunks(p.read_bytes()) == [], p.name


def test_half_resolution_keeps_edge_colour():
    a = np.zeros((4, 4, 4), np.uint8)
    a[0, 0] = (255, 0, 0, 255)                                     # one opaque red pixel next to transparent black
    out = np.asarray(images.half(Image.fromarray(a, 'RGBA')))
    assert out[0, 0, 3] > 0 and out[0, 0, 0] > 200 and out[0, 0, 1] < 30


def test_atlas_json_matches_texturepacker_hash_layout():
    entry = {'id': 'fire_impact_01', 'element': 'fire', 'form': 'impact', 'grid': [4, 2], 'cell': [64, 64],
             'frames': 7, 'bottom_inset': 0.1, 'version': '1.0.0'}
    data = atlas_json(entry, 'fire_impact_01.png', (256, 128), [64, 64], 30, False)
    assert len(data['frames']) == 7 and data['animations']['fire_impact_01'][0] == 'fire_impact_01_000'
    f5 = data['frames']['fire_impact_01_005']
    assert f5['frame'] == {'x': 64, 'y': 64, 'w': 64, 'h': 64} and f5['rotated'] is False
    assert data['meta']['image'] == 'fire_impact_01.png' and data['meta']['size'] == {'w': 256, 'h': 128}


def test_godot_spriteframes_resource():
    entry = {'id': 'water_column_01', 'element': 'water', 'form': 'column', 'grid': [4, 2], 'cell': [32, 64],
             'frames': 8, 'bottom_inset': 0.125, 'version': '1.0.0', 'fps': 30, 'loop': False, 'blend': 'alpha'}
    tres = spriteframes_tres(entry, {}, 'res://x/water_column_01.png')
    assert tres.startswith('[gd_resource type="SpriteFrames"')
    assert len(re.findall(r'\[sub_resource type="AtlasTexture"', tres)) == 8
    assert 'region = Rect2(96, 64, 32, 64)' in tres
    assert '"loop": false' in tres and '"speed": 30.0' in tres and '"name": &"default"' in tres
    assert '"pivot": [0.5, 0.875]' in tres


def test_packages_contain_what_each_engine_needs(built):
    root, res = built
    z = {k: zipfile.ZipFile(p) for k, p in res['zips'].items()}
    unity = set(z['unity'].namelist())
    assert 'Packages/com.kanaworks.sakuga-vfx/package.json' in unity
    assert 'Packages/com.kanaworks.sakuga-vfx/Runtime/Shaders/SakugaFlipbook.shader' in unity
    assert 'Assets/KanaWorksSakugaVFX/Flipbooks/fire/fire_impact_01/fire_impact_01.flipbook.json' in unity
    assert 'Assets/KanaWorksSakugaVFX/LICENSE.md' in unity and 'Assets/KanaWorksSakugaVFX/CREDIT.txt' in unity
    assert not any(n.endswith('.meta') or n.startswith(('LICENSE', 'README')) for n in unity)   # never overwrite project files
    godot = set(z['godot'].namelist())
    base = 'addons/kanaworks_sakuga_vfx/'
    for n in ('sakuga_flipbook_2d.gd', 'sakuga_flipbook_3d.gd', 'sakuga_flipbook_3d.gdshader', 'LICENSE.md', 'CREDIT.txt',
              'flipbooks/index.json', 'flipbooks/water/water_column_01/water_column_01.tres',
              'flipbooks/water/water_column_01/water_column_01.png.import'):
        assert base + n in godot, n
    assert 'sakuga_demo/demo_3d.tscn' in godot and 'project.godot' not in godot
    web = set(z['web'].namelist())
    pre = 'kanaworks-sakuga-vfx-1.0.0-web/'
    for n in ('index.html', 'web/index.html', 'web/sakuga-player.js', 'web/examples/pixi.html', 'catalog.json',
              'LICENSE.md', 'assets/fire/fire_impact_01/fire_impact_01_1024.png',
              'assets/fire/fire_impact_01/fire_impact_01.atlas.json', 'assets/fire/fire_impact_01/preview.webp'):
        assert pre + n in web, n
    one = set(zipfile.ZipFile(res['assets']['fire_impact_01']).namelist())
    assert {'fire_impact_01/LICENSE.md', 'fire_impact_01/CREDIT.txt', 'fire_impact_01/fire_impact_01_add_1024.png'} <= one
    out = res['out']
    manifest = json.loads((out / 'downloads/manifest.json').read_text(encoding='utf-8'))
    assert set(manifest['packages']) == {'unity', 'godot', 'web', 'flipbooks'}
    assert (out / 'site/downloads/manifest.json').is_file() and (out / 'site/web/index.html').is_file()
    assert not list((out / 'site/downloads').rglob('*.zip'))   # zips live on the GitHub release, not Pages
    assert manifest['release'].endswith('/releases/download/v1.0.0/') and manifest['assets']['fire_impact_01']['zip'] == 'fire_impact_01.zip'
    sums = (out / 'downloads/SHA256SUMS.txt').read_text(encoding='utf-8')
    assert 'kanaworks-sakuga-vfx-1.0.0-unity.zip' in sums and '  fire_impact_01.zip' in sums


def test_zips_are_reproducible(built):
    root, res = built
    first = {k: p.read_bytes() for k, p in res['zips'].items()}
    again = build(root, out=root / 'dist2', previews=False, readmes=False, site=False)
    assert {k: p.read_bytes() for k, p in again['zips'].items()} == first


def test_build_refuses_a_broken_catalog(built, tmp_path):
    root, _ = built
    cat = load_catalog(root / 'catalog.json')
    cat['assets'][0]['grid'] = [3, 3]
    (root / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
    try:
        with pytest.raises(SystemExit):
            build(root, out=tmp_path / 'x')
    finally:
        cat['assets'][0]['grid'] = [4, 2]
        (root / 'catalog.json').write_text(json.dumps(cat), encoding='utf-8')
