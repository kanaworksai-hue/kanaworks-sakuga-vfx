# KanaWorks SAKUGA VFX - Godot 4 addon

Plays the KanaWorks SAKUGA VFX flipbooks in Godot 4.3+ (2D and 3D). Code: MIT. Flipbook images: KanaWorks_AI VFX
Asset License (`LICENSE.md` in this folder). Credit line: `VFX: KanaWorks_AI (@KanaWorks_AI)`.

- `flipbooks/<element>/<id>/<id>.tres` - a SpriteFrames per effect (animation `default`, the sheet's fps and loop),
  with a `sakuga` metadata dictionary (grid, cell, pivot, blend, form). Works in a plain `AnimatedSprite2D` too.
- `SakugaFlipbook2D` (AnimatedSprite2D): sheet pivot, additive option, hide / free / hold at the end.
- `SakugaFlipbook3D` (MeshInstance3D): camera-facing quad (columns stay upright), unlit shader with alpha blend or
  additive, 256 px = 1 m.

```gdscript
const FIRE_HIT := preload("res://addons/kanaworks_sakuga_vfx/flipbooks/fire/fire_impact_01/fire_impact_01.tres")
SakugaFlipbook2D.spawn(FIRE_HIT, self, hit_position)          # 2D: plays once, frees itself
SakugaFlipbook3D.spawn(FIRE_HIT, self, hit_point_3d)          # 3D: same .tres
```

Layered effects (the flipbook plus the secondary layers of its recipe: flash, ring, sparks, debris, smoke, ground
decal, light, sound):

- `SakugaEffect3D` (Node3D): set `effect_id` (or call `SakugaEffect3D.spawn("fire_impact_01", self, hit_point)`).
  Size S / M / L, scale, colour override, speed, lifetime for loops. Signals: `contact`,
  `shake_requested(amplitude, duration)`, `hit_stop(seconds)`, `finished`.
- `effects/sakuga_<category>.tscn`: one ready-made scene per category (projectile, impact, area, column, strike,
  beam, vortex, buff, shield, cast, slash, aura); change `effect_id` in the Inspector.
- `shared/`: the white layer textures, ground decals and sound effects the recipes refer to.

Not in the Godot version yet: projectile flight (move the node yourself and spawn `recipe.pairs.impact` on arrival,
as `sakuga_demo/demo_fx.gd` does), offline `simulate(t)`, stretched spark trails, and ground detection for decals
under mid-air hits (they sit 0.9 m below the effect).

Demo scenes: `res://sakuga_demo/demo_2d.tscn`, `res://sakuga_demo/demo_3d.tscn` and `res://sakuga_demo/demo_fx.tscn`
(layered effects; Left / Right to browse, Enter to replay).
