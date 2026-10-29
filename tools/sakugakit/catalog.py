"""Catalog and sidecar checks, `sync` (derive fields from the sheet) and `add` (drop in a new sheet)."""
import datetime
import shutil
import struct
from pathlib import Path

from PIL import Image

from . import images, recipes
from .common import (BLENDS, DATE_RE, ELEMENTS, FIELDS, FORMS, ID_RE, RECIPE_FIELDS, ROOT, SEMVER_RE,
                     asset_dir, asset_png, dump_json, load_catalog, published, read_json, save_catalog, sidecar,
                     sidecar_path, write_text)

PNG_SIGNATURE = b'\x89PNG\r\n\x1a\n'
PNG_PIXEL_CHUNKS = {b'IHDR', b'PLTE', b'tRNS', b'IDAT', b'IEND'}
REQUIRED = ('id', 'element', 'form', 'grid', 'added', 'version')
INSET_TOLERANCE = 0.002


def png_extra_chunks(data):
    if not data.startswith(PNG_SIGNATURE):
        return ['not a PNG']
    extra, offset = [], len(PNG_SIGNATURE)
    while offset + 8 <= len(data):
        length, kind = struct.unpack('>I4s', data[offset:offset + 8])
        if kind not in PNG_PIXEL_CHUNKS:
            extra.append(kind.decode('latin-1'))
        offset += 12 + length
        if kind == b'IEND':
            break
    return extra


def check_entry_fields(a):
    """Errors in one catalog entry that need no pixels."""
    errs = []
    aid = a.get('id', '?')
    for k in REQUIRED:
        if k not in a:
            errs.append(f'{aid}: missing field "{k}"')
    if errs:
        return errs
    m = ID_RE.match(a['id'])
    if not m:
        errs.append(f'{aid}: id must look like element_form_NN (lowercase), e.g. fire_impact_01')
    else:
        if m.group(1) != a['element']:
            errs.append(f'{aid}: id starts with "{m.group(1)}" but element is "{a["element"]}"')
        if m.group(2) != a['form']:
            errs.append(f'{aid}: id form "{m.group(2)}" differs from form "{a["form"]}"')
    if a['element'] not in ELEMENTS:
        errs.append(f'{aid}: unknown element "{a["element"]}" (known: {", ".join(ELEMENTS)})')
    if a['form'] not in FORMS:
        errs.append(f'{aid}: unknown form "{a["form"]}" (known: {", ".join(FORMS)})')
    g = a['grid']
    if not (isinstance(g, list) and len(g) == 2 and all(isinstance(v, int) and v > 0 for v in g)):
        errs.append(f'{aid}: grid must be [cols, rows]')
    fps = a.get('fps', 30)
    if not (isinstance(fps, (int, float)) and 1 <= fps <= 120):
        errs.append(f'{aid}: fps must be 1..120')
    if not isinstance(a.get('loop', False), bool):
        errs.append(f'{aid}: loop must be true or false')
    if a.get('blend', 'alpha') not in BLENDS:
        errs.append(f'{aid}: blend must be one of {BLENDS}')
    if not DATE_RE.match(str(a['added'])):
        errs.append(f'{aid}: added must be YYYY-MM-DD')
    if not SEMVER_RE.match(str(a['version'])):
        errs.append(f'{aid}: version must be MAJOR.MINOR.PATCH')
    unknown = set(a) - set(FIELDS) - {'status', 'note'}
    if unknown:
        errs.append(f'{aid}: unknown fields {sorted(unknown)}')
    return errs


def complete(entry, defaults, measured, ids=(), coverage=None):
    """Fill derived and default fields of an entry from its sheet, then its generated recipe.
    `ids` are all published ids (for pairs); `coverage` is the per-frame opaque area (for the contact frame)."""
    out = {k: v for k, v in entry.items() if k not in RECIPE_FIELDS}
    out['cell'] = measured['cell']
    out['frames'] = min(int(entry.get('frames') or measured['frames']), measured['frames'])
    out['bottom_inset'] = measured['bottom_inset']
    out.setdefault('fps', defaults.get('fps', 30))
    out.setdefault('loop', FORMS[entry['form']][0])
    out.setdefault('blend', defaults.get('blend', 'alpha'))
    contact = None
    if coverage is not None:
        contact = recipes.contact_from_coverage(coverage[:out['frames']], out['loop'])
    elif 'contact_frame' in entry:
        contact = entry['contact_frame']
    out.update(recipes.recipe(out, ids, contact))
    ordered = {k: out[k] for k in FIELDS if k in out}
    ordered.update({k: v for k, v in out.items() if k not in FIELDS})
    return ordered


