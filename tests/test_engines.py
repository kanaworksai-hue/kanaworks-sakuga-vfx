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
    compile_(tmp_path / 'ed.dll', sorted((UNITY / 'Editor').glob('*.cs')), [f'-r:{tmp_path / "rt.dll"}'])
