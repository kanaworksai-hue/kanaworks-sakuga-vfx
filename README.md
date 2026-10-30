# KanaWorks SAKUGA VFX

<div align="center">

<img src="docs/assets/banner.jpg" alt="KanaWorks SAKUGA VFX: free anime-style game VFX flipbooks with a hand-drawn look" width="820">

<p>
  <a href="LICENSE.md"><img alt="Assets: KanaWorks_AI VFX Asset License" src="https://img.shields.io/badge/assets-KanaWorks__AI_VFX_License-ff6aa2?style=flat-square"></a>
  <a href="LICENSE.md#mit-license-code"><img alt="Code: MIT" src="https://img.shields.io/badge/code-MIT-6b4dff?style=flat-square"></a>
  <img alt="Unity 6 URP" src="https://img.shields.io/badge/Unity_6-URP-111?style=flat-square&logo=unity">
  <img alt="Godot 4" src="https://img.shields.io/badge/Godot-4-478cbf?style=flat-square&logo=godotengine&logoColor=white">
  <img alt="Web" src="https://img.shields.io/badge/Web-Canvas_·_PixiJS_·_Phaser_·_Three.js-19b8d6?style=flat-square">
  <a href="https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx"><img alt="Star on GitHub" src="docs/assets/star-badge.svg"></a>
</p>

<p><a href="https://github.com/kanaworksai-hue"><img src="docs/brand/kanaworks-mark.png" height="22" alt="" align="absmiddle"></a>&nbsp;<a href="https://github.com/kanaworksai-hue"><b>A KanaWorks_AI project</b></a></p>

English | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [한국어](README.ko.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md)

</div>

