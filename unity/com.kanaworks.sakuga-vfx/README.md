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

Code: MIT (`LICENSE.md`). Flipbook images: KanaWorks_AI VFX Asset License. Credit line:
`VFX: KanaWorks_AI (@KanaWorks_AI)`.
