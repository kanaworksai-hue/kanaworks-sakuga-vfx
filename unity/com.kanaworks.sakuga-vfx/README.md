# KanaWorks SAKUGA VFX - Unity package (URP)

Plays the KanaWorks SAKUGA VFX flipbooks in Unity 6 (URP 17+).

1. Unzip the Unity download into your project folder: it adds `Packages/com.kanaworks.sakuga-vfx/` (this package)
   and `Assets/KanaWorksSakugaVFX/Flipbooks/` (the sheets with their `.flipbook.json` sidecars).
2. Unity imports the sheets with the right settings and builds `M_<id>.mat`, `M_<id>_Particles.mat`,
   `<id>_Quad.asset` and `<id>.prefab` next to each sidecar.
   To rebuild: **Tools > KanaWorks SAKUGA VFX > Build materials and prefabs**.
3. **Tools > KanaWorks SAKUGA VFX > Build demo scene**, then Play.

In code:

```csharp
public SakugaFlipbook hitPrefab;                       // drag fire_impact_01.prefab here
SakugaFlipbook.Spawn(hitPrefab, hitPoint, Quaternion.identity);   // plays once, destroys itself
```

- `SakugaFlipbook`: fps, loop, start delay, speed, unscaled time, random start frame (loops), end action
  (hide / hold last frame / disable / destroy) and a `Finished` event.
- Shader `KanaWorks/SakugaFlipbook`: unlit, straight-alpha sheet; Blend = Alpha Blend or Additive; Billboard =
  None / Face Camera / Upright (columns stay vertical, pivot on the ground); HDR tint for Bloom.
- Particle System: use `M_<id>_Particles.mat` (Frame -1, no billboard) and set Texture Sheet Animation to the
  sheet's grid (for example 8 x 8) with the frame count from the sidecar.
- Scale: 256 px = 1 unit. Scale the prefab to taste.

## Layered effects

Next to every `<id>.prefab` the importer builds `<id>_fx.prefab`: a `SakugaEffect` that plays the flipbook plus
the secondary layers of its recipe (contact flash and glow, shockwave ring, stretched sparks, debris and cel smoke
Particle Systems built in code, a ground decal, a point light with an intensity curve, sound) and raises shake and
hit-stop requests at the contact frame. All ids go into `Assets/KanaWorksSakugaVFX/Resources/SakugaLibrary.asset`.

```csharp
SakugaFX.Play("fire_impact_01", hitPoint);                                   // pooled one-shot
SakugaFX.Cast("ice_projectile_02", hand.position, enemy, p => enemy.Damage(10)); // charge, flight, impact
SakugaFX.Attach("water_shield_01", player, Vector3.up).lifetime = 3f;        // follows the player
var fx = SakugaFX.Play("rock_column_01", enemy.position);
fx.Contact += e => enemy.Flinch();
fx.Simulate(0.5f);                                                           // offline: exact state at 0.5 s
```

- `SakugaEffect`: size S / M / L, scale, colour override (re-ramps the drawing too), speed, lifetime for loops,
  start time, lights / sound / shake / hit stop / decal switches, hit direction, `Finished`, `Contact` and
  `ShakeRequested` events, deterministic `Simulate(t)` / `BeginManual()` + `StepManual(dt)`.
- `SakugaProjectile`: speed, arc, homing, rolls "travel" sheets along the flight, trail by distance, spawns the
  paired impact on arrival (also while simulating offline).
- `SakugaCameraShake`: offsets the camera only while it renders, so Cinemachine or your own controller never sees
  it. `SakugaFX.AutoShake = false` and `SakugaFX.ShakeRequested` to route shakes elsewhere.
- `SakugaHitStop`: real-time hit stop with a 0.15 s per second budget; `SakugaFX.AutoHitStop = false` if your game
  owns `Time.timeScale`.
- Shader: soft particles (depth fade where effects meet the floor or a body; needs the URP depth texture), HDR core
  boost (only the brightest part blooms), camera push for hits, recolour, Travel billboard mode.
- `SakugaShot` + `SakugaShot.SimulateAll`: timelines of casts and hits that play live or rebuild any moment offline.
- Demo scenes (**Tools > KanaWorks SAKUGA VFX > Demo scenes**): Arena (action RPG), Top-down shooter, Tower defense,
  Gallery (every effect; arrow keys). Batch: `KanaWorks.SakugaVFX.EditorTools.SakugaBatch.BuildEverything`,
  `RenderDemoStills`, `RenderSequence`.

Code: MIT (`LICENSE.md`). Flipbook images: KanaWorks_AI VFX Asset License. Credit line:
`VFX: KanaWorks_AI (@KanaWorks_AI)`.
