"""Nothing personal, machine-specific, secret or production-related ships in the repository.

Generic rules only, so this file itself discloses nothing: e-mail addresses, home and temporary paths, credential
formats, image metadata (PNG, JPEG, WebP, GIF), video and 3D model files, prompt or log files, and a list of terms that
must never appear, stored as truncated SHA-256 hashes of lowercase words (single words, hyphenated and joined pairs, and
two-character CJK runs) so the list does not name what it guards against.

A maintainer can add more terms (names, handles, private project names) in a file kept outside the repository and point
SAKUGA_PRIVATE_TERMS at it (one term per line, case-insensitive); without it that check skips.
"""
import hashlib
import os
import re
import struct
import subprocess
from pathlib import Path

import pytest

from conftest import ROOT

THIS = Path(__file__).resolve()
SKIPPED_PARTS = {'.git', '__pycache__', '.pytest_cache', 'dist', 'node_modules', '.godot'}

EMAIL = re.compile(r'[\w.+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}\b')
ALLOWED_EMAIL = re.compile(r'@(?:example\.(?:com|org|invalid)|users\.noreply\.github\.com)$', re.I)
LOCAL_PATH = re.compile(r'(?<![\w.])(?:/Users/[^/\s"\'`<>]+|/home/[^/\s"\'`<>]+|/root/|/private/(?:tmp|var)/|'
                        r'/var/folders/|[A-Za-z]:\\Users\\)', re.I)
CREDENTIAL = re.compile(r'(?:sk-[A-Za-z0-9_-]{20,}|sk-ant-[A-Za-z0-9_-]{10,}|gh[pousr]_[A-Za-z0-9]{20,}|github_pat_\w{20,}|'
                        r'AKIA[0-9A-Z]{16}|xox[abposr]-[A-Za-z0-9-]{10,}|AIza[0-9A-Za-z_-]{30,}|'
                        r'-----BEGIN [A-Z ]*PRIVATE KEY-----|'
                        r'(?:api[_-]?key|secret|token|password|passwd)["\']?\s*[:=]\s*["\']?[A-Za-z0-9_\-/+=]{16,})', re.I)
# Truncated sha256 of lowercase terms that must never appear (tools and services, private character names).
BANNED = {
    '00fc04864030716d', '32161e08e0f2abdf', 'ec9d55cbf3097cf8', '60965168ce762e94', '053ea4804ef1bb33',
    '204be88528978fdc', 'adc4a22ba41333c5', '1472950b8ae0b5cd', '8a105898989c6e17', '00a23ea065a51385',
    'd931372ba7f836a0', '7d3194f79e645c42', 'a4c745facd3a9215', '61b77e47289aa9c2', '9a79f7ea506e6aa1',
    '66c6f57f73358bfa', '25906f29d2d4b638', '3669ffb921cca6ee', 'afac327e73694e68', '3559fb35e3328cff',
    '0c0902428f07fd49', '090af99262e4c433', '422332977c41eb77', '5ca0b910e19bfef2', 'af759d1d0ef00484',
    '194cbf5c4433b278', 'b33b1345570e7b7a', 'cbf8d7db3e2d0b6c', 'f64660efbe61dd28',
}
FORBIDDEN_SUFFIXES = {'.mp4', '.mov', '.webm', '.mkv', '.avi', '.m4v', '.log', '.fbx', '.glb', '.gltf', '.blend', '.obj',
                      '.vrm', '.pmx', '.psd', '.wav', '.mp3', '.ogg'}
# Our own generated sound effects ship here, as tag-free PCM WAV (checked chunk by chunk below).
AUDIO_DIR = 'assets/_shared/sfx/'
WAV_PIXEL_CHUNKS = {b'fmt ', b'data', b'fact'}
FORBIDDEN_NAMES = re.compile(r'prompt|generation|\.env$|credential', re.I)

PNG_SIGNATURE = b'\x89PNG\r\n\x1a\n'
PNG_PIXEL_CHUNKS = {b'IHDR', b'PLTE', b'tRNS', b'IDAT', b'IEND', b'gAMA', b'cHRM', b'sRGB', b'sBIT', b'pHYs'}
JPEG_PIXEL_MARKERS = {0xC0, 0xC1, 0xC2, 0xC4, 0xDB, 0xDD, 0xE0}
WEBP_PIXEL_CHUNKS = {b'VP8 ', b'VP8L', b'VP8X', b'ALPH', b'ANIM', b'ANMF'}
CJK = re.compile(r'[\u3040-\u30ff\u3400-\u9fff\uac00-\ud7af]+')


