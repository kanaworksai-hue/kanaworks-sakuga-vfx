# KanaWorks SAKUGA VFX

<div align="center">

<!-- TODO(banner): docs/assets/banner.jpg 由另一个会话制作，保持此路径。 -->
<img src="docs/assets/banner.jpg" alt="KanaWorks SAKUGA VFX：免费的手绘风动画游戏特效序列帧" width="820">

<p>
  <a href="LICENSE.md"><img alt="Assets: KanaWorks_AI VFX Asset License" src="https://img.shields.io/badge/assets-KanaWorks__AI_VFX_License-ff6aa2?style=flat-square"></a>
  <a href="LICENSE.md#mit-license-code"><img alt="Code: MIT" src="https://img.shields.io/badge/code-MIT-6b4dff?style=flat-square"></a>
  <img alt="Unity 6 URP" src="https://img.shields.io/badge/Unity_6-URP-111?style=flat-square&logo=unity">
  <img alt="Godot 4" src="https://img.shields.io/badge/Godot-4-478cbf?style=flat-square&logo=godotengine&logoColor=white">
  <img alt="Web" src="https://img.shields.io/badge/Web-Canvas_·_PixiJS_·_Phaser_·_Three.js-19b8d6?style=flat-square">
  <a href="https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/stargazers"><img alt="GitHub stars" src="https://img.shields.io/github/stars/kanaworksai-hue/kanaworks-sakuga-vfx?style=flat-square&logo=github&label=stars&color=f5c518"></a>
</p>

<p><a href="https://github.com/kanaworksai-hue"><img src="docs/brand/kanaworks-mark.png" height="22" alt="" align="absmiddle"></a>&nbsp;<a href="https://github.com/kanaworksai-hue"><b>A KanaWorks_AI project</b></a></p>

[English](README.md) | 简体中文 | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md)

</div>

**一个免费的手绘风动画（作画系）游戏特效库。** 火、水、雷、风、毒等特效，以透明背景的序列帧精灵图（flipbook）形式提供，
可以直接用于 Unity、Godot、网页，以及任何能播放精灵图的引擎。

它是为想要好看的技能特效、却不会用 AI 工具制作、也没有预算请特效师的人准备的：学生、独立开发者、小团队、Game Jam、视频创作者。
下载、署名、直接用。

- **拿来就能播**：每个特效是一张直通 Alpha（straight alpha）的 PNG 图集（2048 px，每帧四周留有透明边，帧与帧不会串色），
  再加一个小小的 `.flipbook.json`，写明网格、帧数、fps、是否循环和锚点。
- **各引擎都能用**：Unity 6 URP 包（着色器、播放组件、为每个特效生成预制体的导入器、演示场景）、Godot 4 插件（每个特效一个
  SpriteFrames、2D/3D 播放器、演示场景）、约 4 KB 的网页播放器以及 PixiJS、Phaser 3、Three.js 示例，其他引擎可用
  TexturePacker 格式的 JSON。
