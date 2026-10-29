"""The command line prints ASCII only, so a Windows console on cp1252 (or any code page) never crashes."""
import os
import subprocess
import sys

import pytest

from conftest import ROOT, sheet

CLI = [sys.executable, str(ROOT / 'tools' / 'sakuga.py')]


def run(args, **kw):
    env = dict(os.environ, PYTHONIOENCODING='cp1252', PYTHONUTF8='0')
    return subprocess.run(CLI + args, capture_output=True, env=env, **kw)


def ascii_text(b):
    return b.decode('ascii')       # raises if anything outside ASCII was printed


@pytest.mark.parametrize('args', [['--help'], ['validate', '--help'], ['add', '--help'], ['build', '--help'],
                                  ['sync', '--help'], ['install', '--help']])
def test_help_is_ascii(args):
    r = run(args)
    assert r.returncode == 0
    ascii_text(r.stdout)


def test_validate_on_the_repository_is_ascii_and_clean():
    r = run(['validate'])
    assert r.returncode == 0, r.stdout
    assert '0 error(s)' in ascii_text(r.stdout)


def test_errors_are_ascii_and_exit_2(library, tmp_path):
    src = tmp_path / 'x.png'
    sheet().save(src)
    r = run(['--root', str(library), 'add', str(src), '--id', 'fire_impact_01', '--grid', '3', '2'])
    assert r.returncode == 2
    assert 'error:' in ascii_text(r.stdout)
    r = run(['--root', str(library), 'install', 'unity', str(tmp_path)])
    assert r.returncode == 2 and 'not a Unity project' in ascii_text(r.stdout)


def test_add_sync_validate_via_cli(library, tmp_path):
    src = tmp_path / 'x.png'
    sheet().save(src)
    r = run(['--root', str(library), 'add', str(src), '--id', 'ice_impact_01', '--grid', '4', '2', '--added', '2026-01-02'])
    assert r.returncode == 0, r.stdout
    ascii_text(r.stdout)
    assert run(['--root', str(library), 'sync']).returncode == 0
    r = run(['--root', str(library), 'validate'])
    assert r.returncode == 0 and '0 error(s)' in ascii_text(r.stdout)


def test_install_into_engine_projects(library, tmp_path):
    from sakugakit.build import install_godot, install_unity
    from sakugakit.catalog import add
    src = tmp_path / 'x.png'
    sheet().save(src)
    add(src, 'ice_impact_01', (4, 2), root=library, added='2026-01-02')
    u = tmp_path / 'U'
    (u / 'Assets').mkdir(parents=True)
    (u / 'ProjectSettings').mkdir()
    meta = u / 'Packages/com.kanaworks.sakuga-vfx/Runtime/SakugaFlipbook.cs.meta'
    meta.parent.mkdir(parents=True)
    meta.write_text('guid: keepme', encoding='utf-8')
    install_unity(u, root=library)
    assert (u / 'Assets/KanaWorksSakugaVFX/Flipbooks/ice/ice_impact_01/ice_impact_01.png').is_file()
    assert meta.read_text(encoding='utf-8') == 'guid: keepme'        # GUIDs survive an update
    g = tmp_path / 'G'
    g.mkdir()
    (g / 'project.godot').write_text('config_version=5\n', encoding='utf-8')
    install_godot(g, root=library)
    assert (g / 'addons/kanaworks_sakuga_vfx/flipbooks/ice/ice_impact_01/ice_impact_01.tres').is_file()
