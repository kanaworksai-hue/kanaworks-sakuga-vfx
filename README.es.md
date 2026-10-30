# KanaWorks SAKUGA VFX

<div align="center">

<img src="docs/assets/banner.jpg" alt="KanaWorks SAKUGA VFX: flipbooks gratuitos de efectos de juego de anime con estilo dibujado a mano" width="820">

<p>
  <a href="LICENSE.md"><img alt="Assets: KanaWorks_AI VFX Asset License" src="https://img.shields.io/badge/assets-KanaWorks__AI_VFX_License-ff6aa2?style=flat-square"></a>
  <a href="LICENSE.md#mit-license-code"><img alt="Code: MIT" src="https://img.shields.io/badge/code-MIT-6b4dff?style=flat-square"></a>
  <img alt="Unity 6 URP" src="https://img.shields.io/badge/Unity_6-URP-111?style=flat-square&logo=unity">
  <img alt="Godot 4" src="https://img.shields.io/badge/Godot-4-478cbf?style=flat-square&logo=godotengine&logoColor=white">
  <img alt="Web" src="https://img.shields.io/badge/Web-Canvas_·_PixiJS_·_Phaser_·_Three.js-19b8d6?style=flat-square">
  <a href="https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx"><img alt="Star on GitHub" src="docs/assets/star-badge.svg"></a>
</p>

<p><a href="https://github.com/kanaworksai-hue"><img src="docs/brand/kanaworks-mark.png" height="22" alt="" align="absmiddle"></a>&nbsp;<a href="https://github.com/kanaworksai-hue"><b>A KanaWorks_AI project</b></a></p>

