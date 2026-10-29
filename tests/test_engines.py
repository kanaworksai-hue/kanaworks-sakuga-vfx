import json
import os
import re
import shutil
import subprocess

import pytest

from conftest import ROOT

UNITY = ROOT / 'unity' / 'com.kanaworks.sakuga-vfx'
GODOT = ROOT / 'godot'


def test_unity_package_manifest_and_assemblies():
    pkg = json.loads((UNITY / 'package.json').read_text(encoding='utf-8'))
    assert pkg['name'] == 'com.kanaworks.sakuga-vfx' and pkg['unity'] == '6000.0' and pkg['license'] == 'MIT'
    assert 'com.unity.render-pipelines.universal' in pkg['dependencies']
    for asmdef in UNITY.rglob('*.asmdef'):
        data = json.loads(asmdef.read_text(encoding='utf-8'))
        assert data['name'].startswith('KanaWorks.SakugaVFX')
    shader = (UNITY / 'Runtime/Shaders/SakugaFlipbook.shader').read_text(encoding='utf-8')
    name = re.search(r'Shader "([^"]+)"', shader).group(1)
    importer = (UNITY / 'Editor/SakugaImporter.cs').read_text(encoding='utf-8')
    assert f'"{name}"' in importer
    for prop in ('_MainTex', '_Grid', '_Frame', '_Additive', '_Billboard'):
        assert prop in shader and prop in importer + (UNITY / 'Runtime/SakugaFlipbook.cs').read_text(encoding='utf-8')
    assert 'Blend One OneMinusSrcAlpha' in shader and '"RenderPipeline" = "UniversalPipeline"' in shader


def test_unity_import_rules_follow_the_skill():
    src = (UNITY / 'Editor/SakugaImporter.cs').read_text(encoding='utf-8')
    for rule in ('mipmapEnabled = false', 'TextureImporterNPOTScale.None', 'maxTextureSize = 4096', 'maxTextureSize = 2048',
                 'TextureImporterFormat.ASTC_6x6', 'TextureWrapMode.Clamp', 'Path.ChangeExtension(assetPath, SidecarSuffix)'):
        assert rule in src, rule


def test_godot_files_reference_each_other():
    for tscn in GODOT.rglob('*.tscn'):
        for path in re.findall(r'path="res://([^"]+)"', tscn.read_text(encoding='utf-8')):
            assert (GODOT / path).is_file(), f'{tscn.name} -> {path}'
    for gd in GODOT.rglob('*.gd'):
        for path in re.findall(r'preload\("res://([^"]+)"\)', gd.read_text(encoding='utf-8')):
            assert (GODOT / path).is_file(), f'{gd.name} -> {path}'
    shader = (GODOT / 'addons/kanaworks_sakuga_vfx/sakuga_flipbook_3d.gdshader').read_text(encoding='utf-8')
    script = (GODOT / 'addons/kanaworks_sakuga_vfx/sakuga_flipbook_3d.gd').read_text(encoding='utf-8')
    for uniform in re.findall(r'uniform \w+ (\w+)', shader):
        if uniform != 'alpha_boost':
            assert f'"{uniform}"' in script, uniform


@pytest.mark.skipif(not shutil.which('godot'), reason='godot not installed')
def test_godot_headless_loads_every_flipbook_and_runs_the_demos(tmp_path):
    from sakugakit.build import stage_godot
    from sakugakit.common import load_catalog, published
    cat = load_catalog(ROOT / 'catalog.json')
    project = tmp_path / 'g'
    stage_godot(ROOT, published(cat), cat['defaults'], project)
    shutil.copyfile(GODOT / 'project.godot', project / 'project.godot')
    shutil.copyfile(ROOT / 'tests/godot/check_addon.gd', project / 'check_addon.gd')
    imp = subprocess.run(['godot', '--headless', '--path', str(project), '--import'], capture_output=True, text=True, timeout=600)
    assert imp.returncode == 0, imp.stderr[-2000:]
    run = subprocess.run(['godot', '--headless', '--path', str(project), '--fixed-fps', '30', '--script', 'res://check_addon.gd'],
                         capture_output=True, text=True, timeout=300)
    out = run.stdout + run.stderr
    assert run.returncode == 0 and 'SAKUGA_GODOT_CHECK OK' in out, out[-3000:]
    assert 'SCRIPT ERROR' not in out and 'Parse Error' not in out, out[-3000:]


UNITY_CONTENTS = os.environ.get('SAKUGA_UNITY_CONTENTS')   # <Unity.app>/Contents (macOS) or <Editor>/Data (Windows)


