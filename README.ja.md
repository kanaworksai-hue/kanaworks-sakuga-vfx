# KanaWorks SAKUGA VFX

<div align="center">

<!-- TODO(banner): docs/assets/banner.jpg は別セッションで制作中。このパスのままにする。 -->
<img src="docs/assets/banner.jpg" alt="KanaWorks SAKUGA VFX：無料の手描きアニメ調ゲームエフェクト連番素材" width="820">

<p>
  <a href="LICENSE.md"><img alt="Assets: KanaWorks_AI VFX Asset License" src="https://img.shields.io/badge/assets-KanaWorks__AI_VFX_License-ff6aa2?style=flat-square"></a>
  <a href="LICENSE.md#mit-license-code"><img alt="Code: MIT" src="https://img.shields.io/badge/code-MIT-6b4dff?style=flat-square"></a>
  <img alt="Unity 6 URP" src="https://img.shields.io/badge/Unity_6-URP-111?style=flat-square&logo=unity">
  <img alt="Godot 4" src="https://img.shields.io/badge/Godot-4-478cbf?style=flat-square&logo=godotengine&logoColor=white">
  <img alt="Web" src="https://img.shields.io/badge/Web-Canvas_·_PixiJS_·_Phaser_·_Three.js-19b8d6?style=flat-square">
  <a href="https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/stargazers"><img alt="GitHub stars" src="https://img.shields.io/github/stars/kanaworksai-hue/kanaworks-sakuga-vfx?style=flat-square&logo=github&label=stars&color=f5c518"></a>
</p>

<p><a href="https://github.com/kanaworksai-hue"><img src="docs/brand/kanaworks-mark.png" height="22" alt="" align="absmiddle"></a>&nbsp;<a href="https://github.com/kanaworksai-hue"><b>A KanaWorks_AI project</b></a></p>

[English](README.md) | [简体中文](README.zh-CN.md) | 日本語 | [한국어](README.ko.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md)

</div>

**手描きアニメ調（作画系）のゲームエフェクトを無料で配布するライブラリです。** 炎・水・雷・風・毒などのエフェクトを、
背景透過の連番スプライトシート（フリップブック）として提供します。Unity、Godot、Web、そしてスプライトシートを再生できる
あらゆるエンジンでそのまま使えます。

AI ツールでエフェクトを作れない人、エフェクトアーティストに依頼する予算がない人のために作りました。学生、個人開発者、
小さなスタジオ、ゲームジャム、映像制作。ダウンロードして、クレジットを書いて、そのまま使ってください。

- **すぐ再生できる**：各エフェクトはストレートアルファの PNG アトラス（2048px。各コマの周囲に透明の余白があり、隣のコマが
  にじまない）と、グリッド・コマ数・fps・ループ・ピボットを書いた小さな `.flipbook.json` のセットです。
- **どのエンジンでも**：Unity 6 URP パッケージ（シェーダー、再生コンポーネント、エフェクトごとにプレハブを作るインポーター、
  デモシーン）、Godot 4 アドオン（エフェクトごとの SpriteFrames、2D/3D プレイヤー、デモシーン）、約 4KB の Web プレイヤーと
  PixiJS・Phaser 3・Three.js のサンプル、その他のエンジン向けに TexturePacker 形式の JSON。