[English](README.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [한국어](README.ko.md) | [Português (Brasil)](README.pt-BR.md) | Español

</div>

**Una biblioteca gratuita de efectos de juego de anime (sakuga) con estilo dibujado a mano**: fuego, agua, trueno, viento,
veneno y más, como sprite sheets (flipbooks) con fondo transparente que funcionan en Unity, Godot, la web o cualquier
motor que reproduzca un sprite sheet.

Está pensada para quien quiere efectos de habilidad bonitos pero no puede hacerlos con herramientas de IA o no tiene
presupuesto para un artista de VFX: estudiantes, desarrolladores en solitario, estudios pequeños, game jams, creadores
de vídeo. Descarga, da el crédito y úsalos.

- **Listos para reproducir**: cada efecto es un atlas PNG con alfa directo (straight alpha; 2048 px, con márgenes
  transparentes para que ningún fotograma sangre en el siguiente) y un pequeño `.flipbook.json` con la cuadrícula, el
  número de fotogramas, los fps, el bucle y el pivote.
- **Cualquier motor**: paquete Unity 6 URP (shader, componente de reproducción, importador que crea un prefab por efecto,
  escena de demostración), addon de Godot 4 (SpriteFrames por efecto, reproductores 2D y 3D, escenas de demostración),
  un reproductor web de 4 KB y ejemplos para PixiJS, Phaser 3 y Three.js, y JSON al estilo TexturePacker para lo demás.
- **Variantes**: hojas de 2048 y 1024, cada una también en versión aditiva sobre negro.
- **Gratis, con crédito**: gratis para personas y empresas con ingresos anuales por debajo de 1 millón de USD, incluido
  el uso comercial. Consulta la [Licencia](#licencia).

<div align="center">
<img src="docs/assets/overview.webp" alt="Todos los efectos en reproducción" width="820">
</div>

**Galería con vista previa en vivo y descargas:** https://kanaworksai-hue.github.io/kanaworks-sakuga-vfx/
· **Descargas:** [última versión](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)

## Efectos

<!-- assets:start -->
| Vista previa | ID | Elemento | Forma | Fotogramas | Bucle | Añadido |
|:---:|---|---|---|---:|:---:|---|
| <img src="assets/water/water_column_01/preview.webp" width="72" alt="water_column_01"> | [`water_column_01`](assets/water/water_column_01/) | water | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/water/water_impact_01/preview.webp" width="72" alt="water_impact_01"> | [`water_impact_01`](assets/water/water_impact_01/) | water | impact | 45 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_projectile_01/preview.webp" width="72" alt="fire_projectile_01"> | [`fire_projectile_01`](assets/fire/fire_projectile_01/) | fire | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/fire/fire_impact_01/preview.webp" width="72" alt="fire_impact_01"> | [`fire_impact_01`](assets/fire/fire_impact_01/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_01/preview.webp" width="72" alt="thunder_impact_01"> | [`thunder_impact_01`](assets/thunder/thunder_impact_01/) | thunder | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_01/preview.webp" width="72" alt="wind_vortex_01"> | [`wind_vortex_01`](assets/wind/wind_vortex_01/) | wind | vortex | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_01/preview.webp" width="72" alt="poison_impact_01"> | [`poison_impact_01`](assets/poison/poison_impact_01/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_impact_01/preview.webp" width="72" alt="dark_impact_01"> | [`dark_impact_01`](assets/dark/dark_impact_01/) | dark | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_projectile_01/preview.webp" width="72" alt="dark_projectile_01"> | [`dark_projectile_01`](assets/dark/dark_projectile_01/) | dark | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/ice/ice_impact_01/preview.webp" width="72" alt="ice_impact_01"> | [`ice_impact_01`](assets/ice/ice_impact_01/) | ice | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_projectile_01/preview.webp" width="72" alt="ice_projectile_01"> | [`ice_projectile_01`](assets/ice/ice_projectile_01/) | ice | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/light/light_column_01/preview.webp" width="72" alt="light_column_01"> | [`light_column_01`](assets/light/light_column_01/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_impact_01/preview.webp" width="72" alt="light_impact_01"> | [`light_impact_01`](assets/light/light_impact_01/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_projectile_01/preview.webp" width="72" alt="poison_projectile_01"> | [`poison_projectile_01`](assets/poison/poison_projectile_01/) | poison | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/rock/rock_column_01/preview.webp" width="72" alt="rock_column_01"> | [`rock_column_01`](assets/rock/rock_column_01/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_01/preview.webp" width="72" alt="rock_impact_01"> | [`rock_impact_01`](assets/rock/rock_impact_01/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_01/preview.webp" width="72" alt="thunder_strike_01"> | [`thunder_strike_01`](assets/thunder/thunder_strike_01/) | thunder | strike | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_01/preview.webp" width="72" alt="wind_impact_01"> | [`wind_impact_01`](assets/wind/wind_impact_01/) | wind | impact | 41 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_impact_02/preview.webp" width="72" alt="dark_impact_02"> | [`dark_impact_02`](assets/dark/dark_impact_02/) | dark | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_impact_03/preview.webp" width="72" alt="dark_impact_03"> | [`dark_impact_03`](assets/dark/dark_impact_03/) | dark | impact | 64 (8x8) | sí | 2026-10-29 |
| <img src="assets/dark/dark_projectile_02/preview.webp" width="72" alt="dark_projectile_02"> | [`dark_projectile_02`](assets/dark/dark_projectile_02/) | dark | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/dark/dark_projectile_03/preview.webp" width="72" alt="dark_projectile_03"> | [`dark_projectile_03`](assets/dark/dark_projectile_03/) | dark | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/fire/fire_impact_02/preview.webp" width="72" alt="fire_impact_02"> | [`fire_impact_02`](assets/fire/fire_impact_02/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_impact_03/preview.webp" width="72" alt="fire_impact_03"> | [`fire_impact_03`](assets/fire/fire_impact_03/) | fire | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_projectile_02/preview.webp" width="72" alt="fire_projectile_02"> | [`fire_projectile_02`](assets/fire/fire_projectile_02/) | fire | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/fire/fire_projectile_03/preview.webp" width="72" alt="fire_projectile_03"> | [`fire_projectile_03`](assets/fire/fire_projectile_03/) | fire | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/ice/ice_impact_02/preview.webp" width="72" alt="ice_impact_02"> | [`ice_impact_02`](assets/ice/ice_impact_02/) | ice | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_impact_03/preview.webp" width="72" alt="ice_impact_03"> | [`ice_impact_03`](assets/ice/ice_impact_03/) | ice | impact | 64 (8x8) | sí | 2026-10-29 |
| <img src="assets/ice/ice_projectile_02/preview.webp" width="72" alt="ice_projectile_02"> | [`ice_projectile_02`](assets/ice/ice_projectile_02/) | ice | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/ice/ice_projectile_03/preview.webp" width="72" alt="ice_projectile_03"> | [`ice_projectile_03`](assets/ice/ice_projectile_03/) | ice | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/light/light_column_02/preview.webp" width="72" alt="light_column_02"> | [`light_column_02`](assets/light/light_column_02/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_column_03/preview.webp" width="72" alt="light_column_03"> | [`light_column_03`](assets/light/light_column_03/) | light | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/light/light_impact_02/preview.webp" width="72" alt="light_impact_02"> | [`light_impact_02`](assets/light/light_impact_02/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_impact_03/preview.webp" width="72" alt="light_impact_03"> | [`light_impact_03`](assets/light/light_impact_03/) | light | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_02/preview.webp" width="72" alt="poison_impact_02"> | [`poison_impact_02`](assets/poison/poison_impact_02/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_impact_03/preview.webp" width="72" alt="poison_impact_03"> | [`poison_impact_03`](assets/poison/poison_impact_03/) | poison | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_projectile_02/preview.webp" width="72" alt="poison_projectile_02"> | [`poison_projectile_02`](assets/poison/poison_projectile_02/) | poison | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/poison/poison_projectile_03/preview.webp" width="72" alt="poison_projectile_03"> | [`poison_projectile_03`](assets/poison/poison_projectile_03/) | poison | projectile | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/rock/rock_column_02/preview.webp" width="72" alt="rock_column_02"> | [`rock_column_02`](assets/rock/rock_column_02/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_column_03/preview.webp" width="72" alt="rock_column_03"> | [`rock_column_03`](assets/rock/rock_column_03/) | rock | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_02/preview.webp" width="72" alt="rock_impact_02"> | [`rock_impact_02`](assets/rock/rock_impact_02/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_impact_03/preview.webp" width="72" alt="rock_impact_03"> | [`rock_impact_03`](assets/rock/rock_impact_03/) | rock | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_02/preview.webp" width="72" alt="thunder_impact_02"> | [`thunder_impact_02`](assets/thunder/thunder_impact_02/) | thunder | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_03/preview.webp" width="72" alt="thunder_impact_03"> | [`thunder_impact_03`](assets/thunder/thunder_impact_03/) | thunder | impact | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_02/preview.webp" width="72" alt="thunder_strike_02"> | [`thunder_strike_02`](assets/thunder/thunder_strike_02/) | thunder | strike | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_strike_03/preview.webp" width="72" alt="thunder_strike_03"> | [`thunder_strike_03`](assets/thunder/thunder_strike_03/) | thunder | strike | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/water/water_column_02/preview.webp" width="72" alt="water_column_02"> | [`water_column_02`](assets/water/water_column_02/) | water | column | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/water/water_column_03/preview.webp" width="72" alt="water_column_03"> | [`water_column_03`](assets/water/water_column_03/) | water | column | 56 (8x7) | sí | 2026-10-29 |
| <img src="assets/water/water_impact_02/preview.webp" width="72" alt="water_impact_02"> | [`water_impact_02`](assets/water/water_impact_02/) | water | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_impact_03/preview.webp" width="72" alt="water_impact_03"> | [`water_impact_03`](assets/water/water_impact_03/) | water | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_02/preview.webp" width="72" alt="wind_impact_02"> | [`wind_impact_02`](assets/wind/wind_impact_02/) | wind | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_impact_03/preview.webp" width="72" alt="wind_impact_03"> | [`wind_impact_03`](assets/wind/wind_impact_03/) | wind | impact | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_02/preview.webp" width="72" alt="wind_vortex_02"> | [`wind_vortex_02`](assets/wind/wind_vortex_02/) | wind | vortex | 56 (8x7) | no | 2026-10-29 |
| <img src="assets/wind/wind_vortex_03/preview.webp" width="72" alt="wind_vortex_03"> | [`wind_vortex_03`](assets/wind/wind_vortex_03/) | wind | vortex | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_aura_01/preview.webp" width="72" alt="dark_aura_01"> | [`dark_aura_01`](assets/dark/dark_aura_01/) | dark | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/dark/dark_charge_01/preview.webp" width="72" alt="dark_charge_01"> | [`dark_charge_01`](assets/dark/dark_charge_01/) | dark | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/dark/dark_shield_01/preview.webp" width="72" alt="dark_shield_01"> | [`dark_shield_01`](assets/dark/dark_shield_01/) | dark | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/dark/dark_slash_01/preview.webp" width="72" alt="dark_slash_01"> | [`dark_slash_01`](assets/dark/dark_slash_01/) | dark | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_aura_01/preview.webp" width="72" alt="fire_aura_01"> | [`fire_aura_01`](assets/fire/fire_aura_01/) | fire | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/fire/fire_charge_01/preview.webp" width="72" alt="fire_charge_01"> | [`fire_charge_01`](assets/fire/fire_charge_01/) | fire | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/fire/fire_shield_01/preview.webp" width="72" alt="fire_shield_01"> | [`fire_shield_01`](assets/fire/fire_shield_01/) | fire | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/fire/fire_slash_01/preview.webp" width="72" alt="fire_slash_01"> | [`fire_slash_01`](assets/fire/fire_slash_01/) | fire | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_aura_01/preview.webp" width="72" alt="ice_aura_01"> | [`ice_aura_01`](assets/ice/ice_aura_01/) | ice | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/ice/ice_charge_01/preview.webp" width="72" alt="ice_charge_01"> | [`ice_charge_01`](assets/ice/ice_charge_01/) | ice | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/ice/ice_shield_01/preview.webp" width="72" alt="ice_shield_01"> | [`ice_shield_01`](assets/ice/ice_shield_01/) | ice | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/ice/ice_slash_01/preview.webp" width="72" alt="ice_slash_01"> | [`ice_slash_01`](assets/ice/ice_slash_01/) | ice | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_aura_01/preview.webp" width="72" alt="light_aura_01"> | [`light_aura_01`](assets/light/light_aura_01/) | light | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/light/light_charge_01/preview.webp" width="72" alt="light_charge_01"> | [`light_charge_01`](assets/light/light_charge_01/) | light | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/light/light_shield_01/preview.webp" width="72" alt="light_shield_01"> | [`light_shield_01`](assets/light/light_shield_01/) | light | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/light/light_slash_01/preview.webp" width="72" alt="light_slash_01"> | [`light_slash_01`](assets/light/light_slash_01/) | light | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_aura_01/preview.webp" width="72" alt="poison_aura_01"> | [`poison_aura_01`](assets/poison/poison_aura_01/) | poison | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/poison/poison_charge_01/preview.webp" width="72" alt="poison_charge_01"> | [`poison_charge_01`](assets/poison/poison_charge_01/) | poison | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/poison/poison_shield_01/preview.webp" width="72" alt="poison_shield_01"> | [`poison_shield_01`](assets/poison/poison_shield_01/) | poison | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/poison/poison_slash_01/preview.webp" width="72" alt="poison_slash_01"> | [`poison_slash_01`](assets/poison/poison_slash_01/) | poison | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_aura_01/preview.webp" width="72" alt="rock_aura_01"> | [`rock_aura_01`](assets/rock/rock_aura_01/) | rock | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/rock/rock_charge_01/preview.webp" width="72" alt="rock_charge_01"> | [`rock_charge_01`](assets/rock/rock_charge_01/) | rock | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/rock/rock_shield_01/preview.webp" width="72" alt="rock_shield_01"> | [`rock_shield_01`](assets/rock/rock_shield_01/) | rock | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/rock/rock_slash_01/preview.webp" width="72" alt="rock_slash_01"> | [`rock_slash_01`](assets/rock/rock_slash_01/) | rock | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_aura_01/preview.webp" width="72" alt="thunder_aura_01"> | [`thunder_aura_01`](assets/thunder/thunder_aura_01/) | thunder | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/thunder/thunder_charge_01/preview.webp" width="72" alt="thunder_charge_01"> | [`thunder_charge_01`](assets/thunder/thunder_charge_01/) | thunder | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/thunder/thunder_shield_01/preview.webp" width="72" alt="thunder_shield_01"> | [`thunder_shield_01`](assets/thunder/thunder_shield_01/) | thunder | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/thunder/thunder_slash_01/preview.webp" width="72" alt="thunder_slash_01"> | [`thunder_slash_01`](assets/thunder/thunder_slash_01/) | thunder | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_aura_01/preview.webp" width="72" alt="water_aura_01"> | [`water_aura_01`](assets/water/water_aura_01/) | water | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/water/water_charge_01/preview.webp" width="72" alt="water_charge_01"> | [`water_charge_01`](assets/water/water_charge_01/) | water | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/water/water_shield_01/preview.webp" width="72" alt="water_shield_01"> | [`water_shield_01`](assets/water/water_shield_01/) | water | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/water/water_slash_01/preview.webp" width="72" alt="water_slash_01"> | [`water_slash_01`](assets/water/water_slash_01/) | water | slash | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_aura_01/preview.webp" width="72" alt="wind_aura_01"> | [`wind_aura_01`](assets/wind/wind_aura_01/) | wind | aura | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/wind/wind_charge_01/preview.webp" width="72" alt="wind_charge_01"> | [`wind_charge_01`](assets/wind/wind_charge_01/) | wind | charge | 64 (8x8) | no | 2026-10-29 |
| <img src="assets/wind/wind_shield_01/preview.webp" width="72" alt="wind_shield_01"> | [`wind_shield_01`](assets/wind/wind_shield_01/) | wind | shield | 40 (8x5) | sí | 2026-10-29 |
| <img src="assets/wind/wind_slash_01/preview.webp" width="72" alt="wind_slash_01"> | [`wind_slash_01`](assets/wind/wind_slash_01/) | wind | slash | 64 (8x8) | no | 2026-10-29 |
<!-- assets:end -->

Formas: `projectile` se repite mientras vuela; `impact` se reproduce una vez al golpear; `column` se levanta desde el
suelo; `vortex` gira en el sitio; `strike` cae desde arriba al suelo; `shield` se repite alrededor de un personaje; `aura`
se repite a los pies de un personaje; `charge` es el lanzamiento (combina con todos los projectiles de su elemento); `slash`
es un golpe cuerpo a cuerpo. Los nombres siguen `elemento_forma_NN` (`element_form_NN`): `_01` es el aspecto clásico, `_02`
y `_03` son variantes de estilo (por ejemplo, `fire_projectile_02` fuego de almas y `fire_projectile_03` meteoro). La receta da
a cada uno su categoría (una variante puede ser `area`, `beam` o `buff`).

## Efectos por capas (recetas)

Cada efecto tiene además una **receta** (en `catalog.json` y en su sidecar) que convierte el dibujo en un efecto de
habilidad listo para usar: categoría y casos de uso, los efectos con los que se combina (carga, proyectil, impacto), dónde se
coloca (centro o suelo; billboard, de pie, plano o girado a lo largo del vuelo), su tamaño en metros, el fotograma de contacto
y las capas secundarias: destello de contacto, anillo de onda expansiva, chispas, escombros, humo cel, calcomanía en el suelo,
una luz con curva de intensidad, temblor de cámara, hit stop y sonido. El paquete de Unity crea con ella un `<id>_fx.prefab`,
el addon de Godot un `SakugaEffect3D`, y `web/sakuga-fx.js` la reproduce en un canvas. Las texturas blancas, las calcomanías
y los sonidos compartidos están en [`assets/_shared/`](assets/_shared/).

```sh
python tools/sakuga.py suggest "ice arrow hitting a slime"
#  17.5  ice_projectile_02  projectile glacier  ice lance, ice arrow, ice shard
#  ...
#  chain: ice_charge_01 -> ice_projectile_02 -> ice_impact_01
```

```csharp
SakugaFX.Cast("ice_projectile_02", hand.position, slime, p => slime.Damage(10));   // Unity: charge, flight, impact
```

Si usas un agente de programación, empieza por [AGENTS.md](AGENTS.md) (y [llms.txt](llms.txt)).

## Descargas

Cada [versión](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest) incluye:

| Archivo | Para |
|---|---|
| `kanaworks-sakuga-vfx-<version>-unity.zip` | Unity 6 (URP): descomprímelo en la carpeta del proyecto |
| `kanaworks-sakuga-vfx-<version>-godot.zip` | Godot 4.3+: descomprímelo en la carpeta del proyecto |
| `kanaworks-sakuga-vfx-<version>-web.zip` | reproductor web, ejemplos y galería, con todas las hojas |
| `kanaworks-sakuga-vfx-<version>-flipbooks.zip` | todas las hojas en todas sus variantes, para cualquier motor |
| `assets/<id>.zip` (en la galería) | un efecto, todas las variantes |

También puedes tomar un solo `assets/<element>/<id>/<id>.png` y su `.flipbook.json` directamente de este repositorio.

## Inicio rápido

### Unity 6 (URP)

1. Descomprime `...-unity.zip` en la carpeta del proyecto. Añade `Packages/com.kanaworks.sakuga-vfx/` y
   `Assets/KanaWorksSakugaVFX/Flipbooks/`.
2. Unity importa las hojas con los ajustes correctos y crea `<id>.prefab` junto a cada una.
   **Tools > KanaWorks SAKUGA VFX > Build demo scene** y luego Play.
3. En código:

```csharp
public SakugaFlipbook fireHit;                                    // arrastra fire_impact_01.prefab aquí
SakugaFlipbook.Spawn(fireHit, hitPoint, Quaternion.identity);     // se reproduce una vez y se destruye
```

Shader `KanaWorks/SakugaFlipbook`: mezcla alfa o aditiva, billboard orientado a la cámara o vertical (las columnas se
mantienen verticales con la raíz en el suelo), tinte HDR para Bloom. Para un Particle System usa
`M_<id>_Particles.mat` con Texture Sheet Animation ajustado a la cuadrícula de la hoja.

### Godot 4.3+

1. Descomprime `...-godot.zip` en la carpeta del proyecto (añade `addons/kanaworks_sakuga_vfx/` y `sakuga_demo/`).
2. Abre `res://sakuga_demo/demo_2d.tscn` o `demo_3d.tscn` y pulsa F6.
3. En código:

```gdscript
const FIRE_HIT := preload("res://addons/kanaworks_sakuga_vfx/flipbooks/fire/fire_impact_01/fire_impact_01.tres")
SakugaFlipbook2D.spawn(FIRE_HIT, self, hit_position)     # 2D (AnimatedSprite2D), se libera al terminar
SakugaFlipbook3D.spawn(FIRE_HIT, self, hit_point)        # billboard 3D, el mismo .tres
```

Cada `.tres` es un `SpriteFrames` normal, así que funciona en cualquier `AnimatedSprite2D`.

### Web

```html
<canvas id="fx" width="512" height="512"></canvas>
<script type="module">
  import { load, mount } from './web/sakuga-player.js';
  mount(document.getElementById('fx'), await load('./assets/fire/fire_impact_01/fire_impact_01.flipbook.json'));
</script>
```

`sakuga-player.js` no tiene dependencias (Canvas2D, unos 4 KB): `load()`, `Flipbook` (`update(dt)`, `draw(ctx, x, y)`),
`mount()` y `spritesheetData()` para PixiJS y Phaser. Ejemplos: [`web/examples/`](web/examples/) (Canvas2D, PixiJS 8,
Phaser 3, Three.js; las bibliotecas se cargan desde un CDN con versiones fijas). Sirve la carpeta por HTTP (por ejemplo
`python -m http.server`) y abre `web/`.

### Cualquier otro motor

Lee el sidecar y reproduce la cuadrícula de izquierda a derecha y de arriba abajo:

| Campo | Significado |
|---|---|
| `grid` | `[columnas, filas]` |
| `cell` | `[ancho, alto]` de un fotograma en píxeles |
| `frames` | fotogramas a reproducir (las primeras `frames` celdas) |
| `fps`, `loop` | velocidad (30) y si se repite (proyectiles) |
| `blend` | `alpha` (alfa directo) o `additive` |
| `pivot` | `[x, y]` en la celda, 0-1 desde la esquina superior izquierda: el centro, o la raíz de una columna |
| `bottom_inset` | espacio vacío bajo la raíz de una columna, como fracción de la altura de la celda |
| `source` | el PNG que está al lado |

La versión publicada también incluye `<id>.atlas.json` (TexturePacker "JSON Hash" con la lista `animations`).

## Crédito

Pon esto donde uses o vuelvas a publicar los efectos (créditos del juego, descripción del vídeo, pie de página):

```
VFX: KanaWorks_AI (@KanaWorks_AI)
```

Al volver a publicar los efectos o sus vistas previas, conserva el crédito y un enlace a
https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx.

## Licencia

Esta es una **biblioteca de VFX gratuita**, no un proyecto de código abierto: los efectos se pueden usar gratis con condiciones.
Resumen (se aplica la [licencia completa](LICENSE.md) y prevalece su texto en japonés):

- **Gratis** para uso personal, no comercial y comercial de personas y empresas cuyos ingresos brutos anuales (ejercicio
  anterior, incluidas las afiliadas) sean **inferiores a USD 1.000.000**. A partir de esa cifra, contacta con
  [KanaWorks_AI](https://x.com/KanaWorks_AI) para obtener una licencia aparte.
- **Crédito obligatorio** en todo uso: `VFX: KanaWorks_AI (@KanaWorks_AI)`. Las republicaciones conservan el crédito y el enlace al repositorio.
- **Permitido**: modificar los efectos dentro de tu obra (colores, tiempo, recorte, composición); entrenar modelos de IA.
- **Prohibido**: redistribuir o vender los efectos en sí como paquete de assets (tal cual o ligeramente modificados,
  incluidas las tiendas de assets); afirmar que los hiciste tú; dar a entender que KanaWorks_AI te respalda.
- **Sin garantía, sin soporte, sin responsabilidad**: un regalo gratuito, tal cual.
- El código (`tools/`, `unity/`, `godot/`, `web/`) es **MIT**. El nombre y el logo de KanaWorks_AI no tienen licencia
  ([docs/brand](docs/brand/README.md)).

## Peticiones

¿Quieres un elemento, una forma o un motor que no esté aquí? Abre una
[petición de asset](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/issues/new?template=asset-request.yml):
elemento, forma, motor, para qué es y una descripción con palabras. No adjuntes capturas ni vídeos del trabajo de otras personas.

La biblioteca puede actualizarse si a la gente le gusta. No hay calendario ni promesa de que una petición se haga.

## Haz los tuyos

¿Quieres efectos con tu propio estilo? El proyecto hermano
[**effekseer-vfx-skill**](https://github.com/kanaworksai-hue/effekseer-vfx-skill) es una agent skill que permite a un
agente de programación crear VFX de juego en tiempo real para Effekseer, Godot y Unity, incluidos flipbooks como estos.

## Para mantenedores

Las herramientas necesitan Python 3.10+ con Pillow y NumPy (`pip install -r tools/requirements.txt`).

**Añadir un efecto** (un atlas PNG terminado, transparente, con alfa directo, fotogramas de izquierda a derecha y de arriba abajo):

```sh
python tools/sakuga.py add path/to/sheet.png --id ice_impact_01 --grid 8 8   # lo copia, quita metadatos, añade la línea al catálogo
python tools/sakuga.py build      # preview.webp, vista general, tablas de los README, zips en dist/ y sitio
python -m pytest -q               # pruebas de validación, privacidad y paquetes
```

O a mano: pon la hoja en `assets/<element>/<id>/<id>.png`, añade una línea a `catalog.json`
(`{"id": "ice_impact_01", "element": "ice", "form": "impact", "grid": [8, 8], "added": "YYYY-MM-DD", "version": "1.0.0"}`),
y ejecuta `python tools/sakuga.py sync` (mide cell, frames y `bottom_inset` y escribe el sidecar) y `build`.
Opciones: `--fps`, `--loop true|false`, `--blend additive`. `python tools/sakuga.py validate` lo comprueba todo.

`python tools/sakuga.py install unity <UnityProject>` / `install godot <GodotProject>` copia el paquete y las hojas a un
proyecto para probar. La CI ejecuta las pruebas en cada push; una etiqueta `v*` publica los zips en una versión de
GitHub y la galería en GitHub Pages.

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

<p><b>Made with ♥ by <a href="https://x.com/KanaWorks_AI">KanaWorks_AI</a></b> · <a href="https://x.com/KanaWorks_AI">@KanaWorks_AI</a> en X</p>

<p><sub>Si los efectos ayudan a tu juego, una ⭐ ayuda a que otros los encuentren. Más proyectos: <a href="https://github.com/kanaworksai-hue">github.com/kanaworksai-hue</a></sub></p>

<p><sub>Unity, Godot, PixiJS, Phaser y Three.js son marcas de sus respectivos propietarios. Este es un proyecto independiente, sin afiliación ni respaldo de ellos.</sub></p>

</div>