def h(term):
    return hashlib.sha256(term.encode('utf-8')).hexdigest()[:16]


def files():
    """Files git would publish (tracked or not ignored); every file when the folder is not a git repository."""
    try:
        out = subprocess.run(['git', 'ls-files', '-z', '--cached', '--others', '--exclude-standard'], cwd=ROOT,
                             capture_output=True, check=True).stdout.decode('utf-8')
        paths = [ROOT / p for p in out.split('\0') if p]
    except (OSError, subprocess.CalledProcessError):
        paths = [p for p in ROOT.rglob('*') if p.is_file()]
    for p in sorted(paths):
        if p.is_file() and not SKIPPED_PARTS & set(p.relative_to(ROOT).parts) and p.name != '.DS_Store':
            yield p


def banned_hits(text):
    lowered = text.lower()
    words = re.findall(r'[a-z0-9]+(?:-[a-z0-9]+)*', lowered)
    candidates = set()
    for w in words:
        parts = w.split('-')
        candidates.update(parts)
        candidates.add(w)
        candidates.update(a + b for a, b in zip(parts, parts[1:]))
        candidates.update(f'{a}-{b}' for a, b in zip(parts, parts[1:]))
    plain = re.findall(r'[a-z0-9]+', lowered)
    candidates.update(a + b for a, b in zip(plain, plain[1:]))          # "two words" written apart
    for run in CJK.findall(text):
        candidates.update(run[i:i + 2] for i in range(len(run) - 1))
    return sorted({f'banned term #{sorted(BANNED).index(h(c))}' for c in candidates if h(c) in BANNED})


def private_terms():
    source = os.environ.get('SAKUGA_PRIVATE_TERMS')
    if not source:
        return None
    lines = Path(source).expanduser().read_text(encoding='utf-8').splitlines()
    return [t.strip().lower() for t in lines if t.strip() and not t.lstrip().startswith('#')]


def text_hits(text, terms=()):
    found = [m.group(0) for m in EMAIL.finditer(text) if not ALLOWED_EMAIL.search(m.group(0))]
    found += [m.group(0) for m in LOCAL_PATH.finditer(text)]
    found += [m.group(0)[:12] + '...' for m in CREDENTIAL.finditer(text)]
    found += banned_hits(text)
    lowered = text.lower()
    found += [f'private term #{i + 1}' for i, term in enumerate(terms) if term in lowered]
    return found


def png_metadata(data):
    if not data.startswith(PNG_SIGNATURE):
        return ['not a PNG']
    extra, offset, ended = [], len(PNG_SIGNATURE), False
    while offset + 8 <= len(data):
        length, kind = struct.unpack('>I4s', data[offset:offset + 8])
        if offset + 12 + length > len(data):
            return extra + [f'{kind.decode("latin-1")} chunk runs past the end']
        if kind not in PNG_PIXEL_CHUNKS:
            extra.append(kind.decode('latin-1'))
        offset += 12 + length
        if kind == b'IEND':
            ended = length == 0
            break
    if not ended:
        extra.append('no IEND')
    elif offset < len(data):
        extra.append(f'{len(data) - offset} bytes after IEND')
    return extra


def jpeg_metadata(data):
    if not data.startswith(b'\xff\xd8'):
        return ['not a JPEG']
    extra, offset = [], 2
    while offset + 4 <= len(data):
        if data[offset] != 0xFF:
            return extra + ['broken segment structure']
        marker = data[offset + 1]
        if marker == 0xDA:
            return extra
        length = struct.unpack('>H', data[offset + 2:offset + 4])[0]
        if marker not in JPEG_PIXEL_MARKERS:
            extra.append(f'segment 0x{marker:02X}')
        offset += 2 + length
    return extra + ['no scan']


def webp_metadata(data):
    if data[:4] != b'RIFF' or data[8:12] != b'WEBP':
        return ['not a WebP']
    extra, offset = [], 12
    while offset + 8 <= len(data):
        kind, size = data[offset:offset + 4], struct.unpack('<I', data[offset + 4:offset + 8])[0]
        if kind not in WEBP_PIXEL_CHUNKS:
            extra.append(kind.decode('latin-1'))
        offset += 8 + size + (size & 1)
    return extra