def validate(root=ROOT):
    """Return (errors, warnings). Errors fail CI."""
    root = Path(root)
    errors, warnings = [], []
    try:
        cat = load_catalog(root / 'catalog.json')
    except Exception as exc:                                   # noqa: BLE001
        return [f'catalog.json unreadable: {exc}'], []
    defaults = cat.get('defaults', {})
    seen = set()
    assets = published(cat)
    ids = [a.get('id') for a in assets]
    for a in cat.get('assets', []):
        if a.get('status', 'published') not in ('published', 'planned'):
            errors.append(f'{a.get("id", "?")}: status must be published or planned')
    for a in assets:
        errs = check_entry_fields(a)
        if a.get('id') in seen:
            errs.append(f'{a["id"]}: duplicate id')
        seen.add(a.get('id'))
        if errs:
            errors += errs
            continue
        png = asset_png(a, root)
        if not png.is_file():
            errors.append(f'{a["id"]}: missing {png.relative_to(root).as_posix()}')
            continue
        data = png.read_bytes()
        extra = png_extra_chunks(data)
        if extra:
            errors.append(f'{a["id"]}: PNG carries metadata chunks {extra} (run: python tools/sakuga.py sync)')
        im = images.open_rgba(png)
        try:
            measured = images.measure(im, a['grid'])
        except ValueError as exc:
            errors.append(f'{a["id"]}: {exc}')
            continue
        want = complete(a, defaults, measured, ids, images.coverage(im, a['grid'], a.get('frames')))
        missing = [k for k in RECIPE_FIELDS if k not in a]
        if missing:
            errors.append(f'{a["id"]}: catalog lacks the recipe fields {missing} (run: python tools/sakuga.py sync)')
        else:
            errors += recipes.check(a)
            stale = [k for k in RECIPE_FIELDS if a[k] != want[k]]
            if stale:
                errors.append(f'{a["id"]}: recipe fields {stale} differ from tools/sakugakit/recipes.py '
                              '(run: python tools/sakuga.py sync)')
        for k in ('cell', 'frames', 'fps', 'loop', 'blend'):
            if k not in a:
                errors.append(f'{a["id"]}: catalog lacks "{k}" (run: python tools/sakuga.py sync)')
            elif a[k] != want[k] and k != 'frames':
                errors.append(f'{a["id"]}: catalog {k}={a[k]} but the sheet says {want[k]}')
        if 'frames' in a and not 1 <= a['frames'] <= measured['frames']:
            errors.append(f'{a["id"]}: frames must be 1..{measured["frames"]}')
        if 'bottom_inset' not in a or abs(a['bottom_inset'] - measured['bottom_inset']) > INSET_TOLERANCE:
            errors.append(f'{a["id"]}: bottom_inset {a.get("bottom_inset")} but measured {measured["bottom_inset"]} '
                          '(run: python tools/sakuga.py sync)')
        with Image.open(png) as raw:
            if raw.mode != 'RGBA':
                errors.append(f'{a["id"]}: sheet must be RGBA (straight alpha), got {raw.mode}')
        if images.gutter_alpha(im, a['grid']) > 0:
            warnings.append(f'{a["id"]}: some cell border pixels are not transparent (frames may bleed when filtered)')
        if max(im.size) > 4096:
            warnings.append(f'{a["id"]}: sheet larger than 4096 px; mobile GPUs may refuse it')
        side = sidecar_path(a, root)
        if not side.is_file():
            errors.append(f'{a["id"]}: missing sidecar (run: python tools/sakuga.py sync)')
        elif read_json(side) != sidecar(a, defaults):
            errors.append(f'{a["id"]}: sidecar out of date with catalog.json (run: python tools/sakuga.py sync)')
        if not (asset_dir(a, root) / 'preview.webp').is_file():
            warnings.append(f'{a["id"]}: no preview.webp yet (run: python tools/sakuga.py build)')
        allowed = {png.name, side.name, 'preview.webp'}
        stray = sorted(p.name for p in asset_dir(a, root).iterdir() if p.name not in allowed and p.name != '.DS_Store')
        if stray:
            errors.append(f'{a["id"]}: unexpected files in the asset folder {stray}')
    # folders that no catalog line describes
    adir = root / 'assets'
    if adir.is_dir():
        known = {(a['element'], a['id']) for a in assets if 'element' in a and 'id' in a}
        for el in sorted(p for p in adir.iterdir() if p.is_dir()):
            if el.name.startswith('_'):                      # assets/_shared: textures and sounds every effect uses
                continue
            for d in sorted(p for p in el.iterdir() if p.is_dir()):
                if (el.name, d.name) not in known:
                    errors.append(f'assets/{el.name}/{d.name}: folder has no catalog line')
    return errors, warnings


