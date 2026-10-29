import json
import shutil
import sys
from pathlib import Path

import numpy as np
import pytest
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))

CODE_DIRS = ('unity', 'godot', 'web', 'docs/brand')


def sheet(cols=4, rows=2, cell=(64, 64), rooted=False, seed=0):
    """A small synthetic flipbook: one soft disc (or a ground-rooted bar) per cell, transparent gutters."""
    w, h = cell
    a = np.zeros((rows * h, cols * w, 4), np.uint8)
    rng = np.random.default_rng(seed)
    yy, xx = np.mgrid[0:h, 0:w]
    for i in range(cols * rows):
        r, c = divmod(i, cols)
        if rooted:
            shape = (np.abs(xx - w / 2) < w / 6) & (yy > 8) & (yy <= h - 9)     # last filled row h-9 -> inset 8/h
        else:
            shape = np.hypot(xx - w / 2, yy - h / 2) < (w / 2 - 6) * (0.4 + 0.6 * i / (cols * rows))
        cellpx = a[r * h:(r + 1) * h, c * w:(c + 1) * w]
        cellpx[shape] = (*rng.integers(80, 255, 3), 255)
    return Image.fromarray(a, 'RGBA')


@pytest.fixture
def library(tmp_path):
    """An empty library in a temp folder with this repository's code, license and catalog header."""
    root = tmp_path / 'lib'
    root.mkdir()
    for d in CODE_DIRS:
        shutil.copytree(ROOT / d, root / d, ignore=shutil.ignore_patterns('.godot', 'flipbooks', '*.uid', '.DS_Store'))
    shutil.copyfile(ROOT / 'LICENSE.md', root / 'LICENSE.md')
    head = json.loads((ROOT / 'catalog.json').read_text(encoding='utf-8'))
    head['assets'] = []
    (root / 'catalog.json').write_text(json.dumps(head, indent=2), encoding='utf-8')
    for name in ('README.md', 'README.ja.md'):
        (root / name).write_text('# t\n<!-- assets:start -->\n<!-- assets:end -->\n', encoding='utf-8')
    return root


@pytest.fixture
def two_assets(library, tmp_path):
    from sakugakit.catalog import add
    p1, p2 = tmp_path / 'impact.png', tmp_path / 'column.png'
    sheet().save(p1)
    sheet(cols=4, rows=2, cell=(32, 64), rooted=True, seed=1).save(p2)
    add(p1, 'fire_impact_01', (4, 2), root=library, added='2026-01-02')
    add(p2, 'water_column_01', (4, 2), root=library, added='2026-01-02')
    return library
