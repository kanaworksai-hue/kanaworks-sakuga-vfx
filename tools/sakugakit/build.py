"""`build`: previews, overview, README tables, derived sheets, engine packages, per-asset zips and the Pages site.

Everything under dist/ is generated and never committed. assets/*/preview.webp, docs/assets/overview.webp and the
asset tables inside the README files are generated too, but committed so GitHub shows them.
"""
import hashlib
import json
import re
import shutil
import zipfile
from pathlib import Path

from . import images
from .catalog import validate
from .common import (CREDIT, HOMEPAGE, LIBRARY, ROOT, SLUG, asset_dir, asset_png, dump_json, load_catalog, published,
                     say, sidecar, write_text)

ZIP_TIME = (2026, 1, 1, 0, 0, 0)
GODOT_ADDON = 'addons/kanaworks_sakuga_vfx'
GODOT_DEMO = 'sakuga_demo'
UNITY_PACKAGE = 'com.kanaworks.sakuga-vfx'
UNITY_FLIPBOOKS = 'Assets/KanaWorksSakugaVFX/Flipbooks'

CREDIT_TXT = f"""{LIBRARY}
{HOMEPAGE}

Put this credit wherever you use or repost these effects (game credits, video description, page footer):

    {CREDIT}

Reposts must keep this credit and the link above. Full terms: LICENSE.md
"""


# ------------------------------------------------------------------ data files
def variant_names(aid):
    """(file stem, half resolution, additive) for every sheet variant of an asset."""
    return [(aid, False, False), (f'{aid}_1024', True, False), (f'{aid}_add', False, True), (f'{aid}_add_1024', True, True)]


