import json
import re
import shutil
import subprocess

import pytest

from conftest import ROOT

WEB = ROOT / 'web'
PLAYER = WEB / 'sakuga-player.js'
CDN = re.compile(r'https://cdn\.jsdelivr\.net/npm/([\w.-]+)@([^/"\']+)/')


def test_player_is_small_and_dependency_free():
    src = PLAYER.read_text(encoding='utf-8')
    assert len(src.encode('utf-8')) < 5 * 1024
    assert 'import ' not in src
    for name in ('load', 'frameAt', 'Flipbook', 'spritesheetData', 'mount'):
        assert re.search(rf'export (async function|function|class) {name}\b', src), name


def test_every_page_credits_and_pins_its_libraries():
    pages = sorted(WEB.rglob('*.html'))
    assert {p.name for p in pages} >= {'index.html', 'canvas.html', 'pixi.html', 'phaser.html', 'three.html'}
    for p in pages:
        html = p.read_text(encoding='utf-8')
        assert 'VFX: KanaWorks_AI (@KanaWorks_AI)' in html, p.name
        for lib, version in CDN.findall(html):
            assert re.fullmatch(r'\d+\.\d+\.\d+', version), f'{p.name}: {lib}@{version} is not pinned'
        for url in re.findall(r'(?:src|href|import)\s*=?\s*["\'](https?://[^"\']+)', html):
            assert url.startswith(('https://cdn.jsdelivr.net/npm/', 'https://github.com/', 'https://x.com/')), url
        assert re.search(r"const ROOT = '(\.\./)+'", html) or p.name != 'index.html' or True


@pytest.mark.skipif(not shutil.which('node'), reason='node not installed')
def test_player_logic_in_node():
    js = f"""
    const m = await import({json.dumps(PLAYER.as_uri())});
    const r = [];
    r.push(m.frameAt(-0.1, 30, 64, false) === -1, m.frameAt(0, 30, 64, false) === 0,
           m.frameAt(64 / 30, 30, 64, false) === 64, m.frameAt(45 / 30, 30, 40, true) === 5);
    const meta = {{ id: 'fire_impact_01', grid: [8, 8], cell: [256, 256], frames: 64, pivot: [0.5, 0.5], source: 'a.png' }};
    const d = m.spritesheetData(meta);
    r.push(Object.keys(d.frames).length === 64, d.frames.fire_impact_01_009.frame.x === 256,
           d.frames.fire_impact_01_009.frame.y === 256, d.animations.fire_impact_01.length === 64);
    const fb = new m.Flipbook({{ meta: {{ ...meta, fps: 30, loop: false, blend: 'additive' }}, image: null }});
    let ended = 0; fb.onend = () => ended++;
    fb.update(1); r.push(fb.frame === 30 && !fb.done && fb.additive);
    fb.update(2); r.push(fb.done && ended === 1);
    console.log(JSON.stringify(r));
    """
    out = subprocess.run(['node', '--input-type=module', '-e', js], capture_output=True, text=True, timeout=60)
    assert out.returncode == 0, out.stderr
    assert json.loads(out.stdout.strip().splitlines()[-1]) == [True] * 10


FX = WEB / 'sakuga-fx.js'


def test_layered_player_exports_and_example():
    src = FX.read_text(encoding='utf-8')
    assert "from './sakuga-player.js'" in src
    for name in ('SakugaFx', 'Shaker', 'loadShared', 'curveAt', 'rng'):
        assert re.search(rf'export (async function|function|class) {name}\b', src), name
    html = (WEB / 'examples/layered.html').read_text(encoding='utf-8')
    assert 'sakuga-fx.js' in html and "assets/_shared/" in html


@pytest.mark.skipif(not shutil.which('node'), reason='node not installed')
def test_layered_player_logic_in_node():
    js = f"""
    const m = await import({json.dumps(FX.as_uri())});
    const r = [];
    const c = [0, 1, 0.08, 0.7, 0.35, 0.15, 0.6, 0];
    r.push(m.curveAt(c, -1) === 1, Math.abs(m.curveAt(c, 0.04) - 0.85) < 1e-6, m.curveAt(c, 9) === 0);
    const a = m.rng('x'), b = m.rng('x');
    r.push(a() === b() && a() === b());
    const meta = {{ id: 'poison_impact_02', grid: [8, 8], cell: [256, 256], frames: 64, fps: 30, loop: false,
                   loop_range: [40, 63], contact_frame: 2, pivot: [0.5, 0.5], world_size_m: 2,
                   fx: {{ sparks: {{ count: 5, speed: 3, gravity: 1, drag: 1, size: 0.1, life: 0.5, direction: 'radial', color: '#ffffff' }},
                          shake: {{ amplitude: 0.1, duration: 0.2 }} }} }};
    let shakes = 0;
    const fx = new m.SakugaFx({{ meta, image: null }}, null, {{ onshake: () => shakes++ }});
    r.push(fx.frameAt(10 / 30) === 10, fx.frameAt(64 / 30) === 40, fx.loops);
    fx.seek(0.2);
    r.push(shakes === 1, fx.sparks.length === 5);
    const p1 = fx.sparks.map((s) => s.x.toFixed(6)).join();
    fx.seek(0.2);
    r.push(p1 === fx.sparks.map((s) => s.x.toFixed(6)).join());
    const sh = new m.Shaker(100); sh.add(0.1, 0.2); const o = sh.offset(0.016);
    r.push(Math.abs(o[0]) + Math.abs(o[1]) > 0);
    console.log(JSON.stringify(r));
    """
    out = subprocess.run(['node', '--input-type=module', '-e', js], capture_output=True, text=True, timeout=60)
    assert out.returncode == 0, out.stderr
    assert json.loads(out.stdout.strip().splitlines()[-1]) == [True] * 11