- **多种版本**：2048 和 1024 两种尺寸，每种都有黑底加算混合版。
- **免费，只需署名**：年总收入低于 100 万美元的个人和公司可免费使用，包括商用。见[许可](#许可)。

<div align="center">
<img src="docs/assets/overview.webp" alt="全部特效播放预览" width="820">
</div>

**可在线预览和下载的图库：** https://kanaworksai-hue.github.io/kanaworks-sakuga-vfx/
· **下载：** [最新版本](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)

## 特效列表

<!-- assets:start -->
| 预览 | ID | 元素 | 形态 | 帧数 | 循环 | 加入日期 |
|:---:|---|---|---|---:|:---:|---|
| <img src="assets/water/water_column_01/preview.webp" width="72" alt="water_column_01"> | [`water_column_01`](assets/water/water_column_01/) | water | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/water/water_impact_01/preview.webp" width="72" alt="water_impact_01"> | [`water_impact_01`](assets/water/water_impact_01/) | water | impact | 45 (8x8) | 否 | 2026-10-29 |
| <img src="assets/fire/fire_projectile_01/preview.webp" width="72" alt="fire_projectile_01"> | [`fire_projectile_01`](assets/fire/fire_projectile_01/) | fire | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/fire/fire_impact_01/preview.webp" width="72" alt="fire_impact_01"> | [`fire_impact_01`](assets/fire/fire_impact_01/) | fire | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_01/preview.webp" width="72" alt="thunder_impact_01"> | [`thunder_impact_01`](assets/thunder/thunder_impact_01/) | thunder | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_vortex_01/preview.webp" width="72" alt="wind_vortex_01"> | [`wind_vortex_01`](assets/wind/wind_vortex_01/) | wind | vortex | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_impact_01/preview.webp" width="72" alt="poison_impact_01"> | [`poison_impact_01`](assets/poison/poison_impact_01/) | poison | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_impact_01/preview.webp" width="72" alt="dark_impact_01"> | [`dark_impact_01`](assets/dark/dark_impact_01/) | dark | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_projectile_01/preview.webp" width="72" alt="dark_projectile_01"> | [`dark_projectile_01`](assets/dark/dark_projectile_01/) | dark | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_impact_01/preview.webp" width="72" alt="ice_impact_01"> | [`ice_impact_01`](assets/ice/ice_impact_01/) | ice | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/ice/ice_projectile_01/preview.webp" width="72" alt="ice_projectile_01"> | [`ice_projectile_01`](assets/ice/ice_projectile_01/) | ice | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/light/light_column_01/preview.webp" width="72" alt="light_column_01"> | [`light_column_01`](assets/light/light_column_01/) | light | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/light/light_impact_01/preview.webp" width="72" alt="light_impact_01"> | [`light_impact_01`](assets/light/light_impact_01/) | light | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_projectile_01/preview.webp" width="72" alt="poison_projectile_01"> | [`poison_projectile_01`](assets/poison/poison_projectile_01/) | poison | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/rock/rock_column_01/preview.webp" width="72" alt="rock_column_01"> | [`rock_column_01`](assets/rock/rock_column_01/) | rock | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_impact_01/preview.webp" width="72" alt="rock_impact_01"> | [`rock_impact_01`](assets/rock/rock_impact_01/) | rock | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_01/preview.webp" width="72" alt="thunder_strike_01"> | [`thunder_strike_01`](assets/thunder/thunder_strike_01/) | thunder | strike | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_impact_01/preview.webp" width="72" alt="wind_impact_01"> | [`wind_impact_01`](assets/wind/wind_impact_01/) | wind | impact | 41 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_impact_02/preview.webp" width="72" alt="dark_impact_02"> | [`dark_impact_02`](assets/dark/dark_impact_02/) | dark | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_impact_03/preview.webp" width="72" alt="dark_impact_03"> | [`dark_impact_03`](assets/dark/dark_impact_03/) | dark | impact | 64 (8x8) | 是 | 2026-10-29 |
| <img src="assets/dark/dark_projectile_02/preview.webp" width="72" alt="dark_projectile_02"> | [`dark_projectile_02`](assets/dark/dark_projectile_02/) | dark | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/dark/dark_projectile_03/preview.webp" width="72" alt="dark_projectile_03"> | [`dark_projectile_03`](assets/dark/dark_projectile_03/) | dark | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/fire/fire_impact_02/preview.webp" width="72" alt="fire_impact_02"> | [`fire_impact_02`](assets/fire/fire_impact_02/) | fire | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/fire/fire_impact_03/preview.webp" width="72" alt="fire_impact_03"> | [`fire_impact_03`](assets/fire/fire_impact_03/) | fire | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/fire/fire_projectile_02/preview.webp" width="72" alt="fire_projectile_02"> | [`fire_projectile_02`](assets/fire/fire_projectile_02/) | fire | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/fire/fire_projectile_03/preview.webp" width="72" alt="fire_projectile_03"> | [`fire_projectile_03`](assets/fire/fire_projectile_03/) | fire | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_impact_02/preview.webp" width="72" alt="ice_impact_02"> | [`ice_impact_02`](assets/ice/ice_impact_02/) | ice | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/ice/ice_impact_03/preview.webp" width="72" alt="ice_impact_03"> | [`ice_impact_03`](assets/ice/ice_impact_03/) | ice | impact | 64 (8x8) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_projectile_02/preview.webp" width="72" alt="ice_projectile_02"> | [`ice_projectile_02`](assets/ice/ice_projectile_02/) | ice | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_projectile_03/preview.webp" width="72" alt="ice_projectile_03"> | [`ice_projectile_03`](assets/ice/ice_projectile_03/) | ice | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/light/light_column_02/preview.webp" width="72" alt="light_column_02"> | [`light_column_02`](assets/light/light_column_02/) | light | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/light/light_column_03/preview.webp" width="72" alt="light_column_03"> | [`light_column_03`](assets/light/light_column_03/) | light | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/light/light_impact_02/preview.webp" width="72" alt="light_impact_02"> | [`light_impact_02`](assets/light/light_impact_02/) | light | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/light/light_impact_03/preview.webp" width="72" alt="light_impact_03"> | [`light_impact_03`](assets/light/light_impact_03/) | light | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_impact_02/preview.webp" width="72" alt="poison_impact_02"> | [`poison_impact_02`](assets/poison/poison_impact_02/) | poison | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_impact_03/preview.webp" width="72" alt="poison_impact_03"> | [`poison_impact_03`](assets/poison/poison_impact_03/) | poison | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_projectile_02/preview.webp" width="72" alt="poison_projectile_02"> | [`poison_projectile_02`](assets/poison/poison_projectile_02/) | poison | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/poison/poison_projectile_03/preview.webp" width="72" alt="poison_projectile_03"> | [`poison_projectile_03`](assets/poison/poison_projectile_03/) | poison | projectile | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/rock/rock_column_02/preview.webp" width="72" alt="rock_column_02"> | [`rock_column_02`](assets/rock/rock_column_02/) | rock | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_column_03/preview.webp" width="72" alt="rock_column_03"> | [`rock_column_03`](assets/rock/rock_column_03/) | rock | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_impact_02/preview.webp" width="72" alt="rock_impact_02"> | [`rock_impact_02`](assets/rock/rock_impact_02/) | rock | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_impact_03/preview.webp" width="72" alt="rock_impact_03"> | [`rock_impact_03`](assets/rock/rock_impact_03/) | rock | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_02/preview.webp" width="72" alt="thunder_impact_02"> | [`thunder_impact_02`](assets/thunder/thunder_impact_02/) | thunder | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_03/preview.webp" width="72" alt="thunder_impact_03"> | [`thunder_impact_03`](assets/thunder/thunder_impact_03/) | thunder | impact | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_02/preview.webp" width="72" alt="thunder_strike_02"> | [`thunder_strike_02`](assets/thunder/thunder_strike_02/) | thunder | strike | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_03/preview.webp" width="72" alt="thunder_strike_03"> | [`thunder_strike_03`](assets/thunder/thunder_strike_03/) | thunder | strike | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/water/water_column_02/preview.webp" width="72" alt="water_column_02"> | [`water_column_02`](assets/water/water_column_02/) | water | column | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/water/water_column_03/preview.webp" width="72" alt="water_column_03"> | [`water_column_03`](assets/water/water_column_03/) | water | column | 56 (8x7) | 是 | 2026-10-29 |
| <img src="assets/water/water_impact_02/preview.webp" width="72" alt="water_impact_02"> | [`water_impact_02`](assets/water/water_impact_02/) | water | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/water/water_impact_03/preview.webp" width="72" alt="water_impact_03"> | [`water_impact_03`](assets/water/water_impact_03/) | water | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_impact_02/preview.webp" width="72" alt="wind_impact_02"> | [`wind_impact_02`](assets/wind/wind_impact_02/) | wind | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_impact_03/preview.webp" width="72" alt="wind_impact_03"> | [`wind_impact_03`](assets/wind/wind_impact_03/) | wind | impact | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_vortex_02/preview.webp" width="72" alt="wind_vortex_02"> | [`wind_vortex_02`](assets/wind/wind_vortex_02/) | wind | vortex | 56 (8x7) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_vortex_03/preview.webp" width="72" alt="wind_vortex_03"> | [`wind_vortex_03`](assets/wind/wind_vortex_03/) | wind | vortex | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_aura_01/preview.webp" width="72" alt="dark_aura_01"> | [`dark_aura_01`](assets/dark/dark_aura_01/) | dark | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/dark/dark_charge_01/preview.webp" width="72" alt="dark_charge_01"> | [`dark_charge_01`](assets/dark/dark_charge_01/) | dark | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/dark/dark_shield_01/preview.webp" width="72" alt="dark_shield_01"> | [`dark_shield_01`](assets/dark/dark_shield_01/) | dark | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/dark/dark_slash_01/preview.webp" width="72" alt="dark_slash_01"> | [`dark_slash_01`](assets/dark/dark_slash_01/) | dark | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/fire/fire_aura_01/preview.webp" width="72" alt="fire_aura_01"> | [`fire_aura_01`](assets/fire/fire_aura_01/) | fire | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/fire/fire_charge_01/preview.webp" width="72" alt="fire_charge_01"> | [`fire_charge_01`](assets/fire/fire_charge_01/) | fire | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/fire/fire_shield_01/preview.webp" width="72" alt="fire_shield_01"> | [`fire_shield_01`](assets/fire/fire_shield_01/) | fire | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/fire/fire_slash_01/preview.webp" width="72" alt="fire_slash_01"> | [`fire_slash_01`](assets/fire/fire_slash_01/) | fire | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/ice/ice_aura_01/preview.webp" width="72" alt="ice_aura_01"> | [`ice_aura_01`](assets/ice/ice_aura_01/) | ice | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_charge_01/preview.webp" width="72" alt="ice_charge_01"> | [`ice_charge_01`](assets/ice/ice_charge_01/) | ice | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/ice/ice_shield_01/preview.webp" width="72" alt="ice_shield_01"> | [`ice_shield_01`](assets/ice/ice_shield_01/) | ice | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/ice/ice_slash_01/preview.webp" width="72" alt="ice_slash_01"> | [`ice_slash_01`](assets/ice/ice_slash_01/) | ice | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/light/light_aura_01/preview.webp" width="72" alt="light_aura_01"> | [`light_aura_01`](assets/light/light_aura_01/) | light | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/light/light_charge_01/preview.webp" width="72" alt="light_charge_01"> | [`light_charge_01`](assets/light/light_charge_01/) | light | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/light/light_shield_01/preview.webp" width="72" alt="light_shield_01"> | [`light_shield_01`](assets/light/light_shield_01/) | light | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/light/light_slash_01/preview.webp" width="72" alt="light_slash_01"> | [`light_slash_01`](assets/light/light_slash_01/) | light | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_aura_01/preview.webp" width="72" alt="poison_aura_01"> | [`poison_aura_01`](assets/poison/poison_aura_01/) | poison | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/poison/poison_charge_01/preview.webp" width="72" alt="poison_charge_01"> | [`poison_charge_01`](assets/poison/poison_charge_01/) | poison | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/poison/poison_shield_01/preview.webp" width="72" alt="poison_shield_01"> | [`poison_shield_01`](assets/poison/poison_shield_01/) | poison | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/poison/poison_slash_01/preview.webp" width="72" alt="poison_slash_01"> | [`poison_slash_01`](assets/poison/poison_slash_01/) | poison | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_aura_01/preview.webp" width="72" alt="rock_aura_01"> | [`rock_aura_01`](assets/rock/rock_aura_01/) | rock | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/rock/rock_charge_01/preview.webp" width="72" alt="rock_charge_01"> | [`rock_charge_01`](assets/rock/rock_charge_01/) | rock | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/rock/rock_shield_01/preview.webp" width="72" alt="rock_shield_01"> | [`rock_shield_01`](assets/rock/rock_shield_01/) | rock | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/rock/rock_slash_01/preview.webp" width="72" alt="rock_slash_01"> | [`rock_slash_01`](assets/rock/rock_slash_01/) | rock | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_aura_01/preview.webp" width="72" alt="thunder_aura_01"> | [`thunder_aura_01`](assets/thunder/thunder_aura_01/) | thunder | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/thunder/thunder_charge_01/preview.webp" width="72" alt="thunder_charge_01"> | [`thunder_charge_01`](assets/thunder/thunder_charge_01/) | thunder | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/thunder/thunder_shield_01/preview.webp" width="72" alt="thunder_shield_01"> | [`thunder_shield_01`](assets/thunder/thunder_shield_01/) | thunder | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/thunder/thunder_slash_01/preview.webp" width="72" alt="thunder_slash_01"> | [`thunder_slash_01`](assets/thunder/thunder_slash_01/) | thunder | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/water/water_aura_01/preview.webp" width="72" alt="water_aura_01"> | [`water_aura_01`](assets/water/water_aura_01/) | water | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/water/water_charge_01/preview.webp" width="72" alt="water_charge_01"> | [`water_charge_01`](assets/water/water_charge_01/) | water | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/water/water_shield_01/preview.webp" width="72" alt="water_shield_01"> | [`water_shield_01`](assets/water/water_shield_01/) | water | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/water/water_slash_01/preview.webp" width="72" alt="water_slash_01"> | [`water_slash_01`](assets/water/water_slash_01/) | water | slash | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_aura_01/preview.webp" width="72" alt="wind_aura_01"> | [`wind_aura_01`](assets/wind/wind_aura_01/) | wind | aura | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/wind/wind_charge_01/preview.webp" width="72" alt="wind_charge_01"> | [`wind_charge_01`](assets/wind/wind_charge_01/) | wind | charge | 64 (8x8) | 否 | 2026-10-29 |
| <img src="assets/wind/wind_shield_01/preview.webp" width="72" alt="wind_shield_01"> | [`wind_shield_01`](assets/wind/wind_shield_01/) | wind | shield | 40 (8x5) | 是 | 2026-10-29 |
| <img src="assets/wind/wind_slash_01/preview.webp" width="72" alt="wind_slash_01"> | [`wind_slash_01`](assets/wind/wind_slash_01/) | wind | slash | 64 (8x8) | 否 | 2026-10-29 |
<!-- assets:end -->

形态（form）：`projectile` 飞行时循环播放；`impact` 命中时播放一次；`column` 从地面升起的柱子；`vortex` 原地旋转的旋涡；
`strike` 从上方落到地面；`shield` 围绕角色循环播放；`aura` 在角色脚下循环播放；`charge` 是蓄力施法（可与同元素的所有
projectile 搭配）；`slash` 是近战命中。命名规则为 `元素_形态_编号`（`element_form_NN`）：`_01` 是经典样式，`_02` 和 `_03`
是风格变体（例如 `fire_projectile_02` 是魂火，`fire_projectile_03` 是陨石）。每个特效的类别由配方决定（变体也可能是
`area`、`beam` 或 `buff`）。

## 分层特效（配方）

每个特效还有一份**配方**（recipe，写在 `catalog.json` 和它的 sidecar 里），把这张图变成拿来就能用的技能特效：类别和用途、
搭配的特效（蓄力、飞行物、命中）、放置方式（中心或地面；广告牌、直立、平铺，或沿飞行方向旋转）、以米为单位的大小、命中帧，
以及二级图层：命中闪光、冲击波环、火花、碎片、赛璐璐风烟雾、地面贴花、带强度曲线的灯光、镜头震动、顿帧（hit stop）和音效。
Unity 包据此生成 `<id>_fx.prefab`，Godot 插件生成 `SakugaEffect3D`，`web/sakuga-fx.js` 则在 canvas 上播放。共用的白色纹理、
贴花和音效在 [`assets/_shared/`](assets/_shared/) 中。

```sh
python tools/sakuga.py suggest "ice arrow hitting a slime"
#  17.5  ice_projectile_02  projectile glacier  ice lance, ice arrow, ice shard
#  ...
#  chain: ice_charge_01 -> ice_projectile_02 -> ice_impact_01
```

```csharp
SakugaFX.Cast("ice_projectile_02", hand.position, slime, p => slime.Damage(10));   // Unity: charge, flight, impact
```

编程代理（coding agent）请先看 [AGENTS.md](AGENTS.md)（以及 [llms.txt](llms.txt)）。

## 下载

每个[发布版本](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)包含：

| 文件 | 用途 |
|---|---|
| `kanaworks-sakuga-vfx-<version>-unity.zip` | Unity 6（URP）：解压到项目文件夹 |
| `kanaworks-sakuga-vfx-<version>-godot.zip` | Godot 4.3+：解压到项目文件夹 |
| `kanaworks-sakuga-vfx-<version>-web.zip` | 网页播放器、示例和图库，含全部精灵图 |
| `kanaworks-sakuga-vfx-<version>-flipbooks.zip` | 全部精灵图的全部版本，适用于任何引擎 |
| `assets/<id>.zip`（图库） | 单个特效，全部版本 |

也可以直接从本仓库取 `assets/<element>/<id>/<id>.png` 和对应的 `.flipbook.json`。

## 快速上手

### Unity 6（URP）

1. 把 `...-unity.zip` 解压到项目文件夹，会添加 `Packages/com.kanaworks.sakuga-vfx/` 和 `Assets/KanaWorksSakugaVFX/Flipbooks/`。
2. Unity 会用正确的设置导入精灵图，并在每张图旁边生成 `<id>.prefab`。
   执行 **Tools > KanaWorks SAKUGA VFX > Build demo scene**，然后 Play。
3. 代码中：

```csharp
public SakugaFlipbook fireHit;                                    // 把 fire_impact_01.prefab 拖进来
SakugaFlipbook.Spawn(fireHit, hitPoint, Quaternion.identity);     // 播放一次后自动销毁
```

着色器 `KanaWorks/SakugaFlipbook`：Alpha 混合或加算、朝向相机或直立广告牌（柱子保持竖直、根部贴地）、可用于 Bloom 的 HDR
颜色。粒子系统请使用 `M_<id>_Particles.mat`，并把 Texture Sheet Animation 设为精灵图的网格。

### Godot 4.3+

1. 把 `...-godot.zip` 解压到项目文件夹（添加 `addons/kanaworks_sakuga_vfx/` 和 `sakuga_demo/`）。
2. 打开 `res://sakuga_demo/demo_2d.tscn` 或 `demo_3d.tscn`，按 F6。
3. 代码中：

```gdscript
const FIRE_HIT := preload("res://addons/kanaworks_sakuga_vfx/flipbooks/fire/fire_impact_01/fire_impact_01.tres")
SakugaFlipbook2D.spawn(FIRE_HIT, self, hit_position)     # 2D（AnimatedSprite2D），播完自动释放
SakugaFlipbook3D.spawn(FIRE_HIT, self, hit_point)        # 3D 广告牌，同一个 .tres
```

每个 `.tres` 都是普通的 `SpriteFrames`，任何 `AnimatedSprite2D` 都能用。

### 网页

```html
<canvas id="fx" width="512" height="512"></canvas>
<script type="module">
  import { load, mount } from './web/sakuga-player.js';
  mount(document.getElementById('fx'), await load('./assets/fire/fire_impact_01/fire_impact_01.flipbook.json'));
</script>
```

`sakuga-player.js` 无依赖（Canvas2D，约 4 KB）：`load()`、`Flipbook`（`update(dt)`、`draw(ctx, x, y)`）、`mount()`，以及给
PixiJS、Phaser 用的 `spritesheetData()`。示例见 [`web/examples/`](web/examples/)（Canvas2D、PixiJS 8、Phaser 3、Three.js；
库从固定版本的 CDN 加载）。用 HTTP 提供该文件夹（例如 `python -m http.server`）后打开 `web/`。

### 其他引擎

读取 sidecar 文件，按从左到右、从上到下的顺序播放网格：

| 字段 | 含义 |
|---|---|
| `grid` | `[列, 行]` |
| `cell` | 单帧的 `[宽, 高]`（像素） |
| `frames` | 要播放的帧数（前 `frames` 格） |
| `fps`, `loop` | 播放速度（30）以及是否循环（projectile） |
| `blend` | `alpha`（直通 Alpha）或 `additive` |
| `pivot` | 格内 `[x, y]`，以左上为原点的 0–1：中心，或柱子的根部 |
| `bottom_inset` | 柱子根部以下的空白，占格高的比例 |
| `source` | 同目录下的 PNG |

发布版本中还有 TexturePacker「JSON Hash」格式（带 `animations` 列表）的 `<id>.atlas.json`。

## 署名

使用或转发这些特效时，请在能看到的地方注明（游戏制作人员名单、视频简介、网页页脚等）：

```
VFX: KanaWorks_AI (@KanaWorks_AI)
```

转发、转载特效或其预览时，请保留署名和 https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx 的链接。

## 许可

这是一个**免费的特效库**，不是开源项目：特效可在一定条件下免费使用。
摘要（以[完整许可](LICENSE.md)为准，且以日文版为准）：

- 年总收入（上一会计年度，含关联公司）**低于 100 万美元**的个人和公司，个人、非商业、商业用途均**免费**。
  达到或超过该金额，请联系 [KanaWorks_AI](https://x.com/KanaWorks_AI) 另行取得授权。
- 每次使用都**必须署名**：`VFX: KanaWorks_AI (@KanaWorks_AI)`。转发时保留署名和仓库链接。
- **允许**：在自己的作品中修改特效（改色、调时间、裁切、合成）；用于训练 AI 模型。
- **禁止**：把特效本身作为素材包再分发或出售（原样或轻微修改，包括素材商店）；声称是自己做的；暗示 KanaWorks_AI 为你背书。
- **无担保、无支持、免责**：这是按现状提供的免费礼物。
- 代码（`tools/`、`unity/`、`godot/`、`web/`）采用 **MIT** 许可。KanaWorks_AI 的名称和 logo 不在授权范围内
  （[docs/brand](docs/brand/README.md)）。

## 需求

想要这里没有的元素、形态或引擎？请提交[素材需求](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/issues/new?template=asset-request.yml)：
元素、形态、引擎、用途，以及文字描述。请不要附上他人作品的截图或视频。

如果受欢迎，这个库可能会更新。没有时间表，也不保证需求一定会被制作。

## 自己制作

想做自己风格的特效？姊妹项目 [**effekseer-vfx-skill**](https://github.com/kanaworksai-hue/effekseer-vfx-skill) 是一个
agent skill，能让编程代理为 Effekseer、Godot 和 Unity 制作实时游戏特效，包括这样的序列帧。

## 维护者说明

工具需要 Python 3.10+ 以及 Pillow、NumPy（`pip install -r tools/requirements.txt`）。

**添加特效**（制作完成的透明背景、直通 Alpha 的 PNG 图集，帧顺序从左到右、从上到下）：

```sh
python tools/sakuga.py add path/to/sheet.png --id ice_impact_01 --grid 8 8   # 复制、去除元数据、加一行目录
python tools/sakuga.py build      # preview.webp、总览图、README 表格、dist/ 的 zip 和网站
python -m pytest -q               # 校验、隐私和包测试
```

手动方式：把图放到 `assets/<element>/<id>/<id>.png`，在 `catalog.json` 加一行
（`{"id": "ice_impact_01", "element": "ice", "form": "impact", "grid": [8, 8], "added": "YYYY-MM-DD", "version": "1.0.0"}`），
再运行 `python tools/sakuga.py sync`（测量 cell、frames、`bottom_inset` 并写入 sidecar）和 `build`。
选项：`--fps`、`--loop true|false`、`--blend additive`。`python tools/sakuga.py validate` 检查全部内容。

`python tools/sakuga.py install unity <UnityProject>` / `install godot <GodotProject>` 会把包和精灵图复制到项目中用于测试。
CI 在每次推送时运行测试；推送 `v*` 标签会把 zip 发布到 GitHub Release，并把图库发布到 GitHub Pages。

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

<p><b>Made with ♥ by <a href="https://x.com/KanaWorks_AI">KanaWorks_AI</a></b> · X：<a href="https://x.com/KanaWorks_AI">@KanaWorks_AI</a></p>

<p><sub>如果这些特效对你的游戏有帮助，点个 ⭐ 能让更多人找到它。更多项目：<a href="https://github.com/kanaworksai-hue">github.com/kanaworksai-hue</a></sub></p>

<p><sub>Unity、Godot、PixiJS、Phaser 和 Three.js 是各自所有者的商标。本项目为独立项目，与它们无关联，也未获其认可。</sub></p>

</div>
