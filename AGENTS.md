# AGENTS.md: using KanaWorks SAKUGA VFX from a coding agent

This file tells a coding agent (or a person in a hurry) how to pick the right effect for a game moment and wire it
into Unity, Godot or the web. Everything here is plain data in this repository; no service or account is needed.

## 1. Find the effect

Every effect has a **recipe** in `catalog.json` (and the same fields in its `<id>.flipbook.json` sidecar):

| Field | Meaning |
|---|---|
| `id` | `element_form_NN`, for example `ice_projectile_02`. `_01` is the classic look, `_02` / `_03` are style variants. |
| `category` | `projectile`, `impact`, `area`, `column`, `strike`, `beam`, `vortex`, `buff`, `shield`, `cast`, `slash`, `aura` |
| `use_cases`, `tags`, `style` | what it is for, in short English phrases (`"ice arrow"`, `"heal pickup"`, `"stun field"`) |
| `pairs` | `{cast, projectile, impact}`: the charge that goes in front, the projectile that carries it, the hit after it |
| `anchor` | `center` (put the origin where the hit lands) or `ground` (put it on the floor) |
| `orientation` | `billboard`, `upright` (stands on the floor), `ground` (lies flat), `travel` (rolled along the flight, +X) |
| `world_size_m`, `duration_s` | height of the drawing in metres at size M; length of one pass in seconds |
| `contact_frame` | the frame where the hit lands: flash, shake, hit stop, sound and the target's reaction go here |
| `loop`, `loop_range` | loops play forever; a one-shot with `loop_range` plays once, then repeats that range (puddles, clouds) |
| `fx` | secondary layers: `flash`, `ring`, `sparks`, `debris`, `smoke`, `decal`, `light`, `trail`, `shake`, `hit_stop_s`, `sfx` |

Ask the catalog in plain English (ASCII output, works on Python 3.9+):

```sh
python tools/sakuga.py suggest "ice arrow hitting a slime"
python tools/sakuga.py suggest --json "tower that zaps enemies"
```

It prints ranked ids and a `chain:` line (for example `ice_charge_01 -> ice_projectile_02 -> ice_impact_01`).
Rules of thumb:

- A spell that flies: the `projectile`, its `pairs.cast` at the hand, its `pairs.impact` at the target.
- A hit with no flight (melee, instant spell): an `impact` or a `slash` at the contact point.
- Something from the sky or the floor: `strike` or `column` at the target's feet.
- A zone that stays: `area` (often looping), `vortex`.
- On a character: `shield` (around the body), `aura` (at the feet), `buff` (a one-shot gain).
- Heavy hits: styles `meteor`, `tidal`, `judgement`, `stone`. Healing: `spring`, `blessing`. Debuffs: `miasma`, `crimson`.

Effects are tuned for a character about 1.8 m tall. Size presets: S 0.6, M 1.0, L 1.6 (`recipes.SIZE_PRESETS`).

## 2. Unity 6 (URP)

Install: `python tools/sakuga.py install unity <project>` (or unzip the Unity download). The importer builds, next to
each sheet, `<id>.prefab` (just the flipbook) and `<id>_fx.prefab` (the layered effect), plus
`Assets/KanaWorksSakugaVFX/Resources/SakugaLibrary.asset`. To rebuild: **Tools > KanaWorks SAKUGA VFX > Build
layered effects**. Batch mode: `-executeMethod KanaWorks.SakugaVFX.EditorTools.SakugaBatch.BuildEverything`.

```csharp
using KanaWorks.SakugaVFX;

SakugaFX.Play("fire_impact_01", hitPoint);                         // one-shot, pooled
SakugaFX.Play("rock_column_01", enemy.position, Quaternion.identity, 1.3f);
SakugaFX.Cast("ice_projectile_02", hand.position, enemy, p => enemy.Damage(10));   // charge, flight, impact
SakugaFX.Cast("fire_projectile_03", hand.position, targetPoint, onHit: null, speed: 10f, arc: 0.2f);
var shield = SakugaFX.Attach("water_shield_01", player, Vector3.up, 1f);            // follows the player
shield.Stop();                                                                      // loops end on Stop()
var beam = SakugaFX.Beam("thunder_strike_03", from, to);

var fx = SakugaFX.Play("thunder_impact_01", p);
fx.Contact += e => enemy.Flinch();                                 // the moment the hit lands
fx.Finished += e => Debug.Log("done");
SakugaFX.ShakeRequested += (amp, dur, pos) => { /* Cinemachine impulse, for example */ };
SakugaFX.AutoShake = false;   // route shakes yourself;  SakugaFX.AutoHitStop = false;  if you own Time.timeScale
```