- **バリエーション**：2048 と 1024 のシート、それぞれ黒背景の加算合成版つき。
- **無料・クレジット表記のみ**：年間総収入 100 万米ドル未満の個人・企業は商用利用も含めて無料。[ライセンス](#ライセンス)を参照。

<div align="center">
<img src="docs/assets/overview.webp" alt="全エフェクトの再生プレビュー" width="820">
</div>

**プレビューとダウンロードができるギャラリー：** https://kanaworksai-hue.github.io/kanaworks-sakuga-vfx/
· **ダウンロード：** [最新リリース](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)

## エフェクト一覧

<!-- assets:start -->
| プレビュー | ID | 属性 | 形 | フレーム | ループ | 追加日 |
|:---:|---|---|---|---:|:---:|---|
| <img src="assets/water/water_column_01/preview.webp" width="72" alt="water_column_01"> | [`water_column_01`](assets/water/water_column_01/) | water | column | 56 (8x7) | いいえ | 2026-10-29 |
| <img src="assets/water/water_impact_01/preview.webp" width="72" alt="water_impact_01"> | [`water_impact_01`](assets/water/water_impact_01/) | water | impact | 64 (8x8) | いいえ | 2026-10-29 |
| <img src="assets/fire/fire_projectile_01/preview.webp" width="72" alt="fire_projectile_01"> | [`fire_projectile_01`](assets/fire/fire_projectile_01/) | fire | projectile | 40 (8x5) | はい | 2026-10-29 |
| <img src="assets/fire/fire_impact_01/preview.webp" width="72" alt="fire_impact_01"> | [`fire_impact_01`](assets/fire/fire_impact_01/) | fire | impact | 64 (8x8) | いいえ | 2026-10-29 |
| <img src="assets/thunder/thunder_impact_01/preview.webp" width="72" alt="thunder_impact_01"> | [`thunder_impact_01`](assets/thunder/thunder_impact_01/) | thunder | impact | 64 (8x8) | いいえ | 2026-10-29 |
| <img src="assets/wind/wind_vortex_01/preview.webp" width="72" alt="wind_vortex_01"> | [`wind_vortex_01`](assets/wind/wind_vortex_01/) | wind | vortex | 64 (8x8) | いいえ | 2026-10-29 |
| <img src="assets/poison/poison_impact_01/preview.webp" width="72" alt="poison_impact_01"> | [`poison_impact_01`](assets/poison/poison_impact_01/) | poison | impact | 64 (8x8) | いいえ | 2026-10-29 |
<!-- assets:end -->

形（form）：`projectile` は飛んでいる間ループ、`impact` は着弾時に 1 回再生、`column` は地面から立ち上がる柱、
`vortex` はその場で渦巻く、`strike` は上から地面に落ちる。名前は `属性_形_番号`（`element_form_NN`）です。

## ダウンロード

各[リリース](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/releases/latest)には次のファイルがあります。

| ファイル | 用途 |
|---|---|
| `kanaworks-sakuga-vfx-<version>-unity.zip` | Unity 6（URP）：プロジェクトフォルダに展開 |
| `kanaworks-sakuga-vfx-<version>-godot.zip` | Godot 4.3+：プロジェクトフォルダに展開 |
| `kanaworks-sakuga-vfx-<version>-web.zip` | Web プレイヤー、サンプル、ギャラリーと全シート |
| `kanaworks-sakuga-vfx-<version>-flipbooks.zip` | 全シートの全バリエーション（どのエンジンでも） |
| `assets/<id>.zip`（ギャラリー） | エフェクト 1 つ分、全バリエーション |

このリポジトリから `assets/<element>/<id>/<id>.png` と `.flipbook.json` を直接取ってきても使えます。

## クイックスタート

### Unity 6（URP）

1. `...-unity.zip` をプロジェクトフォルダに展開します。`Packages/com.kanaworks.sakuga-vfx/` と
   `Assets/KanaWorksSakugaVFX/Flipbooks/` が追加されます。
2. Unity が正しい設定でシートを読み込み、各シートの横に `<id>.prefab` を作ります。
   **Tools > KanaWorks SAKUGA VFX > Build demo scene** を実行して Play。
3. コードから：

```csharp
public SakugaFlipbook fireHit;                                    // fire_impact_01.prefab をドラッグ
SakugaFlipbook.Spawn(fireHit, hitPoint, Quaternion.identity);     // 1 回再生して自動で破棄
```

シェーダー `KanaWorks/SakugaFlipbook`：アルファブレンド／加算、カメラ向きまたは直立ビルボード（柱は根元を地面に付けたまま
垂直を保つ）、Bloom 用の HDR ティント。パーティクルシステムでは `M_<id>_Particles.mat` を使い、Texture Sheet Animation を
シートのグリッドに合わせます。

### Godot 4.3+

1. `...-godot.zip` をプロジェクトフォルダに展開します（`addons/kanaworks_sakuga_vfx/` と `sakuga_demo/` が追加されます）。
2. `res://sakuga_demo/demo_2d.tscn` か `demo_3d.tscn` を開いて F6。
3. コードから：

```gdscript
const FIRE_HIT := preload("res://addons/kanaworks_sakuga_vfx/flipbooks/fire/fire_impact_01/fire_impact_01.tres")
SakugaFlipbook2D.spawn(FIRE_HIT, self, hit_position)     # 2D（AnimatedSprite2D）、終わったら自動で削除
SakugaFlipbook3D.spawn(FIRE_HIT, self, hit_point)        # 3D ビルボード、同じ .tres
```

`.tres` は普通の `SpriteFrames` なので、どの `AnimatedSprite2D` でも使えます。

### Web

```html
<canvas id="fx" width="512" height="512"></canvas>
<script type="module">
  import { load, mount } from './web/sakuga-player.js';
  mount(document.getElementById('fx'), await load('./assets/fire/fire_impact_01/fire_impact_01.flipbook.json'));
</script>
```

`sakuga-player.js` は依存なし（Canvas2D、約 4KB）：`load()`、`Flipbook`（`update(dt)`、`draw(ctx, x, y)`）、`mount()`、
PixiJS・Phaser 用の `spritesheetData()`。サンプルは [`web/examples/`](web/examples/)（Canvas2D、PixiJS 8、Phaser 3、
Three.js。ライブラリはバージョン固定の CDN から読み込み）。フォルダを HTTP で配信して（例：`python -m http.server`）
`web/` を開いてください。

### その他のエンジン

サイドカーを読み、グリッドを左から右、上から下の順に再生します。

| フィールド | 意味 |
|---|---|
| `grid` | `[列, 行]` |
| `cell` | 1 コマの `[幅, 高さ]`（px） |
| `frames` | 再生するコマ数（先頭から `frames` 個） |
| `fps`, `loop` | 再生速度（30）とループするか（projectile） |
| `blend` | `alpha`（ストレートアルファ）または `additive` |
| `pivot` | コマ内の `[x, y]`、左上原点の 0〜1：中心、または柱の根元 |
| `bottom_inset` | 柱の根元より下の空白（コマの高さに対する割合） |
| `source` | 隣にある PNG |

リリースには TexturePacker「JSON Hash」形式（`animations` つき）の `<id>.atlas.json` も入っています。

## クレジット

エフェクトを使う・再投稿するときは、必ず次のクレジットを表記してください（ゲームのクレジット、動画の概要欄、ページのフッターなど）。

```
VFX: KanaWorks_AI (@KanaWorks_AI)
```

エフェクトやプレビューを再投稿・転載するときは、クレジットと
https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx へのリンクを残してください。

## ライセンス

これは**無料の VFX ライブラリ**であり、オープンソースプロジェクトではありません。エフェクトは条件付きで無料で使えます。
要約（[ライセンス全文](LICENSE.md)が適用され、日本語版が正文です）：

- 年間総収入（直前の会計年度、関連会社を含む）が **100 万米ドル未満**の個人・企業は、個人・非営利・商用を問わず**無料**。
  それ以上の場合は [KanaWorks_AI](https://x.com/KanaWorks_AI) に連絡して別途ライセンスを取得してください。
- すべての利用で**クレジット表記が必要**：`VFX: KanaWorks_AI (@KanaWorks_AI)`。再投稿ではクレジットとリポジトリのリンクを残す。
- **できること**：自分の作品の中での改変（色変更、タイミング変更、切り抜き、合成）、AI モデルの学習への利用。
- **禁止**：エフェクトそのものを素材集・アセットパックとして再配布・販売すること（そのまま、または軽微な改変、アセットストア含む）、
  自作と偽ること、KanaWorks_AI が推薦しているかのように見せること。
- **無保証・サポートなし・免責**：現状のままの無料の贈り物です。
- コード（`tools/`、`unity/`、`godot/`、`web/`）は **MIT**。KanaWorks_AI の名称とロゴはライセンス対象外です
  （[docs/brand](docs/brand/README.md)）。

## リクエスト

欲しい属性・形・エンジンがあれば、[素材リクエスト](https://github.com/kanaworksai-hue/kanaworks-sakuga-vfx/issues/new?template=asset-request.yml)
の Issue を作ってください：属性、形、エンジン、用途、文章での説明。他の人の作品のスクリーンショットや動画は添付しないでください。

好評であれば更新することがあります。予定はなく、リクエストが必ず実現するとは限りません。

## 自分で作る

自分のスタイルでエフェクトを作りたい場合は、姉妹プロジェクト
[**effekseer-vfx-skill**](https://github.com/kanaworksai-hue/effekseer-vfx-skill) をどうぞ。コーディングエージェントで
Effekseer・Godot・Unity 用のリアルタイムゲームエフェクト（このようなフリップブックを含む）を作れるエージェントスキルです。

## メンテナー向け

ツールには Python 3.10+ と Pillow、NumPy が必要です（`pip install -r tools/requirements.txt`）。

**エフェクトの追加**（完成した背景透過・ストレートアルファの PNG アトラス、コマは左から右・上から下）：

```sh
python tools/sakuga.py add path/to/sheet.png --id ice_impact_01 --grid 8 8   # コピー、メタデータ削除、カタログに 1 行追加
python tools/sakuga.py build      # preview.webp、一覧、README の表、dist/ の zip とサイト
python -m pytest -q               # 検証・プライバシー・パッケージのテスト
```

手作業の場合：`assets/<element>/<id>/<id>.png` にシートを置き、`catalog.json` に 1 行追加
（`{"id": "ice_impact_01", "element": "ice", "form": "impact", "grid": [8, 8], "added": "YYYY-MM-DD", "version": "1.0.0"}`）、
`python tools/sakuga.py sync`（cell・frames・`bottom_inset` を計測してサイドカーを書く）と `build` を実行。
オプション：`--fps`、`--loop true|false`、`--blend additive`。`python tools/sakuga.py validate` で全体をチェックします。

`python tools/sakuga.py install unity <UnityProject>` / `install godot <GodotProject>` でパッケージとシートを
プロジェクトにコピーしてテストできます。CI はプッシュごとにテストを実行し、`v*` タグで zip を GitHub リリースに、
ギャラリーを GitHub Pages に公開します。

<br>

<div align="center">

<a href="https://x.com/KanaWorks_AI"><img src="docs/brand/kanaworks-logo.png" width="130" alt="KanaWorks_AI"></a>

<p><b>Made with ♥ by <a href="https://x.com/KanaWorks_AI">KanaWorks_AI</a></b> · X の <a href="https://x.com/KanaWorks_AI">@KanaWorks_AI</a></p>

<p><sub>エフェクトが役に立ったら ⭐ をもらえると、ほかの人にも見つけてもらいやすくなります。ほかのプロジェクト：<a href="https://github.com/kanaworksai-hue">github.com/kanaworksai-hue</a></sub></p>

<p><sub>Unity、Godot、PixiJS、Phaser、Three.js は各所有者の商標です。本プロジェクトは独立したもので、これらと提携・承認関係にはありません。</sub></p>

</div>