def sync(root=ROOT, ids=None):
    """Re-measure sheets, fill derived catalog fields, strip image metadata and rewrite sidecars."""
    root = Path(root)
    cat = load_catalog(root / 'catalog.json')
    defaults = cat.get('defaults', {})
    changed = []
    new_assets = []
    all_ids = [a.get('id') for a in published(cat)]
    for a in cat.get('assets', []):
        if a.get('status', 'published') != 'published':
            new_assets.append(a)
            continue
        if ids and a.get('id') not in ids:
            # not re-measured, but its recipe still follows recipes.py and the current set of ids (pairs)
            full = complete(a, defaults, {'cell': a['cell'], 'frames': a['frames'], 'bottom_inset': a['bottom_inset']},
                            all_ids) if all(k in a for k in ('cell', 'frames', 'bottom_inset')) else a
            if full != a:
                changed.append(f'{a["id"]}: recipe updated')
            new_assets.append(full)
            _write_sidecar(full, defaults, root, changed)
            continue
        errs = check_entry_fields(a)
        if errs:
            raise ValueError('; '.join(errs))
        png = asset_png(a, root)
        if not png.is_file():
            raise FileNotFoundError(f'{a["id"]}: missing {png.relative_to(root).as_posix()}')
        im = images.open_rgba(png)
        with Image.open(png) as raw:
            mode = raw.mode
        if png_extra_chunks(png.read_bytes()) or mode != 'RGBA':
            images.save_png(im, png)
            changed.append(f'{a["id"]}: sheet re-saved without metadata')
        full = complete(a, defaults, images.measure(im, a['grid']), all_ids,
                        images.coverage(im, a['grid'], a.get('frames')))
        if full != a:
            changed.append(f'{a["id"]}: catalog fields updated')
        new_assets.append(full)
        _write_sidecar(full, defaults, root, changed)
    cat['assets'] = new_assets
    save_catalog(cat, root / 'catalog.json')
    return changed


def _write_sidecar(entry, defaults, root, changed):
    side = sidecar_path(entry, root)
    text = dump_json(sidecar(entry, defaults))
    if not side.is_file() or side.read_text(encoding='utf-8') != text:
        write_text(side, text)
        changed.append(f'{entry["id"]}: sidecar written')


def add(sheet, asset_id, grid, root=ROOT, fps=None, loop=None, blend=None, added=None, version='1.0.0', force=False):
    """Copy a finished sheet into assets/<element>/<id>/ (metadata stripped), add its catalog line, sync it."""
    root = Path(root)
    m = ID_RE.match(asset_id)
    if not m:
        raise ValueError('id must look like element_form_NN, e.g. ice_impact_01')
    element, form = m.group(1), m.group(2)
    entry = {'id': asset_id, 'element': element, 'form': form, 'grid': [int(grid[0]), int(grid[1])],
             'added': added or datetime.date.today().isoformat(), 'version': version}
    if fps is not None:
        entry['fps'] = fps
    if loop is not None:
        entry['loop'] = loop
    if blend is not None:
        entry['blend'] = blend
    errs = check_entry_fields(entry)
    if errs:
        raise ValueError('; '.join(errs))
    cat = load_catalog(root / 'catalog.json')
    if any(a.get('id') == asset_id and a.get('status', 'published') == 'published' for a in cat['assets']) and not force:
        raise ValueError(f'{asset_id} is already in the catalog (pass --force to replace its sheet)')
    im = images.open_rgba(sheet)
    images.measure(im, entry['grid'])                       # raises on a bad grid before anything is written
    dest = asset_dir(entry, root)
    if dest.exists() and force:
        shutil.rmtree(dest)
    images.save_png(im, asset_png(entry, root))
    old = [i for i, a in enumerate(cat['assets']) if a.get('id') == asset_id]
    if old:
        cat['assets'][old[0]] = entry
    else:
        cat['assets'].append(entry)
    save_catalog(cat, root / 'catalog.json')
    sync(root, ids={asset_id})                             # measures the new sheet; the others get fresh pairs
    return asset_png(entry, root)
