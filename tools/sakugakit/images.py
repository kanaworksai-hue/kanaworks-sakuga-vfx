"""Pixel work: measuring sheets, writing metadata-free images, derived variants and previews."""
import io
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

PREVIEW_MAX = 160           # longest side of a cell in preview.webp
OVERVIEW_TILE = 200         # overview grid tile (square)
HOLD_SECONDS = 0.4          # empty pause after a one-shot in looping previews


def open_rgba(path):
    with Image.open(path) as im:
        im.load()
        if im.mode != 'RGBA':
            im = im.convert('RGBA')
        return im.copy()


def clean(im):
    """A copy that carries pixels only: no text chunks, EXIF, XMP, ICC profile or DPI."""
    out = Image.frombytes(im.mode, im.size, im.tobytes())
    out.info = {}
    return out


def save_png(im, path, optimize=True):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    clean(im).save(path, format='PNG', optimize=optimize)
    return path


def png_bytes(im):
    buf = io.BytesIO()
    clean(im).save(buf, format='PNG', optimize=True)
    return buf.getvalue()


def cells(im, grid):
    cols, rows = grid
    w, h = im.size[0] // cols, im.size[1] // rows
    for i in range(cols * rows):
        c, r = i % cols, i // cols
        yield im.crop((c * w, r * h, (c + 1) * w, (r + 1) * h))


def measure(im, grid):
    """cell size, frame count and the ground inset of a sheet with the given grid."""
    cols, rows = int(grid[0]), int(grid[1])
    w, h = im.size
    if cols < 1 or rows < 1 or w % cols or h % rows:
        raise ValueError(f'{w}x{h} does not divide into a {cols}x{rows} grid')
    return {'cell': [w // cols, h // rows], 'frames': cols * rows, 'bottom_inset': bottom_inset(im, cols, rows)}


def bottom_inset(im, cols, rows, alpha=0.25):
    """How far the drawn shape stays above each cell's bottom edge (fraction of the cell height, gutter
    included), taken low (10th percentile) across the cells that have content. Same rule as the
    effekseer-vfx skill, so a ground-rooted column sinks by this much and its root touches the floor."""
    a = np.asarray(im.getchannel('A'), dtype=np.float32) / 255.0
    h, w = a.shape[0] // rows, a.shape[1] // cols
    gaps = []
    for r in range(rows):
        for c in range(cols):
            filled = np.nonzero((a[r * h:(r + 1) * h, c * w:(c + 1) * w] > alpha).any(axis=1))[0]
            if len(filled):
                gaps.append((h - 1 - filled[-1]) / h)
    return round(float(np.percentile(gaps, 10)), 4) if gaps else 0.0


def gutter_alpha(im, grid):
    """Highest alpha on any cell's outer border pixel (0 means frames cannot bleed into each other)."""
    cols, rows = grid
    a = np.asarray(im.getchannel('A'))
    h, w = a.shape[0] // rows, a.shape[1] // cols
    xs = sorted({c * w for c in range(cols)} | {c * w + w - 1 for c in range(cols)})
    ys = sorted({r * h for r in range(rows)} | {r * h + h - 1 for r in range(rows)})
    return int(max(a[:, xs].max(), a[ys, :].max()))


def _premultiplied(im):
    px = np.asarray(im, dtype=np.float32) / 255.0
    rgb = px[..., :3] * px[..., 3:4]
    return rgb, px[..., 3]


def half(im):
    """Half resolution, filtered in premultiplied space so edges keep their colour (no dark fringe)."""
    rgb, a = _premultiplied(im)
    size = (im.size[0] // 2, im.size[1] // 2)

    def down(ch):
        return np.asarray(Image.fromarray(ch, mode='F').resize(size, Image.LANCZOS), dtype=np.float32)

    a2 = np.clip(down(a), 0, 1)
    rgb2 = np.stack([np.clip(down(rgb[..., i]), 0, 1) for i in range(3)], axis=-1)
    safe = np.where(a2 > 1e-4, a2, 1.0)[..., None]
    straight = np.where(a2[..., None] > 1e-4, np.clip(rgb2 / safe, 0, 1), 0.0)
    out = np.dstack([straight, a2])
    return Image.fromarray((out * 255 + 0.5).astype(np.uint8), mode='RGBA')


def additive(im):
    """Opaque RGB on black (colour x alpha): for additive blending in engines without alpha."""
    rgb, _ = _premultiplied(im)
    return Image.fromarray((np.clip(rgb, 0, 1) * 255 + 0.5).astype(np.uint8), mode='RGB')


def _fit(size, limit):
    w, h = size
    s = min(limit / w, limit / h, 1.0)
    return max(1, round(w * s)), max(1, round(h * s))


def frames_small(im, grid, frames, limit):
    out = []
    for i, cell in enumerate(cells(im, grid)):
        if i >= frames:
            break
        out.append(cell.resize(_fit(cell.size, limit), Image.LANCZOS))
    return out


def _webp(frames, path, fps, quality, alpha_quality=100):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    durations = [max(10, round(1000 / fps))] * len(frames)
    frames =[clean(f) for f in frames]
    frames[0].save(path, format='WEBP', save_all=True, append_images=frames[1:], duration=durations,
                   loop=0, quality=quality, alpha_quality=alpha_quality, method=4)
    return path


def preview(im, entry, path, quality=55, alpha_quality=70):
    """Small animated WebP (transparent) that loops forever; one-shots get a short empty pause."""
    small = frames_small(im, entry['grid'], entry['frames'], PREVIEW_MAX)
    if not entry.get('loop'):
        blank = Image.new('RGBA', small[0].size, (0, 0, 0, 0))
        small += [blank] * max(1, round(HOLD_SECONDS * entry.get('fps', 30)))
    return _webp(small, path, entry.get('fps', 30), quality, alpha_quality)


def _font(size):
    try:
        return ImageFont.load_default(size=size)
    except TypeError:                                    # Pillow < 10.1
        return ImageFont.load_default()


def overview(sheets, path, columns=4, fps=30, quality=60):
    """Animated grid of every asset on a dark background, for the top of the README.
    `sheets` is a list of (entry, image)."""
    tile, label = OVERVIEW_TILE, 22
    rows = (len(sheets) + columns - 1) // columns
    cols = min(columns, len(sheets))
    small = [(e, frames_small(im, e['grid'], e['frames'], tile - 16)) for e, im in sheets]
    total = max(len(f) for _, f in small) + round(HOLD_SECONDS * fps)
    font = _font(14)
    bg = (20, 21, 28, 255)
    base = Image.new('RGBA', (cols * tile, rows * (tile + label)), bg)
    draw = ImageDraw.Draw(base)
    for i, (e, _) in enumerate(small):
        x, y = (i % cols) * tile, (i // cols) * (tile + label)
        draw.rounded_rectangle((x + 4, y + 4, x + tile - 4, y + tile + label - 4), 10, fill=(30, 32, 42, 255))
        draw.text((x + tile // 2, y + tile + 6), e['id'], fill=(200, 205, 220, 255), font=font, anchor='mt')
    out = []
    for k in range(total):
        frame = base.copy()
        for i, (e, fr) in enumerate(small):
            n = len(fr)
            idx = k % n if e.get('loop') else k
            if idx >= n:
                continue
            f = fr[idx]
            x = (i % cols) * tile + (tile - f.size[0]) // 2
            y = (i // cols) * (tile + label) + (tile - f.size[1]) // 2 + 4
            frame.alpha_composite(f, (x, y))
        out.append(frame.convert('RGB'))
    return _webp(out, path, fps, quality)