- `SakugaEffect` settings: `size` (S/M/L), `scale`, `overrideColor` + `color` (+ `recolorMain`), `speed`,
  `lifetime` (for loops), `startAt`, `lights`, `sound`, `shake`, `hitStop`, `decal`, `hitDirection`.
- Characters on the **Ignore Raycast** layer are skipped when the ground decal looks for the floor
  (`SakugaFX.GroundMask`, `SakugaFX.GroundHeight`).
- Soft particles need the URP depth texture (URP asset: Depth Texture on); without it the shader skips the fade.
- The ground decal is a flat quad on the floor (raycast down from the hit). `SakugaFX.UseDecalProjector = true` switches
  to a URP Decal Projector when the renderer has the Decal feature (experimental).
- Offline rendering: `effect.Simulate(t)` rebuilds the exact state at `t` (fixed step, fixed seed);
  `SakugaShot` describes casts and hits on a timeline and `SakugaShot.SimulateAll(shots, t)` rebuilds a whole moment.
  `SakugaBatch.RenderSequence` writes a numbered PNG sequence of a demo scene.
- Demo scenes: **Tools > KanaWorks SAKUGA VFX > Demo scenes** (Arena, Top-down shooter, Tower defense, Gallery).

## 3. Godot 4.3+

Install: `python tools/sakuga.py install godot <project>`. Then:

```gdscript
var fx := SakugaEffect3D.spawn("fire_impact_01", self, hit_point)    # frees itself when done
fx.shake_requested.connect(func(amp, dur): my_camera.shake(amp, dur))
fx.hit_stop.connect(func(s): Engine.time_scale = 0.05; await get_tree().create_timer(s, true, false, true).timeout; Engine.time_scale = 1.0)
```

`addons/kanaworks_sakuga_vfx/effects/sakuga_<category>.tscn` is one ready-made scene per category; change its
`effect_id` in the Inspector. Signals: `contact`, `shake_requested(amplitude, duration)`, `hit_stop(seconds)`,
`finished`. The plain 2D / 3D flipbook players (`SakugaFlipbook2D`, `SakugaFlipbook3D`) are still there.

## 4. Web (Canvas2D, no dependencies)

```js
import { load } from './web/sakuga-player.js';
import { SakugaFx, Shaker, loadShared } from './web/sakuga-fx.js';
const shared = await loadShared('./assets/_shared/');
const shaker = new Shaker(80);
const fx = new SakugaFx(await load('./assets/fire/fire_impact_01/fire_impact_01.flipbook.json'), shared,
                        { onshake: (a, d) => shaker.add(a, d) }).play();
// each frame: ctx.translate(...shaker.offset(dt)); fx.update(dt); fx.draw(ctx, x, y, pixelsPerMetre);
```

`fx.seek(t)` jumps to any time deterministically. Example: `web/examples/layered.html`.

## 5. Any other engine

Read the sidecar: play the grid left to right, top to bottom at `fps`; put the pivot at the origin (the root for
`upright` sheets); at `contact_frame` fire the `fx` layers (all units are metres and seconds, colours are sRGB hex,
the light `curve` is flat `[seconds after contact, multiplier, ...]` pairs). Shared white textures, decals and sounds
are in `assets/_shared/` (`shared.json` lists what is there; skip anything missing).

## 6. Adding or changing effects

- New sheet: `python tools/sakuga.py add sheet.png --id fire_slash_02 --grid 8 8`. The recipe is generated.
- Change how effects look: edit `tools/sakugakit/recipes.py` (element defaults, category scaling, styles, rare
  per-asset overrides), then `python tools/sakuga.py sync`. Never hand-edit recipe fields in `catalog.json`:
  `validate` will flag them.
- Shared textures and sounds: `python tools/sakuga.py shared <folder>` (strips metadata and tags).
- Before committing: `python tools/sakuga.py validate` and `python -m pytest -q`.

Credit line for anything you ship: `VFX: KanaWorks_AI (@KanaWorks_AI)`. Code is MIT; images and sounds are under the
KanaWorks_AI VFX Asset License (`LICENSE.md`).
