"""Shared constants, catalog I/O and ASCII-safe console output."""
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
CATALOG = ROOT / 'catalog.json'
ASSETS = ROOT / 'assets'
DIST = ROOT / 'dist'

LIBRARY = 'KanaWorks SAKUGA VFX'
SLUG = 'kanaworks-sakuga-vfx'
CREDIT = 'VFX: KanaWorks_AI (@KanaWorks_AI)'
HOMEPAGE = 'https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx'
LICENSE_NAME = 'KanaWorks_AI VFX Asset License (see LICENSE.md)'

ELEMENTS = ('water', 'fire', 'thunder', 'wind', 'poison', 'ice', 'rock', 'light', 'dark')
# form -> (loops by default, ground-rooted)
FORMS = {
    'projectile': (True, False),
    'impact': (False, False),
    'column': (False, True),
    'vortex': (False, False),
    'strike': (False, True),
}
BLENDS = ('alpha', 'additive')
ID_RE = re.compile(r'^([a-z]+)_([a-z]+)_(\d{2})$')
DATE_RE = re.compile(r'^\d{4}-\d{2}-\d{2}$')
SEMVER_RE = re.compile(r'^\d+\.\d+\.\d+$')

# Catalog field order (also the order written back by `sync`).
FIELDS = ('id', 'element', 'form', 'grid', 'cell', 'frames', 'fps', 'loop', 'blend', 'bottom_inset', 'added', 'version')


def say(*parts):
    """Print ASCII only: Windows consoles on cp1252 (or worse) must never crash on our output."""
    text = ' '.join(str(p) for p in parts)
    sys.stdout.write(text.encode('ascii', 'replace').decode('ascii') + '\n')
    sys.stdout.flush()


def read_json(path):
    return json.loads(Path(path).read_text(encoding='utf-8'))


def write_text(path, text):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding='utf-8', newline='\n')


def dump_json(data):
    return json.dumps(data, indent=2, ensure_ascii=False) + '\n'


def load_catalog(path=CATALOG):
    return read_json(path)


def format_catalog(catalog):
    """Pretty JSON with one asset per line, so adding an asset is adding one line."""
    head = {k: v for k, v in catalog.items() if k != 'assets'}
    lines = json.dumps(head, indent=2, ensure_ascii=False)[:-2].rstrip()   # drop the closing brace
    rows = []
    for asset in catalog.get('assets', []):
        ordered = {k: asset[k] for k in FIELDS if k in asset}
        ordered.update({k: v for k, v in asset.items() if k not in ordered})
        rows.append('    ' + json.dumps(ordered, ensure_ascii=False, separators=(', ', ': ')))
    body = ',\n'.join(rows)
    sep = ',\n' if head else ''
    return lines + sep + '  "assets": [\n' + body + ('\n' if rows else '') + '  ]\n}\n'


def save_catalog(catalog, path=CATALOG):
    write_text(path, format_catalog(catalog))


def asset_dir(entry, root=ROOT):
    return Path(root) / 'assets' / entry['element'] / entry['id']


def asset_png(entry, root=ROOT):
    return asset_dir(entry, root) / f"{entry['id']}.png"


def sidecar_path(entry, root=ROOT):
    return asset_dir(entry, root) / f"{entry['id']}.flipbook.json"


def pivot(entry):
    """Pivot in normalized cell coordinates, origin top-left, y down (sprite-sheet convention).
    Ground-rooted forms put it on the root of the shape; everything else is centred."""
    if FORMS.get(entry['form'], (False, False))[1]:
        return [0.5, round(1.0 - float(entry.get('bottom_inset', 0.0)), 4)]
    return [0.5, 0.5]


def sidecar(entry, defaults=None):
    """The <id>.flipbook.json written next to every sheet. The first five fields match the
    effekseer-vfx skill's Unity export (grid, cell, frames, bottom_inset, source)."""
    d = defaults or {}
    return {
        'grid': list(entry['grid']),
        'cell': list(entry['cell']),
        'frames': entry['frames'],
        'bottom_inset': entry['bottom_inset'],
        'source': f"{entry['id']}.png",
        'id': entry['id'],
        'element': entry['element'],
        'form': entry['form'],
        'fps': entry.get('fps', d.get('fps', 30)),
        'loop': entry.get('loop', FORMS[entry['form']][0]),
        'blend': entry.get('blend', d.get('blend', 'alpha')),
        'pivot': pivot(entry),
        'alpha': 'straight',
        'version': entry['version'],
        'credit': CREDIT,
        'license': LICENSE_NAME,
        'homepage': HOMEPAGE,
    }


def published(catalog):
    """Assets that exist (planned entries, if anyone adds them, are skipped everywhere)."""
    return [a for a in catalog.get('assets', []) if a.get('status', 'published') == 'published']
