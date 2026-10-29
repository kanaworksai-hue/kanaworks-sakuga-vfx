# KanaWorks SAKUGA VFX

<div align="center">

<!-- TODO(banner): docs/assets/banner.jpg is being made in a separate session. Keep this path. -->
<img src="docs/assets/banner.jpg" alt="KanaWorks SAKUGA VFX: free hand-drawn anime-style game VFX flipbooks" width="820">

<p>
  <a href="LICENSE.md"><img alt="Assets: KanaWorks_AI VFX Asset License" src="https://img.shields.io/badge/assets-KanaWorks__AI_VFX_License-ff6aa2?style=flat-square"></a>
  <a href="LICENSE.md#mit-license-code"><img alt="Code: MIT" src="https://img.shields.io/badge/code-MIT-6b4dff?style=flat-square"></a>
  <img alt="Unity 6 URP" src="https://img.shields.io/badge/Unity_6-URP-111?style=flat-square&logo=unity">
  <img alt="Godot 4" src="https://img.shields.io/badge/Godot-4-478cbf?style=flat-square&logo=godotengine&logoColor=white">
  <img alt="Web" src="https://img.shields.io/badge/Web-Canvas_·_PixiJS_·_Phaser_·_Three.js-19b8d6?style=flat-square">
  <a href="https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/stargazers"><img alt="GitHub stars" src="https://img.shields.io/github/stars/kanaworksai-hue/kanaworks-sakuga-vfx?style=flat-square&logo=github&label=stars&color=f5c518"></a>
</p>

<p><a href="https://github.com/kanaworksai-hue"><img src="docs/brand/kanaworks-mark.png" height="22" alt="" align="absmiddle"></a>&nbsp;<a href="https://github.com/kanaworksai-hue"><b>A KanaWorks_AI project</b></a></p>

English | [简体中文](README.zh-CN.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md)

</div>

**A free library of hand-drawn, anime-style (sakuga) game effects**: fire, water, thunder, wind, poison and more, as
transparent flipbook sprite sheets that drop into Unity, Godot, the web or any engine that can play a sprite sheet.

It is made for people who want good-looking skill effects but can't make them with AI tools or don't have the budget
for a VFX artist: students, solo developers, small studios, game jams, video makers. Download, credit, ship.

- **Ready to play**: every effect is a straight-alpha PNG atlas (2048 px, with transparent gutters so frames never
  bleed) plus a small `.flipbook.json` that says its grid, frame count, fps, loop and pivot.
- **Every engine**: a Unity 6 URP package (shader, player component, importer that makes a prefab per effect, demo
  scene), a Godot 4 addon (SpriteFrames per effect, 2D and 3D players, demo scenes), a 4 KB web player and examples
  for PixiJS, Phaser 3 and Three.js, and TexturePacker-style JSON for everything else.