def wav_metadata(data):
    if data[:4] != b'RIFF' or data[8:12] != b'WAVE':
        return ['not a WAV']
    extra, offset = [], 12
    while offset + 8 <= len(data):
        kind, size = data[offset:offset + 4], struct.unpack('<I', data[offset + 4:offset + 8])[0]
        if kind not in WAV_PIXEL_CHUNKS:
            extra.append(kind.decode('latin-1'))
        offset += 8 + size + (size & 1)
    return extra


def gif_metadata(data):
    if data[:6] not in (b'GIF87a', b'GIF89a'):
        return ['not a GIF']
    return ['comment extension'] if b'\x21\xfe' in data else []


def file_hits(path, terms=()):
    rel = path.relative_to(ROOT).as_posix() if path.is_relative_to(ROOT) else path.name
    if path.suffix.lower() == '.wav' and rel.startswith(AUDIO_DIR):
        return wav_metadata(path.read_bytes())
    if path.suffix.lower() in FORBIDDEN_SUFFIXES:
        return [f'{path.suffix} files are not published here']
    hits = [f'file name "{path.name}"'] if FORBIDDEN_NAMES.search(path.name) else []
    hits += [f'file name: {b}' for b in banned_hits(path.relative_to(ROOT).as_posix() if path.is_relative_to(ROOT) else path.name)]
    data = path.read_bytes()
    reader = {'.png': png_metadata, '.jpg': jpeg_metadata, '.jpeg': jpeg_metadata, '.webp': webp_metadata,
              '.gif': gif_metadata}.get(path.suffix.lower())
    if reader:
        return hits + reader(data)
    try:
        text = data.decode('utf-8')
    except UnicodeDecodeError:
        return hits + [f'unreviewed binary file ({path.suffix or "no suffix"})']
    if '\0' in text:
        return hits + ['unreviewed binary file (NUL bytes)']
    return hits + text_hits(text, terms)


def test_scanner_catches_each_kind_of_leak():
    for leak in ('mail me at someone.private@gmail.com', 'see /Users/someone/project', 'cd /home/someone/game',
                 'C:\\Users\\someone\\game', 'log in /private/tmp/session', 'API_KEY = "abcdefghijklmnop1234"',
                 'token: ghp_abcdefghijklmnopqrstuvwxyz0123', '-----BEGIN OPENSSH PRIVATE KEY-----'):
        assert text_hits(leak), leak
    for fine in ('contact@example.com', 'VFX: KanaWorks_AI (@KanaWorks_AI)', 'res://addons/kanaworks_sakuga_vfx/x.tres',
                 'https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx', 'python@3.12', 'a fire_impact_01 sheet'):
        assert not text_hits(fine), fine
    assert text_hits('Private Name here', ['private name']) == ['private term #1']
    assert h('example') not in BANNED and len(BANNED) >= 20
    clean = PNG_SIGNATURE + struct.pack('>I4s', 0, b'IEND') + b'\0' * 4
    tagged = PNG_SIGNATURE + struct.pack('>I4s', 4, b'tEXt') + b'Name' + b'\0' * 4 + struct.pack('>I4s', 0, b'IEND') + b'\0' * 4
    assert png_metadata(clean) == [] and png_metadata(tagged) == ['tEXt'] and png_metadata(clean + b'x')
    webp = b'RIFF' + struct.pack('<I', 20) + b'WEBP' + b'EXIF' + struct.pack('<I', 2) + b'xx'
    assert webp_metadata(webp) == ['EXIF']
    fmt = struct.pack('<4sIHHIIHH', b'fmt ', 16, 1, 1, 8000, 16000, 2, 16)
    wav = b'RIFF' + struct.pack('<I', 4 + len(fmt) + 8) + b'WAVE' + fmt + b'data' + struct.pack('<I', 0)
    tagged_wav = wav + b'LIST' + struct.pack('<I', 4) + b'INFO'
    assert wav_metadata(wav) == [] and wav_metadata(tagged_wav) == ['LIST']


def test_no_published_file_leaks_personal_machine_or_secret_data():
    checked = [p for p in files() if p.resolve() != THIS]
    assert any(p.suffix == '.png' for p in checked) and any(p.suffix == '.webp' for p in checked)
    assert any(p.name == 'LICENSE.md' for p in checked)
    terms = private_terms() or []
    leaks = {p.relative_to(ROOT).as_posix(): found for p in checked if (found := file_hits(p, terms))}
    assert not leaks, f'private or unreviewed content: {leaks}'


def test_maintainer_private_terms_when_configured():
    terms = private_terms()
    if terms is None:
        pytest.skip('set SAKUGA_PRIVATE_TERMS to a term list kept outside the repository')
    assert terms, 'the private term list is empty'
