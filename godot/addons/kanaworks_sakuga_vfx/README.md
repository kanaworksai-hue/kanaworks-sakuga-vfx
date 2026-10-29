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

Demo scenes: `res://sakuga_demo/demo_2d.tscn` and `res://sakuga_demo/demo_3d.tscn`.
