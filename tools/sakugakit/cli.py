"""Command line. All console output is ASCII so any Windows code page can print it."""
import argparse
import sys
from pathlib import Path

from .common import ROOT, say


def _cmd_validate(args):
    from .catalog import validate
    from .shared import check
    errors, warnings = validate(args.root)
    shared_errors, shared_warnings = check(args.root)
    errors, warnings = errors + shared_errors, warnings + shared_warnings
    for w in warnings:
        say('warning:', w)
    for e in errors:
        say('error:', e)
    say(f'{len(errors)} error(s), {len(warnings)} warning(s)')
    return 1 if errors else 0


def _cmd_sync(args):
    from .catalog import sync
    changed = sync(args.root)
    for c in changed:
        say('  ' + c)
    say('sync: nothing to change' if not changed else f'sync: {len(changed)} change(s)')
    return 0


def _cmd_add(args):
    from .catalog import add
    loop = None if args.loop is None else args.loop == 'true'
    png = add(args.sheet, args.id, args.grid, root=args.root, fps=args.fps, loop=loop, blend=args.blend,
              added=args.added, version=args.version, force=args.force)
    say(f'added {args.id}: {Path(png).relative_to(Path(args.root).resolve()).as_posix()}')
    say('next: python tools/sakuga.py build   (previews, README tables, packages)')
    return 0


def _cmd_build(args):
    from .build import build
    res = build(args.root, out=args.out, previews=not args.no_previews, readmes=not args.no_readme, site=not args.no_site)
    for w in res['warnings']:
        if 'preview' not in w:
            say('warning:', w)
    say(f'build: ok, {len(res["assets"])} asset(s) -> {res["out"]}')
    return 0


def _cmd_install(args):
    from .build import install_godot, install_unity
    dest = (install_unity if args.engine == 'unity' else install_godot)(args.project, root=args.root)
    say(f'installed into {Path(dest).name}')
    return 0


def _cmd_shared(args):
    from .shared import import_shared, refresh
    if args.refresh or not args.folder:
        refresh(args.root)
        say('shared: assets/_shared/shared.json rewritten')
        return 0
    done = import_shared(args.folder, args.root)
    for d in done:
        say('  ' + d)
    say(f'shared: {len(done)} file(s) copied into assets/_shared')
    say('next: python tools/sakuga.py validate')
    return 0


def _cmd_suggest(args):
    import json as _json
    from .common import load_catalog
    from .suggest import suggest
    cat = load_catalog(Path(args.root) / 'catalog.json')
    ranked, chain = suggest(cat, ' '.join(args.text), limit=args.limit)
    if args.json:
        say(_json.dumps({'results': [{'id': a['id'], 'score': round(s, 2), 'category': a.get('category'),
                                      'style': a.get('style'), 'pairs': a.get('pairs'),
                                      'use_cases': a.get('use_cases', [])[:3]} for s, a in ranked],
                         'chain': chain}, ensure_ascii=True))
        return 0 if ranked else 1
    if not ranked:
        say('no match: try an element (fire, ice, ...) and a shape (projectile, hit, pillar, area, shield, ...)')
        return 1
    for s, a in ranked:
        uses = ', '.join(a.get('use_cases', [])[:3])
        say(f'{s:5.1f}  {a["id"]:<22} {a.get("category", a["form"]):<10} {a.get("style", "classic"):<9} {uses}')
    if chain:
        say('chain: ' + ' -> '.join(chain))
    return 0


def parser():
    p = argparse.ArgumentParser(prog='sakuga', description='KanaWorks SAKUGA VFX library tools')
    p.add_argument('--root', type=Path, default=ROOT, help='library root (default: this repository)')
    sub = p.add_subparsers(dest='cmd', required=True)
    sub.add_parser('validate', help='check catalog.json, sheets and sidecars').set_defaults(func=_cmd_validate)
    sub.add_parser('sync', help='measure sheets, fill derived catalog fields, strip metadata, write sidecars'
                   ).set_defaults(func=_cmd_sync)
    a = sub.add_parser('add', help='copy a finished sheet in and add its catalog line')
    a.add_argument('sheet', type=Path, help='transparent straight-alpha PNG atlas')
    a.add_argument('--id', required=True, help='element_form_NN, e.g. ice_impact_01')
    a.add_argument('--grid', required=True, nargs=2, type=int, metavar=('COLS', 'ROWS'))
    a.add_argument('--fps', type=int)
    a.add_argument('--loop', choices=('true', 'false'))
    a.add_argument('--blend', choices=('alpha', 'additive'))
    a.add_argument('--added', help='YYYY-MM-DD (default: today)')
    a.add_argument('--version', default='1.0.0')
    a.add_argument('--force', action='store_true', help='replace an asset that is already in the catalog')
    a.set_defaults(func=_cmd_add)
    b = sub.add_parser('build', help='previews, README tables, derived sheets, engine zips and the Pages site')
    b.add_argument('--out', type=Path, help='output folder (default: dist/)')
    b.add_argument('--no-previews', action='store_true', help='keep existing preview.webp and overview.webp')
    b.add_argument('--no-readme', action='store_true', help='do not touch the README asset tables')
    b.add_argument('--no-site', action='store_true')
    b.set_defaults(func=_cmd_build)
    sh = sub.add_parser('shared', help='copy shared textures, decals and sounds into assets/_shared')
    sh.add_argument('folder', type=Path, nargs='?', help='folder with T_*.png, decals/ and sfx/ (or all flat)')
    sh.add_argument('--refresh', action='store_true', help='only rewrite assets/_shared/shared.json')
    sh.set_defaults(func=_cmd_shared)
    sg = sub.add_parser('suggest', help='rank effects for a request, e.g. suggest "ice arrow hitting a slime"')
    sg.add_argument('text', nargs='+', help='what the effect is for, in plain English')
    sg.add_argument('--limit', type=int, default=5)
    sg.add_argument('--json', action='store_true', help='machine-readable output (ASCII JSON)')
    sg.set_defaults(func=_cmd_suggest)
    i = sub.add_parser('install', help='copy the package and flipbooks into a Unity or Godot project')
    i.add_argument('engine', choices=('unity', 'godot'))
    i.add_argument('project', type=Path)
    i.set_defaults(func=_cmd_install)
    return p


def main(argv=None):
    args = parser().parse_args(argv)
    args.root = Path(args.root).resolve()
    try:
        return args.func(args)
    except (ValueError, FileNotFoundError) as exc:
        say(f'error: {exc}')
        return 2
    except SystemExit as exc:
        if isinstance(exc.code, str):
            say(f'error: {exc.code}')
            return 2
        raise


if __name__ == '__main__':
    sys.exit(main())