def variant_sidecar(entry, defaults, stem, half_res, add):
    s = sidecar(entry, defaults)
    if half_res:
        s['cell'] = [c // 2 for c in s['cell']]
    s['source'] = f'{stem}.png'
    if add:
        s['blend'] = 'additive'
        s['alpha'] = 'none (colour premultiplied on black)'
    return s


def atlas_json(entry, image_name, size, cell, fps, loop):
    """TexturePacker 'JSON (Hash)' layout: loads in PixiJS (Assets / Spritesheet) and Phaser 3 (load.atlas)."""
    cols = entry['grid'][0]
    px, py = sidecar(entry)['pivot']
    names, frames = [], {}
    for i in range(entry['frames']):
        name = f"{entry['id']}_{i:03d}"
        names.append(name)
        x, y = (i % cols) * cell[0], (i // cols) * cell[1]
        frames[name] = {
            'frame': {'x': x, 'y': y, 'w': cell[0], 'h': cell[1]}, 'rotated': False, 'trimmed': False,
            'spriteSourceSize': {'x': 0, 'y': 0, 'w': cell[0], 'h': cell[1]},
            'sourceSize': {'w': cell[0], 'h': cell[1]},
            'pivot': {'x': px, 'y': py}, 'anchor': {'x': px, 'y': py},
        }
    return {
        'frames': frames,
        'animations': {entry['id']: names},
        'meta': {'app': HOMEPAGE, 'version': entry['version'], 'image': image_name, 'format': 'RGBA8888',
                 'size': {'w': size[0], 'h': size[1]}, 'scale': '1', 'fps': fps, 'loop': loop, 'credit': CREDIT},
    }


def _godot_value(v):
    if isinstance(v, bool):
        return 'true' if v else 'false'
    if isinstance(v, (int, float)):
        return repr(float(v)) if isinstance(v, float) else str(v)
    if isinstance(v, (list, tuple)):
        return '[' + ', '.join(_godot_value(x) for x in v) + ']'
    if isinstance(v, dict):
        return '{' + ', '.join(f'{json.dumps(k)}: {_godot_value(x)}' for k, x in v.items()) + '}'
    return json.dumps(str(v), ensure_ascii=False)


def spriteframes_tres(entry, defaults, texture_path):
    """Godot 4 SpriteFrames: one AtlasTexture per frame and a `sakuga` metadata dictionary the addon's
    scripts read (pivot, grid, blend, form) so 2D and 3D players share one resource."""
    s = sidecar(entry, defaults)
    cols = entry['grid'][0]
    w, h = entry['cell']
    out = [f'[gd_resource type="SpriteFrames" load_steps={entry["frames"] + 2} format=3]', '',
           f'[ext_resource type="Texture2D" path="{texture_path}" id="1_sheet"]', '']
    for i in range(entry['frames']):
        out += [f'[sub_resource type="AtlasTexture" id="AtlasTexture_{i:03d}"]', 'atlas = ExtResource("1_sheet")',
                f'region = Rect2({(i % cols) * w}, {(i // cols) * h}, {w}, {h})', '']
    frames = ', '.join('{\n"duration": 1.0,\n"texture": SubResource("AtlasTexture_%03d")\n}' % i for i in range(entry['frames']))
    meta = {k: s[k] for k in ('id', 'element', 'form', 'grid', 'cell', 'frames', 'fps', 'loop', 'blend', 'pivot',
                              'bottom_inset', 'version', 'credit')}
    out += ['[resource]', 'animations = [{', f'"frames": [{frames}],', f'"loop": {_godot_value(s["loop"])},',
            '"name": &"default",', f'"speed": {float(s["fps"])}', '}]', f'metadata/sakuga = {_godot_value(meta)}', '']
    return '\n'.join(out)


GODOT_IMPORT = """[remap]

importer="texture"
type="CompressedTexture2D"

[deps]

source_file="{path}"

[params]

compress/mode=0
compress/high_quality=false
compress/lossy_quality=0.7
mipmaps/generate=false
roughness/mode=0
process/fix_alpha_border=true
process/premult_alpha=false
process/size_limit=0
detect_3d/compress_to=0
"""


# ------------------------------------------------------------------ zips
def zip_dir(src, dest, prefix=''):
    """Deterministic zip: sorted entries, fixed timestamps, no OS junk."""
    src, dest = Path(src), Path(dest)
    dest.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(dest, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as z:
        for p in sorted(src.rglob('*')):
            if p.is_dir() or p.name == '.DS_Store' or '__pycache__' in p.parts:
                continue
            info = zipfile.ZipInfo((prefix + p.relative_to(src).as_posix()), ZIP_TIME)
            info.external_attr = 0o644 << 16
            info.compress_type = zipfile.ZIP_STORED if p.suffix in ('.png', '.webp') else zipfile.ZIP_DEFLATED
            z.writestr(info, p.read_bytes())
    return dest


def sha256(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def _copytree(src, dest):
    shutil.copytree(src, dest, dirs_exist_ok=True, ignore=shutil.ignore_patterns('.DS_Store', '__pycache__', '*.meta',
                                                                                 '.godot', '*.uid'))


# ------------------------------------------------------------------ README tables
README_HEADERS = {
    'README.md': ('Preview', 'ID', 'Element', 'Form', 'Frames', 'Loop', 'Added', 'yes', 'no'),
    'README.ja.md': ('プレビュー', 'ID', '属性', '形', 'フレーム', 'ループ', '追加日', 'はい', 'いいえ'),
    'README.zh-CN.md': ('预览', 'ID', '元素', '形态', '帧数', '循环', '加入日期', '是', '否'),
    'README.ko.md': ('미리보기', 'ID', '속성', '형태', '프레임', '루프', '추가일', '예', '아니요'),
    'README.pt-BR.md': ('Prévia', 'ID', 'Elemento', 'Forma', 'Quadros', 'Loop', 'Adicionado', 'sim', 'não'),
    'README.es.md': ('Vista previa', 'ID', 'Elemento', 'Forma', 'Fotogramas', 'Bucle', 'Añadido', 'sí', 'no'),
}
TABLE_RE = re.compile(r'(<!-- assets:start -->\n)(.*?)(<!-- assets:end -->)', re.S)


def asset_table(assets, headers):
    prev, aid, el, form, frames, loop, added, yes, no = headers
    rows = [f'| {prev} | {aid} | {el} | {form} | {frames} | {loop} | {added} |', '|:---:|---|---|---|---:|:---:|---|']
    for a in assets:
        folder = f"assets/{a['element']}/{a['id']}"
        rows.append(f"| <img src=\"{folder}/preview.webp\" width=\"72\" alt=\"{a['id']}\"> | "
                    f"[`{a['id']}`]({folder}/) | {a['element']} | {a['form']} | {a['frames']} "
                    f"({a['grid'][0]}x{a['grid'][1]}) | {yes if a['loop'] else no} | {a['added']} |")
    return '\n'.join(rows) + '\n'


def update_readmes(root, assets):
    changed = []
    for name, headers in README_HEADERS.items():
        path = Path(root) / name
        if not path.is_file():
            continue
        text = path.read_text(encoding='utf-8')
        new = TABLE_RE.sub(lambda m: m.group(1) + asset_table(assets, headers) + m.group(3), text)
        if new != text:
            write_text(path, new)
            changed.append(name)
    return changed


# ------------------------------------------------------------------ build
def write_flipbook_folder(entry, defaults, im, dest, root):
    """All sheet variants of one asset, each with its sidecar and TexturePacker atlas JSON."""
    dest.mkdir(parents=True, exist_ok=True)
    halfim = images.half(im)
    for stem, half_res, add in variant_names(entry['id']):
        src = halfim if half_res else im
        if add:
            src = images.additive(src)
        if not half_res and not add:
            shutil.copyfile(asset_png(entry, root), dest / f'{stem}.png')   # the committed sheet, byte for byte
        else:
            images.save_png(src, dest / f'{stem}.png')
        s = variant_sidecar(entry, defaults, stem, half_res, add)
        write_text(dest / f'{stem}.flipbook.json', dump_json(s))
        write_text(dest / f'{stem}.atlas.json', dump_json(atlas_json(entry, f'{stem}.png', src.size, s['cell'],
                                                                     s['fps'], s['loop'])))
    shutil.copyfile(asset_dir(entry, root) / 'preview.webp', dest / 'preview.webp')


def stage_unity(root, assets, defaults, dest):
    dest = Path(dest)
    _copytree(Path(root) / 'unity' / UNITY_PACKAGE, dest / 'Packages' / UNITY_PACKAGE)
    base = dest / 'Assets' / 'KanaWorksSakugaVFX'
    for a in assets:
        d = dest / UNITY_FLIPBOOKS / a['element'] / a['id']
        d.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(asset_png(a, root), d / f"{a['id']}.png")
        write_text(d / f"{a['id']}.flipbook.json", dump_json(sidecar(a, defaults)))
    shutil.copyfile(Path(root) / 'LICENSE.md', base / 'LICENSE.md')
    write_text(base / 'CREDIT.txt', CREDIT_TXT)
    return dest


def stage_godot(root, assets, defaults, dest):
    dest = Path(dest)
    addon = dest / GODOT_ADDON
    _copytree(Path(root) / 'godot' / GODOT_ADDON, addon)
    _copytree(Path(root) / 'godot' / GODOT_DEMO, dest / GODOT_DEMO)
    listing = []
    for a in assets:
        rel = f"flipbooks/{a['element']}/{a['id']}"
        d = addon / rel
        d.mkdir(parents=True, exist_ok=True)
        res = f'res://{GODOT_ADDON}/{rel}'
        shutil.copyfile(asset_png(a, root), d / f"{a['id']}.png")
        write_text(d / f"{a['id']}.png.import", GODOT_IMPORT.format(path=f"{res}/{a['id']}.png"))
        write_text(d / f"{a['id']}.flipbook.json", dump_json(sidecar(a, defaults)))
        write_text(d / f"{a['id']}.tres", spriteframes_tres(a, defaults, f"{res}/{a['id']}.png"))
        listing.append({'id': a['id'], 'element': a['element'], 'form': a['form'], 'frames': f"{res}/{a['id']}.tres"})
    write_text(addon / 'flipbooks' / 'index.json', dump_json({'credit': CREDIT, 'assets': listing}))
    shutil.copyfile(Path(root) / 'LICENSE.md', addon / 'LICENSE.md')
    write_text(addon / 'CREDIT.txt', CREDIT_TXT)
    return dest


REDIRECT = ('<!doctype html><meta charset="utf-8"><title>KanaWorks SAKUGA VFX</title>'
            '<meta http-equiv="refresh" content="0; url=web/"><a href="web/">KanaWorks SAKUGA VFX gallery</a>\n')


def stage_web(root, assets, flip_root, dest, catalog):
    """Same layout as the repository (web/, assets/, catalog.json), so every relative URL works unchanged."""
    dest = Path(dest)
    _copytree(Path(root) / 'web', dest / 'web')
    for a in assets:
        _copytree(flip_root / a['element'] / a['id'], dest / 'assets' / a['element'] / a['id'])
    _copytree(Path(root) / 'docs' / 'brand', dest / 'docs' / 'brand')
    write_text(dest / 'catalog.json', dump_json(catalog))
    shutil.copyfile(Path(root) / 'LICENSE.md', dest / 'LICENSE.md')
    write_text(dest / 'CREDIT.txt', CREDIT_TXT)
    write_text(dest / 'index.html', REDIRECT)
    return dest


def build(root=ROOT, out=None, previews=True, readmes=True, site=True):
    root = Path(root)
    out = Path(out) if out else root / 'dist'
    errors, warnings = validate(root)
    # a missing preview is a warning for validate but build makes it, so only real errors stop us
    if errors:
        raise SystemExit('catalog has errors, run: python tools/sakuga.py validate')
    catalog = load_catalog(root / 'catalog.json')
    defaults = catalog.get('defaults', {})
    version = catalog['version']
    assets = published(catalog)
    if not assets:
        raise SystemExit('no published assets in catalog.json')
    if out.exists():
        shutil.rmtree(out)
    out.mkdir(parents=True)
    sheets = []
    for a in assets:
        im = images.open_rgba(asset_png(a, root))
        sheets.append((a, im))
        if previews or not (asset_dir(a, root) / 'preview.webp').is_file():
            images.preview(im, a, asset_dir(a, root) / 'preview.webp')
            say(f'  preview   {a["id"]}')
    if previews:
        images.overview(sheets, root / 'docs' / 'assets' / 'overview.webp')
        say('  overview  docs/assets/overview.webp')
    if readmes:
        for name in update_readmes(root, assets):
            say(f'  table     {name}')

    flip = out / 'stage' / 'flipbooks'
    for a, im in sheets:
        write_flipbook_folder(a, defaults, im, flip / a['element'] / a['id'], root)
        say(f'  sheets    {a["id"]} (2048, 1024, additive, atlas json)')
    downloads = out / 'downloads'
    tag = f'{SLUG}-{version}'
    zips = {}

    pack = out / 'stage' / 'pack-flipbooks'
    _copytree(flip, pack)
    shutil.copyfile(root / 'LICENSE.md', pack / 'LICENSE.md')
    write_text(pack / 'CREDIT.txt', CREDIT_TXT)
    write_text(pack / 'catalog.json', dump_json(catalog))
    zips['flipbooks'] = zip_dir(pack, downloads / f'{tag}-flipbooks.zip', f'{tag}-flipbooks/')
    zips['unity'] = zip_dir(stage_unity(root, assets, defaults, out / 'stage' / 'unity'), downloads / f'{tag}-unity.zip')
    zips['godot'] = zip_dir(stage_godot(root, assets, defaults, out / 'stage' / 'godot'), downloads / f'{tag}-godot.zip')
    web = stage_web(root, assets, flip, out / 'stage' / 'web', catalog)
    zips['web'] = zip_dir(web, downloads / f'{tag}-web.zip', f'{tag}-web/')
    per_asset = {}
    for a in assets:
        d = out / 'stage' / 'asset' / a['id']
        _copytree(flip / a['element'] / a['id'], d / a['id'])
        shutil.copyfile(root / 'LICENSE.md', d / a['id'] / 'LICENSE.md')
        write_text(d / a['id'] / 'CREDIT.txt', CREDIT_TXT)
        per_asset[a['id']] = zip_dir(d, downloads / 'assets' / f"{a['id']}.zip")
    for name, z in zips.items():
        say(f'  zip       {z.relative_to(out).as_posix()} ({z.stat().st_size // 1024} KB)')
    say(f'  zip       downloads/assets/<id>.zip x{len(per_asset)}')

    all_zips = list(zips.values()) + list(per_asset.values())
    write_text(downloads / 'SHA256SUMS.txt', ''.join(f'{sha256(z)}  {z.relative_to(downloads).as_posix()}\n'
                                                    for z in sorted(all_zips)))
    manifest = {
        'version': version,
        'packages': {k: {'file': z.name, 'bytes': z.stat().st_size} for k, z in zips.items()},
        'assets': {aid: {'zip': f'assets/{z.name}', 'bytes': z.stat().st_size,
                         'files': [f'{stem}.png' for stem, _, _ in variant_names(aid)]
                         + [f'{stem}.atlas.json' for stem, _, _ in variant_names(aid)]}
                   for aid, z in per_asset.items()},
    }
    write_text(downloads / 'manifest.json', dump_json(manifest))
    if site:
        s = out / 'site'
        _copytree(web, s)
        _copytree(downloads, s / 'downloads')
        write_text(s / 'catalog.json', dump_json({**catalog, 'downloads': 'downloads/manifest.json'}))
        write_text(s / '.nojekyll', '')
        say('  site      dist/site (GitHub Pages)')
    return {'zips': zips, 'assets': per_asset, 'warnings': warnings, 'out': out}


def install_unity(project, root=ROOT):
    project = Path(project).expanduser().resolve()
    if not (project / 'Assets').is_dir() or not (project / 'ProjectSettings').is_dir():
        raise ValueError(f'{project.name} is not a Unity project (expected Assets/ and ProjectSettings/)')
    catalog = load_catalog(root / 'catalog.json')
    pkg = project / 'Packages' / UNITY_PACKAGE
    metas = {m.relative_to(pkg): m.read_bytes() for m in pkg.rglob('*.meta')} if pkg.exists() else {}
    if pkg.exists():
        shutil.rmtree(pkg)
    stage_unity(root, published(catalog), catalog.get('defaults', {}), project)
    for rel, data in metas.items():                        # keep GUIDs so scenes and prefabs keep their scripts
        if (pkg / rel).with_suffix('').exists():
            (pkg / rel).write_bytes(data)
    return project


def install_godot(project, root=ROOT):
    project = Path(project).expanduser().resolve()
    if not (project / 'project.godot').is_file():
        raise ValueError(f'{project.name} is not a Godot project (expected project.godot)')
    catalog = load_catalog(root / 'catalog.json')
    stage_godot(root, published(catalog), catalog.get('defaults', {}), project)
    return project