@pytest.mark.skipif(not UNITY_CONTENTS, reason='set SAKUGA_UNITY_CONTENTS to compile the C# against Unity')
def test_unity_scripts_compile_against_unity(tmp_path):
    from pathlib import Path
    base = Path(UNITY_CONTENTS)
    scripting = base / 'Resources/Scripting' if (base / 'Resources/Scripting').is_dir() else base
    dotnet = next(scripting.glob('DotNetSdk/dotnet*'))
    csc = next(scripting.glob('DotNetSdk/sdk/*/Roslyn/bincore/csc.dll'))
    managed = scripting / 'Managed/UnityEngine'
    ns = next(scripting.glob('NetStandard/ref/*/netstandard.dll'))
    refs = [f'-r:{p}' for p in managed.glob('*.dll')] + [f'-r:{ns}']

    def compile_(out, sources, extra=()):
        cmd = [str(dotnet), str(csc), '-nologo', '-nostdlib', '-noconfig', '-t:library', f'-out:{out}', *refs, *extra,
               *map(str, sources)]
        r = subprocess.run(cmd, capture_output=True, text=True)
        assert r.returncode == 0, r.stdout[-3000:]

    compile_(tmp_path / 'rt.dll', sorted((UNITY / 'Runtime').glob('*.cs')))
    compile_(tmp_path / 'demo.dll', sorted((UNITY / 'Runtime/Demo').glob('*.cs')), [f'-r:{tmp_path / "rt.dll"}'])
    compile_(tmp_path / 'ed.dll', sorted((UNITY / 'Editor').glob('*.cs')),
             [f'-r:{tmp_path / "rt.dll"}', f'-r:{tmp_path / "demo.dll"}'])


def test_unity_layered_api_is_there():
    rt = {p.name: p.read_text(encoding='utf-8') for p in (UNITY / 'Runtime').rglob('*.cs')}
    fx = rt['SakugaFX.cs']
    for sig in ('public static SakugaEffect Play(string id, Vector3 position', 'public static SakugaProjectile Cast(',
                'public static SakugaEffect Attach(', 'public static SakugaEffect Beam(', 'public static void ClearAll()',
                'ShakeRequested', 'HitStopRequested', 'AutoShake', 'AutoHitStop'):
        assert sig in fx, sig
    eff = rt['SakugaEffect.cs']
    for sig in ('public void Simulate(float t', 'public void Play()', 'public void Stop()', 'event Action<SakugaEffect> Finished',
                'ShakeRequested', 'public SakugaSize size', 'public bool overrideColor', 'public float speed'):
        assert sig in eff, sig
    assert 'beginCameraRendering' in rt['SakugaCameraShake.cs'] and 'endCameraRendering' in rt['SakugaCameraShake.cs']
    assert 'interface ISakugaStepper' in eff and 'ISakugaStepper' in rt['SakugaProjectile.cs']
    shader = (UNITY / 'Runtime/Shaders/SakugaFlipbook.shader').read_text(encoding='utf-8')
    for prop in ('_SoftFade', '_CoreBoost', '_CamPush', '_Recolor', '_InstanceColor', '_SakugaDepthReady'):
        assert prop in shader, prop
    demo = json.loads((UNITY / 'Runtime/Demo/KanaWorks.SakugaVFX.Demo.asmdef').read_text(encoding='utf-8'))
    assert demo['versionDefines'][0]['define'] == 'SAKUGA_INPUT_SYSTEM'
    ed = (UNITY / 'Editor/SakugaDemoBuilder.cs').read_text(encoding='utf-8')
    for scene in ('Sakuga_Arena', 'Sakuga_TopDown', 'Sakuga_TowerDefense', 'Sakuga_Gallery'):
        assert scene in ed
    # the runtime reaches URP only through reflection, so it compiles without a URP reference
    assert 'Unity.RenderPipelines' not in json.loads((UNITY / 'Runtime/KanaWorks.SakugaVFX.asmdef').read_text())['references']


def test_staged_engines_carry_shared_assets_and_category_scenes(tmp_path):
    from sakugakit.build import stage_godot, stage_unity
    from sakugakit.common import load_catalog, published
    cat = load_catalog(ROOT / 'catalog.json')
    assets = [a for a in published(cat) if a['id'] in ('fire_impact_01', 'fire_projectile_01', 'water_shield_01')]
    u = stage_unity(ROOT, assets, cat['defaults'], tmp_path / 'u')
    assert (u / 'Assets/KanaWorksSakugaVFX/Shared/textures/T_Flash.png').is_file()
    assert (u / 'Assets/KanaWorksSakugaVFX/Shared/shared.json').is_file()
    g = stage_godot(ROOT, assets, cat['defaults'], tmp_path / 'g')
    scenes = sorted(p.name for p in (g / 'addons/kanaworks_sakuga_vfx/effects').glob('*.tscn'))
    assert scenes == ['sakuga_impact.tscn', 'sakuga_projectile.tscn', 'sakuga_shield.tscn']
    idx = json.loads((g / 'addons/kanaworks_sakuga_vfx/flipbooks/index.json').read_text(encoding='utf-8'))
    assert all(a['sidecar'].endswith('.flipbook.json') for a in idx['assets'])
    assert (g / 'addons/kanaworks_sakuga_vfx/shared/textures/T_Ring.png').is_file()
