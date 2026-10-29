"""assets/_shared: the white textures, ground decals and sound effects every layered effect uses.

    textures/T_Flash.png, T_Ring.png, T_Spark.png, T_Glow.png, T_Smoke_2x2.png, T_Debris_2x2.png   (white, tinted at runtime)
    decals/decal_<element>.png                                                                    (optional)
    sfx/<element>_cast.wav, <element>_travel_loop.wav, <element>_impact.wav, shield_loop.wav,
        charge.wav, slash.wav, aura_loop.wav                                                       (optional)
    shared.json                                                                                   (what is there)

`python tools/sakuga.py shared <folder>` copies new files in (image metadata and WAV tags stripped) and rewrites
shared.json. Engines read shared.json and must cope with any decal or sound that is not there yet.
"""
import struct
import wave
from pathlib import Path

from . import images
from .common import ELEMENTS, ROOT, SHARED, dump_json, read_json, write_text

TEXTURES = {
    'T_Flash': {'grid': [1, 1], 'use': 'contact flash (star)'},
    'T_Ring': {'grid': [1, 1], 'use': 'shockwave ring'},
    'T_Spark': {'grid': [1, 1], 'use': 'spark streak, points along +x'},
    'T_Glow': {'grid': [1, 1], 'use': 'soft glow, trail puff, ground glow'},
    'T_Smoke_2x2': {'grid': [2, 2], 'use': 'cel smoke puffs (4 variants)'},
    'T_Debris_2x2': {'grid': [2, 2], 'use': 'rock / ice chunks (4 variants)'},
}
SFX_NAMES = tuple(f'{e}_{k}' for e in ELEMENTS for k in ('cast', 'travel_loop', 'impact')) + (
    'shield_loop', 'charge', 'slash', 'aura_loop')
DECAL_NAMES = tuple(f'decal_{e}' for e in ELEMENTS)
WAV_CHUNKS = {b'fmt ', b'data', b'fact'}


def shared_dir(root=ROOT):
    return Path(root) / SHARED


def wav_extra_chunks(data):
    """RIFF chunks other than the audio itself (LIST/INFO tags, iXML, bext and so on)."""
    if data[:4] != b'RIFF' or data[8:12] != b'WAVE':
        return ['not a WAV']
    extra, offset = [], 12
    while offset + 8 <= len(data):
        kind, size = data[offset:offset + 4], struct.unpack('<I', data[offset + 4:offset + 8])[0]
        if kind not in WAV_CHUNKS:
            extra.append(kind.decode('latin-1'))
        offset += 8 + size + (size & 1)
    return extra


def copy_wav(src, dest):
    """Re-write a PCM WAV with only its format and samples."""
    with wave.open(str(src), 'rb') as r:
        params, frames = r.getparams(), r.readframes(r.getnframes())
    dest.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(dest), 'wb') as w:
        w.setnchannels(params.nchannels)
        w.setsampwidth(params.sampwidth)
        w.setframerate(params.framerate)
        w.writeframes(frames)
    return dest


def wav_seconds(path):
    with wave.open(str(path), 'rb') as r:
        return round(r.getnframes() / float(r.getframerate()), 3)


def manifest(root=ROOT):
    d = shared_dir(root)
    tex = {n: dict(v, file=f'textures/{n}.png') for n, v in TEXTURES.items() if (d / 'textures' / f'{n}.png').is_file()}
    decals = {n: f'decals/{n}.png' for n in DECAL_NAMES if (d / 'decals' / f'{n}.png').is_file()}
    sfx = {f'{n}.wav': {'file': f'sfx/{n}.wav', 'seconds': wav_seconds(d / 'sfx' / f'{n}.wav'),
                        'loop': n.endswith('_loop')}
           for n in SFX_NAMES if (d / 'sfx' / f'{n}.wav').is_file()}
    return {'textures': tex, 'decals': decals, 'sfx': sfx,
            'note': 'White textures are tinted at runtime. Decals and sounds may be missing: skip that layer.'}


def import_shared(src, root=ROOT):
    """Copy textures, decals and sounds from a folder laid out like assets/_shared (or flat) into the library."""
    src, d = Path(src), shared_dir(root)
    if not src.is_dir():
        raise FileNotFoundError(f'{src.name}: not a folder')
    done = []

    def find(sub, name):
        for p in (src / sub / name, src / name):
            if p.is_file():
                return p
        return None

    for n in TEXTURES:
        p = find('textures', f'{n}.png')
        if p:
            images.save_png(images.open_rgba(p), d / 'textures' / f'{n}.png')
            done.append(f'textures/{n}.png')
    for n in DECAL_NAMES:
        p = find('decals', f'{n}.png')
        if p:
            images.save_png(images.open_rgba(p), d / 'decals' / f'{n}.png')
            done.append(f'decals/{n}.png')
    for n in SFX_NAMES:
        p = find('sfx', f'{n}.wav')
        if p:
            copy_wav(p, d / 'sfx' / f'{n}.wav')
            done.append(f'sfx/{n}.wav')
    write_text(d / 'shared.json', dump_json(manifest(root)))
    return done


def check(root=ROOT):
    """(errors, warnings) for assets/_shared."""
    from .catalog import png_extra_chunks
    d = shared_dir(root)
    errors, warnings = [], []
    if not d.is_dir():
        return errors, ['assets/_shared is missing: layered effects fall back to the main flipbook only']
    for n in TEXTURES:
        if not (d / 'textures' / f'{n}.png').is_file():
            errors.append(f'assets/_shared/textures/{n}.png is missing')
    for p in sorted(d.rglob('*')):
        if not p.is_file() or p.name in ('shared.json', '.DS_Store'):
            continue
        rel = p.relative_to(d).as_posix()
        if p.suffix == '.png':
            if png_extra_chunks(p.read_bytes()):
                errors.append(f'assets/_shared/{rel}: PNG carries metadata (run: python tools/sakuga.py shared <folder>)')
        elif p.suffix == '.wav':
            if wav_extra_chunks(p.read_bytes()):
                errors.append(f'assets/_shared/{rel}: WAV carries tags (run: python tools/sakuga.py shared <folder>)')
        else:
            errors.append(f'assets/_shared/{rel}: unexpected file')
    m = d / 'shared.json'
    if not m.is_file() or read_json(m) != manifest(root):
        errors.append('assets/_shared/shared.json is out of date (run: python tools/sakuga.py shared --refresh)')
    missing = [n for n in SFX_NAMES if not (d / 'sfx' / f'{n}.wav').is_file()]
    if missing:
        warnings.append(f'assets/_shared/sfx: {len(missing)} sound(s) not there yet (effects play silently)')
    return errors, warnings


def refresh(root=ROOT):
    write_text(shared_dir(root) / 'shared.json', dump_json(manifest(root)))