- **Variants**: 2048 and 1024 sheets, each also as an additive version on black.
- **Free, with a credit**: free for individuals and companies under USD 1M annual revenue, commercial use included.
  See [License](#license).

<div align="center">
<img src="docs/assets/overview.webp" alt="All effects playing" width="820">
</div>

**Gallery with live preview and downloads:** https://kanaworksai-hue.github.io/kanaworks-sakuga-vfx/
· **Downloads:** [latest release](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)

## Effects

<!-- assets:start -->
| Preview | ID | Element | Form | Frames | Loop | Added |
|:---:|---|---|---|---:|:---:|---|
| <img src="assets/water/water_column_01/preview.webp" width="72" alt="water_column_01"> | [`water_column_01`](assets/water/water_column_01/) | water | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/water/water_impact_01/preview.webp" width="72" alt="water_impact_01"> | [`water_impact_01`](assets/water/water_impact_01/) | water | impact | 45 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_projectile_01/preview.webp" width="72" alt="fire_projectile_01"> | [`fire_projectile_01`](assets/fire/fire_projectile_01/) | fire | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/fire/fire_impact_01/preview.webp" width="72" alt="fire_impact_01"> | [`fire_impact_01`](assets/fire/fire_impact_01/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_01/preview.webp" width="72" alt="thunder_impact_01"> | [`thunder_impact_01`](assets/thunder/thunder_impact_01/) | thunder | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_01/preview.webp" width="72" alt="wind_vortex_01"> | [`wind_vortex_01`](assets/wind/wind_vortex_01/) | wind | vortex | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_01/preview.webp" width="72" alt="poison_impact_01"> | [`poison_impact_01`](assets/poison/poison_impact_01/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_impact_01/preview.webp" width="72" alt="dark_impact_01"> | [`dark_impact_01`](assets/dark/dark_impact_01/) | dark | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_projectile_01/preview.webp" width="72" alt="dark_projectile_01"> | [`dark_projectile_01`](assets/dark/dark_projectile_01/) | dark | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/ice/ice_impact_01/preview.webp" width="72" alt="ice_impact_01"> | [`ice_impact_01`](assets/ice/ice_impact_01/) | ice | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_projectile_01/preview.webp" width="72" alt="ice_projectile_01"> | [`ice_projectile_01`](assets/ice/ice_projectile_01/) | ice | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/light/light_column_01/preview.webp" width="72" alt="light_column_01"> | [`light_column_01`](assets/light/light_column_01/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_impact_01/preview.webp" width="72" alt="light_impact_01"> | [`light_impact_01`](assets/light/light_impact_01/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_projectile_01/preview.webp" width="72" alt="poison_projectile_01"> | [`poison_projectile_01`](assets/poison/poison_projectile_01/) | poison | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/rock/rock_column_01/preview.webp" width="72" alt="rock_column_01"> | [`rock_column_01`](assets/rock/rock_column_01/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_01/preview.webp" width="72" alt="rock_impact_01"> | [`rock_impact_01`](assets/rock/rock_impact_01/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_01/preview.webp" width="72" alt="thunder_strike_01"> | [`thunder_strike_01`](assets/thunder/thunder_strike_01/) | thunder | strike | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_01/preview.webp" width="72" alt="wind_impact_01"> | [`wind_impact_01`](assets/wind/wind_impact_01/) | wind | impact | 41 (8x8) | no | 2026-10-29 |
<!-- assets:end -->

Forms: `projectile` loops while it flies; `impact` plays once at the hit; `column` stands on the ground and rises;
`vortex` swirls in place; `strike` comes down from above onto the ground. Names are `element_form_NN`.

## Downloads

Each [release](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest) has:

| File | For |
|---|---|
| `kanaworks-sakuga-vfx-<version>-unity.zip` | Unity 6 (URP): unzip into your project folder |
| `kanaworks-sakuga-vfx-<version>-godot.zip` | Godot 4.3+: unzip into your project folder |
| `kanaworks-sakuga-vfx-<version>-web.zip` | the web player, examples and gallery, with every sheet |
| `kanaworks-sakuga-vfx-<version>-flipbooks.zip` | every sheet in every variant, for any engine |
| `assets/<id>.zip` (on the gallery) | one effect, all variants |

You can also take a single `assets/<element>/<id>/<id>.png` and its `.flipbook.json` straight from this repository.

## Quick start

### Unity 6 (URP)

1. Unzip `...-unity.zip` into your project folder. It adds `Packages/com.kanaworks.sakuga-vfx/` and
   `Assets/KanaWorksSakugaVFX/Flipbooks/`.
2. Unity imports the sheets with the right settings and makes `<id>.prefab` next to each one.
   **Tools > KanaWorks SAKUGA VFX > Build demo scene**, then Play.
3. In code:

```csharp
public SakugaFlipbook fireHit;                                    // drag fire_impact_01.prefab here
SakugaFlipbook.Spawn(fireHit, hitPoint, Quaternion.identity);     // plays once, then destroys itself
```

Shader `KanaWorks/SakugaFlipbook`: alpha blend or additive, camera-facing or upright billboard (columns stay
vertical with their root on the ground), HDR tint for Bloom. For a Particle System use `M_<id>_Particles.mat` with
Texture Sheet Animation set to the sheet's grid.

### Godot 4.3+

1. Unzip `...-godot.zip` into your project folder (adds `addons/kanaworks_sakuga_vfx/` and `sakuga_demo/`).
2. Open `res://sakuga_demo/demo_2d.tscn` or `demo_3d.tscn` and press F6.
3. In code:

```gdscript
const FIRE_HIT := preload("res://addons/kanaworks_sakuga_vfx/flipbooks/fire/fire_impact_01/fire_impact_01.tres")
SakugaFlipbook2D.spawn(FIRE_HIT, self, hit_position)     # 2D (AnimatedSprite2D), frees itself when done
SakugaFlipbook3D.spawn(FIRE_HIT, self, hit_point)        # 3D billboard, same .tres
```

Each `.tres` is a normal `SpriteFrames`, so it also works in any `AnimatedSprite2D`.

### Web

```html
<canvas id="fx" width="512" height="512"></canvas>
<script type="module">
  import { load, mount } from './web/sakuga-player.js';
  mount(document.getElementById('fx'), await load('./assets/fire/fire_impact_01/fire_impact_01.flipbook.json'));
</script>
```

`sakuga-player.js` is dependency-free (Canvas2D, about 4 KB): `load()`, `Flipbook` (`update(dt)`, `draw(ctx, x, y)`),
`mount()` and `spritesheetData()` for PixiJS and Phaser. Examples: [`web/examples/`](web/examples/) (Canvas2D,
PixiJS 8, Phaser 3, Three.js; libraries load from a CDN with pinned versions). Serve the folder over HTTP
(for example `python -m http.server`) and open `web/`.

### Any other engine

Read the sidecar and play the grid left to right, top to bottom:

| Field | Meaning |
|---|---|
| `grid` | `[cols, rows]` |
| `cell` | `[width, height]` of one frame in pixels |
| `frames` | frames to play (the first `frames` cells) |
| `fps`, `loop` | playback rate (30) and whether it loops (projectiles) |
| `blend` | `alpha` (straight alpha) or `additive` |
| `pivot` | `[x, y]` in the cell, 0-1 from the top-left: the centre, or the root of a ground column |
| `bottom_inset` | empty space under a column's root, as a fraction of the cell height |
| `source` | the PNG next to it |

The release also has `<id>.atlas.json` (TexturePacker "JSON Hash" with an `animations` list) for tools that read it.

## Credit

Put this wherever you use or repost the effects (game credits, video description, page footer):

```
VFX: KanaWorks_AI (@KanaWorks_AI)
```

When you repost the effects or their previews, keep the credit and a link to
https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx.

## License

This is a **free VFX library**, not an open-source project: the effects are free to use with conditions.
Summary (the [full license](LICENSE.md) applies, and its Japanese text governs):

- **Free** for personal, non-commercial and commercial use by individuals and companies whose annual gross revenue
  (previous fiscal year, affiliates included) is **under USD 1,000,000**. At or above that, contact
  [KanaWorks_AI](https://x.com/KanaWorks_AI) for a separate license.
- **Credit required** in every use: `VFX: KanaWorks_AI (@KanaWorks_AI)`. Reposts keep the credit and the repository link.
- **Allowed**: modifying the effects inside your own work (recolor, retime, crop, composite); training AI models.
- **Not allowed**: redistributing or selling the effects themselves as an asset pack (as they are or lightly
  modified, including asset stores); claiming you made them; implying KanaWorks_AI endorses you.
- **No warranty, no support, no liability**: a free gift, as is.
- The code (`tools/`, `unity/`, `godot/`, `web/`) is **MIT**. The KanaWorks_AI name and logo are not licensed
  ([docs/brand](docs/brand/README.md)).

## Requests

Want an element, a form or an engine that isn't here? Open an
[asset request](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/issues/new?template=asset-request.yml):
element, form, engine, what it's for, and a description in words. Please don't attach screenshots or clips of other
people's work.

The library may be updated if people like it. There is no schedule and no promise that a request will be made.

## Make your own

Want effects in your own style? The sister project
[**effekseer-vfx-skill**](https://github.com/kanaworksai-hue/effekseer-vfx-skill) is an agent skill that lets a
coding agent make real-time game VFX for Effekseer, Godot and Unity, including flipbooks like these.

## For maintainers

Tools need Python 3.10+ with Pillow and NumPy (`pip install -r tools/requirements.txt`).

**Add an effect** (a finished transparent, straight-alpha PNG atlas, frames left to right, top to bottom):

```sh
python tools/sakuga.py add path/to/sheet.png --id ice_impact_01 --grid 8 8   # copies it in, strips metadata, adds the catalog line
python tools/sakuga.py build      # preview.webp, overview, README tables, dist/ zips and site
python -m pytest -q               # validation, privacy and package tests
```

Or by hand: put the sheet at `assets/<element>/<id>/<id>.png`, add one line to `catalog.json`
(`{"id": "ice_impact_01", "element": "ice", "form": "impact", "grid": [8, 8], "added": "YYYY-MM-DD", "version": "1.0.0"}`),
then run `python tools/sakuga.py sync` (measures cell, frames and `bottom_inset`, writes the sidecar) and `build`.
Options: `--fps`, `--loop true|false`, `--blend additive`. `python tools/sakuga.py validate` checks everything.

`python tools/sakuga.py install unity <UnityProject>` / `install godot <GodotProject>` copies the package and the
sheets into a project for testing. CI runs the tests on every push; a `v*` tag builds the zips into a GitHub release
and publishes the gallery to GitHub Pages.

```
assets/<element>/<id>/   <id>.png, <id>.flipbook.json, preview.webp
catalog.json             one line per effect
tools/                   validate, sync, add, build, install (MIT)
unity/                   UPM package com.kanaworks.sakuga-vfx (MIT)
godot/                   addon + demo scenes (MIT)
web/                     player, gallery, examples (MIT)
tests/                   pytest
```

<br>

<div align="center">

<a href="https://x.com/KanaWorks_AI"><img src="docs/brand/kanaworks-logo.png" width="130" alt="KanaWorks_AI"></a>

<p><b>Made with ♥ by <a href="https://x.com/KanaWorks_AI">KanaWorks_AI</a></b> · <a href="https://x.com/KanaWorks_AI">@KanaWorks_AI</a> on X</p>

<p><sub>If the effects help your game, a ⭐ helps others find them. More projects: <a href="https://github.com/kanaworksai-hue">github.com/kanaworksai-hue</a></sub></p>

<p><sub>Unity, Godot, PixiJS, Phaser and Three.js are trademarks of their respective owners. This is an independent project, not affiliated with or endorsed by them.</sub></p>

</div>