**A free library of anime-style (sakuga) game effects with a hand-drawn look**: fire, water, thunder, wind, poison and
more, as transparent flipbook sprite sheets that drop into Unity, Godot, the web or any engine that can play a sprite sheet.

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
| <img src="assets/dark/dark_impact_02/preview.webp" width="72" alt="dark_impact_02"> | [`dark_impact_02`](assets/dark/dark_impact_02/) | dark | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_impact_03/preview.webp" width="72" alt="dark_impact_03"> | [`dark_impact_03`](assets/dark/dark_impact_03/) | dark | impact | 64 (8x8) | yes | 2026-10-29 |
| <img src="assets/dark/dark_projectile_02/preview.webp" width="72" alt="dark_projectile_02"> | [`dark_projectile_02`](assets/dark/dark_projectile_02/) | dark | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/dark/dark_projectile_03/preview.webp" width="72" alt="dark_projectile_03"> | [`dark_projectile_03`](assets/dark/dark_projectile_03/) | dark | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/fire/fire_impact_02/preview.webp" width="72" alt="fire_impact_02"> | [`fire_impact_02`](assets/fire/fire_impact_02/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_impact_03/preview.webp" width="72" alt="fire_impact_03"> | [`fire_impact_03`](assets/fire/fire_impact_03/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_projectile_02/preview.webp" width="72" alt="fire_projectile_02"> | [`fire_projectile_02`](assets/fire/fire_projectile_02/) | fire | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/fire/fire_projectile_03/preview.webp" width="72" alt="fire_projectile_03"> | [`fire_projectile_03`](assets/fire/fire_projectile_03/) | fire | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/ice/ice_impact_02/preview.webp" width="72" alt="ice_impact_02"> | [`ice_impact_02`](assets/ice/ice_impact_02/) | ice | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_impact_03/preview.webp" width="72" alt="ice_impact_03"> | [`ice_impact_03`](assets/ice/ice_impact_03/) | ice | impact | 64 (8x8) | yes | 2026-10-29 |
| <img src="assets/ice/ice_projectile_02/preview.webp" width="72" alt="ice_projectile_02"> | [`ice_projectile_02`](assets/ice/ice_projectile_02/) | ice | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/ice/ice_projectile_03/preview.webp" width="72" alt="ice_projectile_03"> | [`ice_projectile_03`](assets/ice/ice_projectile_03/) | ice | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/light/light_column_02/preview.webp" width="72" alt="light_column_02"> | [`light_column_02`](assets/light/light_column_02/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_column_03/preview.webp" width="72" alt="light_column_03"> | [`light_column_03`](assets/light/light_column_03/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_impact_02/preview.webp" width="72" alt="light_impact_02"> | [`light_impact_02`](assets/light/light_impact_02/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_impact_03/preview.webp" width="72" alt="light_impact_03"> | [`light_impact_03`](assets/light/light_impact_03/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_02/preview.webp" width="72" alt="poison_impact_02"> | [`poison_impact_02`](assets/poison/poison_impact_02/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_03/preview.webp" width="72" alt="poison_impact_03"> | [`poison_impact_03`](assets/poison/poison_impact_03/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_projectile_02/preview.webp" width="72" alt="poison_projectile_02"> | [`poison_projectile_02`](assets/poison/poison_projectile_02/) | poison | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/poison/poison_projectile_03/preview.webp" width="72" alt="poison_projectile_03"> | [`poison_projectile_03`](assets/poison/poison_projectile_03/) | poison | projectile | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/rock/rock_column_02/preview.webp" width="72" alt="rock_column_02"> | [`rock_column_02`](assets/rock/rock_column_02/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_column_03/preview.webp" width="72" alt="rock_column_03"> | [`rock_column_03`](assets/rock/rock_column_03/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_02/preview.webp" width="72" alt="rock_impact_02"> | [`rock_impact_02`](assets/rock/rock_impact_02/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_03/preview.webp" width="72" alt="rock_impact_03"> | [`rock_impact_03`](assets/rock/rock_impact_03/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_02/preview.webp" width="72" alt="thunder_impact_02"> | [`thunder_impact_02`](assets/thunder/thunder_impact_02/) | thunder | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_03/preview.webp" width="72" alt="thunder_impact_03"> | [`thunder_impact_03`](assets/thunder/thunder_impact_03/) | thunder | impact | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_02/preview.webp" width="72" alt="thunder_strike_02"> | [`thunder_strike_02`](assets/thunder/thunder_strike_02/) | thunder | strike | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_03/preview.webp" width="72" alt="thunder_strike_03"> | [`thunder_strike_03`](assets/thunder/thunder_strike_03/) | thunder | strike | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/water/water_column_02/preview.webp" width="72" alt="water_column_02"> | [`water_column_02`](assets/water/water_column_02/) | water | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/water/water_column_03/preview.webp" width="72" alt="water_column_03"> | [`water_column_03`](assets/water/water_column_03/) | water | column | 56 (8x7) | yes | 2026-10-29 |
| <img src="assets/water/water_impact_02/preview.webp" width="72" alt="water_impact_02"> | [`water_impact_02`](assets/water/water_impact_02/) | water | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_impact_03/preview.webp" width="72" alt="water_impact_03"> | [`water_impact_03`](assets/water/water_impact_03/) | water | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_02/preview.webp" width="72" alt="wind_impact_02"> | [`wind_impact_02`](assets/wind/wind_impact_02/) | wind | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_03/preview.webp" width="72" alt="wind_impact_03"> | [`wind_impact_03`](assets/wind/wind_impact_03/) | wind | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_02/preview.webp" width="72" alt="wind_vortex_02"> | [`wind_vortex_02`](assets/wind/wind_vortex_02/) | wind | vortex | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_03/preview.webp" width="72" alt="wind_vortex_03"> | [`wind_vortex_03`](assets/wind/wind_vortex_03/) | wind | vortex | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_aura_01/preview.webp" width="72" alt="dark_aura_01"> | [`dark_aura_01`](assets/dark/dark_aura_01/) | dark | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/dark/dark_charge_01/preview.webp" width="72" alt="dark_charge_01"> | [`dark_charge_01`](assets/dark/dark_charge_01/) | dark | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_shield_01/preview.webp" width="72" alt="dark_shield_01"> | [`dark_shield_01`](assets/dark/dark_shield_01/) | dark | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/dark/dark_slash_01/preview.webp" width="72" alt="dark_slash_01"> | [`dark_slash_01`](assets/dark/dark_slash_01/) | dark | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_aura_01/preview.webp" width="72" alt="fire_aura_01"> | [`fire_aura_01`](assets/fire/fire_aura_01/) | fire | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/fire/fire_charge_01/preview.webp" width="72" alt="fire_charge_01"> | [`fire_charge_01`](assets/fire/fire_charge_01/) | fire | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_shield_01/preview.webp" width="72" alt="fire_shield_01"> | [`fire_shield_01`](assets/fire/fire_shield_01/) | fire | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/fire/fire_slash_01/preview.webp" width="72" alt="fire_slash_01"> | [`fire_slash_01`](assets/fire/fire_slash_01/) | fire | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_aura_01/preview.webp" width="72" alt="ice_aura_01"> | [`ice_aura_01`](assets/ice/ice_aura_01/) | ice | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/ice/ice_charge_01/preview.webp" width="72" alt="ice_charge_01"> | [`ice_charge_01`](assets/ice/ice_charge_01/) | ice | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_shield_01/preview.webp" width="72" alt="ice_shield_01"> | [`ice_shield_01`](assets/ice/ice_shield_01/) | ice | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/ice/ice_slash_01/preview.webp" width="72" alt="ice_slash_01"> | [`ice_slash_01`](assets/ice/ice_slash_01/) | ice | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_aura_01/preview.webp" width="72" alt="light_aura_01"> | [`light_aura_01`](assets/light/light_aura_01/) | light | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/light/light_charge_01/preview.webp" width="72" alt="light_charge_01"> | [`light_charge_01`](assets/light/light_charge_01/) | light | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_shield_01/preview.webp" width="72" alt="light_shield_01"> | [`light_shield_01`](assets/light/light_shield_01/) | light | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/light/light_slash_01/preview.webp" width="72" alt="light_slash_01"> | [`light_slash_01`](assets/light/light_slash_01/) | light | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_aura_01/preview.webp" width="72" alt="poison_aura_01"> | [`poison_aura_01`](assets/poison/poison_aura_01/) | poison | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/poison/poison_charge_01/preview.webp" width="72" alt="poison_charge_01"> | [`poison_charge_01`](assets/poison/poison_charge_01/) | poison | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_shield_01/preview.webp" width="72" alt="poison_shield_01"> | [`poison_shield_01`](assets/poison/poison_shield_01/) | poison | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/poison/poison_slash_01/preview.webp" width="72" alt="poison_slash_01"> | [`poison_slash_01`](assets/poison/poison_slash_01/) | poison | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_aura_01/preview.webp" width="72" alt="rock_aura_01"> | [`rock_aura_01`](assets/rock/rock_aura_01/) | rock | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/rock/rock_charge_01/preview.webp" width="72" alt="rock_charge_01"> | [`rock_charge_01`](assets/rock/rock_charge_01/) | rock | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_shield_01/preview.webp" width="72" alt="rock_shield_01"> | [`rock_shield_01`](assets/rock/rock_shield_01/) | rock | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/rock/rock_slash_01/preview.webp" width="72" alt="rock_slash_01"> | [`rock_slash_01`](assets/rock/rock_slash_01/) | rock | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_aura_01/preview.webp" width="72" alt="thunder_aura_01"> | [`thunder_aura_01`](assets/thunder/thunder_aura_01/) | thunder | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/thunder/thunder_charge_01/preview.webp" width="72" alt="thunder_charge_01"> | [`thunder_charge_01`](assets/thunder/thunder_charge_01/) | thunder | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_shield_01/preview.webp" width="72" alt="thunder_shield_01"> | [`thunder_shield_01`](assets/thunder/thunder_shield_01/) | thunder | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/thunder/thunder_slash_01/preview.webp" width="72" alt="thunder_slash_01"> | [`thunder_slash_01`](assets/thunder/thunder_slash_01/) | thunder | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_aura_01/preview.webp" width="72" alt="water_aura_01"> | [`water_aura_01`](assets/water/water_aura_01/) | water | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/water/water_charge_01/preview.webp" width="72" alt="water_charge_01"> | [`water_charge_01`](assets/water/water_charge_01/) | water | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_shield_01/preview.webp" width="72" alt="water_shield_01"> | [`water_shield_01`](assets/water/water_shield_01/) | water | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/water/water_slash_01/preview.webp" width="72" alt="water_slash_01"> | [`water_slash_01`](assets/water/water_slash_01/) | water | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_aura_01/preview.webp" width="72" alt="wind_aura_01"> | [`wind_aura_01`](assets/wind/wind_aura_01/) | wind | aura | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/wind/wind_charge_01/preview.webp" width="72" alt="wind_charge_01"> | [`wind_charge_01`](assets/wind/wind_charge_01/) | wind | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_shield_01/preview.webp" width="72" alt="wind_shield_01"> | [`wind_shield_01`](assets/wind/wind_shield_01/) | wind | shield | 40 (8x5) | yes | 2026-10-29 |
| <img src="assets/wind/wind_slash_01/preview.webp" width="72" alt="wind_slash_01"> | [`wind_slash_01`](assets/wind/wind_slash_01/) | wind | slash | 64 (8x8) | no | 2026-10-29 |
<!-- assets:end -->

Forms: `projectile` loops while it flies; `impact` plays once at the hit; `column` stands on the ground and rises;
`vortex` swirls in place; `strike` comes down from above onto the ground; `shield` loops around a character; `aura`
loops at a character's feet; `charge` is the cast (it pairs with every projectile of its element); `slash` is a
melee hit. Names are `element_form_NN`: `_01` is the classic look, `_02` and `_03` are style variants (for example
`fire_projectile_02` soulfire, `fire_projectile_03` meteor). The recipe gives each one its category (a variant can be
an `area`, `beam` or `buff`).

## Layered effects (recipes)

Every effect also has a **recipe** (in `catalog.json` and in its sidecar) that turns the drawing into a ready-to-use
skill effect: a category and use cases, the effects it pairs with (charge, projectile, impact), where it sits
(centre or ground; billboard, upright, flat or rolled along its flight), its size in metres, the contact frame, and
the secondary layers: contact flash, shockwave ring, sparks, debris, cel smoke, ground decal, a light with an
intensity curve, camera shake, hit stop and sound. The Unity package builds an `<id>_fx.prefab` from it, the Godot
addon a `SakugaEffect3D`, and `web/sakuga-fx.js` plays it on a canvas. Shared white textures, decals and sounds are in
[`assets/_shared/`](assets/_shared/).

```sh
python tools/sakuga.py suggest "ice arrow hitting a slime"
#  17.5  ice_projectile_02  projectile glacier  ice lance, ice arrow, ice shard
#  ...
#  chain: ice_charge_01 -> ice_projectile_02 -> ice_impact_01
```

```csharp
SakugaFX.Cast("ice_projectile_02", hand.position, slime, p => slime.Damage(10));   // Unity: charge, flight, impact
```

Coding agents: start with [AGENTS.md](AGENTS.md) (and [llms.txt](llms.txt)).

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
