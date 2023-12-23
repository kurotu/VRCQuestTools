# 変更履歴

このプロジェクトのすべての重要な変更はこのファイルに記録されます。

このフォーマットは [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) に基づいており、このプロジェクトは [Semantic Versioning](https://semver.org/spec/v2.0.0.html) に準拠しています。

## [Unreleased]

### 追加
- アバターに変換設定を保存する `VQT Avatar Converter Settings` コンポーネントを追加
- マテリアルごとにマテリアル変換設定を変更する機能を追加
- Toon Lit 以外のシェーダーへのマテリアル変換設定を追加
    - MatCap Lit
    - マテリアル置換
- マテリアル変換設定に「ノーマルマップから影を生成する」オプションを追加
- Poiyomi Toon シェーダーからのマテリアル変換を追加。対応している機能は以下の通り:
    - メインカラー
    - ノーマルマップによる影
    - 発光 (0～3)
- Missing 状態のコンポーネントがあるときの警告内に対象オブジェクトを表示する機能を追加
- VRCQuestTools Extra の機能を統合
    - FinalIK のコンポーネントを削除する機能を追加
    - VirtualLens2 導入時専用の処理を追加

### 変更
- VRChat SDK 3.3.0 以降が必要
- unitypackage のインポートパスを `Assets/KRT/VRCQuestTools` から `Packages/com.github.kurotu.vrc-quest-tools` に変更
- UPM のインポート URL を `https://github.com/kurotu/VRCQuestTools.git?path=Packages/com.github.kurotu.vrc-quest-tools` に変更
- 更新通知をシーンビューの代わりに Inspector に表示するように変更
- 最新バージョンの確認処理を1日1回に変更
- **VRCQuestTools** メニューを **Tools** メニュー内に移動
- メニュー項目の配置を変更

### 削除
- **Check for Updates** メニューを削除。代わりに VCC または Inspector を使用
- シーンから頂点カラーを検出して自動削除する機能を削除。代わりに **Remove All Vertex Colors** メニューを使用

### 修正
- Network ID に割り当てられていた GameObject が実際には存在しないとき変換に失敗する問題を修正
- テクスチャ生成時にリニアテクスチャを sRGB で読み込んでいた問題を修正

## [1.14.0] - 2023-12-09

### 追加
- Unity 2022 のサポート (2019 にも引き続き対応)
- Material Variant からのテクスチャ生成に対応

## [1.13.5] - 2023-12-03

### 修正
- lilToon のメインテクスチャが Texture2D でない場合にテクスチャの生成に失敗する問題を修正

## [1.13.4] - 2023-10-12

### 修正
- VRChat SDK のコントロールパネルでプラットフォームを Android に変更するとコンパイルエラーになることがある問題を修正

## [1.13.3] - 2023-09-17

### 変更
- Android ビルドターゲットのときにシーン内のアバターが1つでも Quest 用にアップロードできないときの警告は VRCSDK 3.3.0 では不要になったため表示しないように変更

### 修正
- 頂点カラーの存在を確認する判定の誤りを修正
- VertexColorRemover コンポーネントがあると VRCSDK 3.3.0 でアバターをアップロードできないことがある問題を修正
- PhysBone の衝突判定数に Endpoint Position および Multi Child Type が反映されない問題を修正

## [1.13.2] - 2023-09-15

### 変更
- デフォルトの保存先フォルダ名に使用できない文字があるときアンダースコアを使用するように変更

### 修正
- 保存先フォルダ名に使用できない文字があると変換に失敗する問題を修正
- 保存先フォルダ名の末尾に半角スペースがあると変換に失敗する問題を修正

## [1.13.1] - 2023-09-12

### 修正
- PhysBone の Ignore Transforms に None があるとパフォーマンス推定値を表示できない問題を修正

## [1.13.0] - 2023-09-04

### 追加
- PhysBones Remover ウィンドウに推定のパフォーマンス値を表示する機能を追加
- プレリリース版の更新通知を表示する機能を追加 (プレリリース版を使用している場合のみ)
- ドキュメンテーションサイトを追加 (https://kurotu.github.io/VRCQuestTools/ja/)

### 修正
- 元のテクスチャが .asset ファイルの場合に発生する不正なキャストエラーを修正
- lilToon の「発光設定 - メインカラーの強度」が変換後のテクスチャに反映されない問題を修正

## [1.12.1] - 2023-07-02

### 修正
- NewtonSoft Json のインポートエラーを修正

## [1.12.0] - 2023-06-30

### 追加
- "Remove All Vertex Colors" メニューをメインメニューに追加

### 変更
- GitHub API ではなく VPM リポジトリを使用して更新を確認するように変更
- Edit モード時のみ更新を確認するように変更
- Play モード時に頂点カラーを削除しないように変更
- Play モード時にシーンの検証を行わないように変更

### 削除
- "Auto Remove Vertex Colors" メニューを削除。代わりにアバターに対して "Remove All Vertex Colors" メニューを使用する
- 頂点カラー削除時の不要なログを削除

### 修正
- 元のテクスチャが Texture2D でない場合に発生する不正なキャストエラーを修正
- アバターの変換に失敗したときに不要なエラーログが出力される問題を修正

## [1.11.0] - 2023-05-22

### 追加
- PhysBone に Network ID を割り当てる機能と説明を追加 (VRCSDK 3.2.0 以降)
- アバターのビルド時に Missing 状態のコンポーネントを削除する機能を追加
- アップデート通知に変更履歴ボタンを追加
- アバターの変換に失敗したときのエラーメッセージにスタックトレースを追加

### 変更
- VRCSDK2, レガシーVRCSDK3, Unity 2018 のサポートを終了
- 変換時にアバター内の Prefab を Unpack (解凍) しないように変更
- 変換時に Missing 状態のコンポーネントを削除しないように変更
- Missing 状態のコンポーネントがある場合の警告メッセージを変更
- **Auto Remove Vertex Colors** (頂点カラーを自動的に削除する) 設定を `ProjectSettings/VRCQuestToolsSettings.json` に保存するように変更

### 修正
- 変換された BlendTree に反映されないパラメーターがある問題を修正

## [1.10.1] - 2023-03-28

### 修正
- lilToon の発光テクスチャの透明度が変換後のテクスチャに反映されない問題を修正
- マテリアル名やアニメーション名に "/" が含まれていると変換に失敗する問題を修正

## [1.10.0] - 2023-03-04

### 追加
- (VRCSDK3) 「メッシュから頂点カラーを削除」オプションを変換ウィンドウに追加
- 頂点カラーの削除/復元を制御するための VertexColorRemover コンポーネントを追加

### 変更
- (VRCSDK3) 頂点カラーが自動的に削除されないように変更。代わりに VertexColorRemover コンポーネントを使用する

## [1.9.1] - 2023-02-13

### 変更
- 変換後のアバターの Prefab を作成しないように変更

### 修正
- アバター内で参照されているオブジェクトを削除したときに Prefab を予期せず参照して発生する以下の問題を修正
  - PhysBones Collision Check Count が実際より多く算出される問題
  - Modular Avatar 使用時にビルドサイズが増加する問題

## [1.9.0] - 2023-01-21

### 追加
- VRChat Package Manager のサポート
- [実験的機能] VPMリポジトリ: https://kurotu.github.io/VRCQuestTools/index.json
- Modular Avatar の Merge Animator に対して Animator Controller を変換する機能を追加
- lilToon 1.3.7 の Emission 合成モードの対応を追加
- Android ビルドターゲットで Quest 用にアップロードできないアバターがある場合の警告に判定理由を追加

### 変更
- パッケージとして使用している場合にアップデート通知で Booth を開かないよう表示内容を変更
- VRCSDK を検出できない場合のメッセージを改善

## [1.8.1] - 2022-09-29
### 修正
- Cubemap を使用するマテリアルがあると変換に失敗する問題を修正

## [1.8.0] - 2022-09-24
### 追加
- Toon Lit 用にメインテクスチャの明るさを調整する機能を追加 (初期設定: 0.83)

### 変更
- テクスチャの生成処理を Magick.NET からシェーダーと RenderTexture による処理に一新
    - unitypackage のサイズが縮小
    - OS・CPU への依存が低減
    - macOS でプラグインを許可する操作が不要

### 修正
- テクスチャを生成するときエミッション色が暗く合成されることがある問題を修正

## [1.7.0] - 2022-09-06
### 追加
- アップデート通知で「スキップ」ボタンにより次のアップデートまで通知を表示しない機能を追加
- 高度な変換設定：変換時に Animator Override Controller を使用して Quest 用の Animator Controller を生成する機能を追加

### 変更
- マテリアルを変更しない変換不要なアニメーションをそのまま使用するように変更
- 警告メッセージの軽微な変更

### 修正
- アップデートがあっても通知が表示されないことがある問題を修正

## [1.6.6] - 2022-08-31
### 修正
- VCC 1.0.0 のプロジェクトで VPM 版 VRCSDK3 付属のアニメーションを変換してしまう問題を修正

## [1.6.5] - 2022-07-18
### 修正
- Blend Tree のモーションが空の場合に Animator Controller の変換に失敗する問題を修正

## [1.6.4] - 2022-07-12
### 修正
- lilToon のバージョン検出に失敗して変換ボタンが表示されなくなることがある問題を修正

## [1.6.3] - 2022-06-27
### 修正
- lilToon 1.3.0 のマテリアルを変換するとテクスチャ生成に失敗する問題を修正

## [1.6.2] - 2022-05-29
### 変更
- Blend Tree が Animator Controller から独立したアセットの場合、変換結果を `BlendTrees` フォルダに保存するように変更

### 修正
- Blend Tree を変換するとき誤って変換前の Blend Tree を上書きし、変換を繰り返すとアバターの変換に失敗する問題を修正
- VRCSDK 付属の Blend Tree を変換してしまう問題を修正
- VPM 版 VRCSDK3 付属のアニメーションを変換してしまう問題を修正

## [1.6.1] - 2022-05-05
### 修正
- lilToon マテリアルで PNG/JPG 以外のテクスチャを使用しているとテクスチャが正しく変換されない問題を修正
- lilToon マテリアルの変換でテクスチャの Wrap Mode が反映されない問題を修正
- Animator Controller の Synced Layer が変換されない問題を修正

## [1.6.0] - 2022-04-29
### 追加
- PhysBones を削除するときに PC/Quest 間の同期に問題が出る選択をしている場合の警告を表示

## [1.5.0] - 2022-04-25
### 追加
- 「Remove PhysBones」メニューを追加
- PhysBones の変換に関する説明文に VRCSDK による自動変換を実行するボタンを追加
- アバターの変換後に Avatar Dynamics のコンポーネント数が Poor 制限値を超えている場合「Remove PhysBones」を実行する機能を追加
- Android ビルドターゲットでアバターにエラーがあるとする条件に Avatar Dynamics のコンポーネント数を追加

## [1.4.0] - 2022-04-22
### 追加
- PhysBones の変換に関する説明文を追加
- 互換性のないアップデートがある場合の警告文を追加

## [1.3.0] - 2022-04-09
### 追加
- lilToon のマテリアルからのテクスチャ生成に対応

## [1.2.1] - 2022-03-01
### 修正
- 不要なテストコードを削除

## [1.2.0] - 2022-03-01
### 追加
- Scene 内のアバターをアップロードできない状態になっている場合に警告を表示
    - アバターに Missing 状態のコンポーネントがある場合
    - Android ビルドターゲットで Quest 用にアップロードできないアバターがある場合

## [1.1.2] - 2021-11-25
### 修正
- マテリアルを変更する BlendTree が変換されない問題を修正

## [1.1.1] - 2021-11-17
### 修正
- lilToon でセットアップしたアバターを変換すると真っ白になることがある問題を修正

## [1.1.0] - 2021-10-23
### 追加
- ArxCharacterShaders のマテリアルからのテクスチャ生成に対応
- VRCQuestTools Extra https://kurotu.booth.pm/items/3375621 との連携機能を追加

## [1.0.2] - 2021-09-25
### 変更
- マテリアルの種類を判定する際にシェーダー名の大文字と小文字を区別しないように変更
### 修正
- 単一アセット内に含まれる複数のアニメーションやマテリアルを変換するとエラーになる問題を修正

## [1.0.0] - 2021-09-10
### 追加
- 更新通知ウィンドウを追加
### 変更
- コードを大幅に書き換え (旧バージョンを削除してからのインポートを推奨)
- 生成したテクスチャの Streaming Mip Maps を有効化するように変更
### 修正
- TGA形式のテクスチャを使用しているとテクスチャが上下反転して生成されることがある問題を修正
- Sunao Shaderのマテリアルでメインテクスチャを指定していない場合にテクスチャが正しく生成されない問題を修正
- Edit Mode と Play Mode を切り替えるとウィンドウの内容の一部がリセットされる問題を修正

## [0.7.0] - 2021-08-08
### 追加
- Sunao Shaderのマテリアルからのテクスチャ生成に対応

## [0.6.0] - 2021-08-05
### 削除
- Unity 2019 では Local Cache Server が不要なため推奨設定から削除
### 修正
- (macOS) 内部プラグインを Unity 2019 用にリネーム

## [0.5.2] - 2021-08-04
### 追加
- 「Quest用テクスチャのみ更新」にエラーダイアログを追加
### 修正
- Resources/unity_builtin_extra を使用するマテリアルの変換に失敗する問題を修正
- Standard シェーダーが動作未確認のシェーダーとして警告される問題を修正

## [0.5.1] - 2021-06-30
### 修正
- VRCSDK2のプロジェクトでコンパイルエラーになる問題を修正

## [0.5.0] - 2021-06-27
### 追加
- (Avatars 3.0) マテリアル変更アニメーションがある場合に Animator Controller とアニメーションを変換する機能を追加 (Thanks zin3)
- 表示言語の選択機能を追加 (日/英)
- 「Remove Missing Components」「Remove Unsupported Components」の実行を確認するダイアログを追加
- 動作未確認のシェーダーに対してのテクスチャ生成について警告を追加
### 変更
- UI の細かな改善
### 修正
- 空のマテリアルスロットがある場合にテクスチャのみの更新に失敗する問題を修正

## [0.4.1] - 2021-05-23
### 変更
- マテリアル変換時のエラーをより詳細に表示するよう変更
- 「Quest用テクスチャを更新」ボタンの配置を変更

## [0.4.0] - 2021-04-01
### 追加
- 「Metallic Smoothness Map」を追加
- 変換済みアバターのテクスチャのみを更新する機能を追加
- 変換後のアバターで削除されるコンポーネントについて警告を追加
### 変更
- 変換後のマテリアルから不要なプロパティを削除するように変更
- 変換後のマテリアルでは GPU インスタンシングを有効にするように変更

## [0.3.0] - 2021-02-06
### 追加
- 更新確認機能を追加
- macOS, Linux で「Quest用のテクスチャを生成する」機能が動作するように変更
- アバターの変換完了時にダイアログで通知するように変更
### 変更
- 変換済みアバターのデフォルトの保存先を Assets/KRT/QuestAvatars に変更
- 生成したマテリアルとテクスチャをそれぞれ Materials, Textures フォルダに保存するように変更
- 「Remove Missing Components」で Unpack Prefab が不要な場合には実行しないように変更
- メニューの表示順を調整

## [0.2.1] - 2021-01-24
### 修正
- プロジェクトを開いたときに「Auto Remove Vertex Colors」のチェックが反映されない問題を修正

## [0.2.0] - 2020-11-29
### 追加
- 「Remove Missing Components」「Remove Unsupported Components」を追加
- オブジェクトの右クリックメニューに VRCQuestTools を追加
- Quest 用テクスチャを生成する際にテクスチャサイズを制限する機能を追加
### 変更
- メニューの実装を整理

## [0.1.2] - 2020-11-09
### 修正
- Missing になっている DynamicBone を含むアバターを変換すると Unity がクラッシュする問題を修正

## [0.1.1] - 2020-10-28
### 変更
- メッセージの内容を一部変更
### 修正
- RenderTexture を使用するマテリアルがあると変換が停止する問題を修正

## [0.1.0] - 2020-10-10
- 公開

[Unreleased]: https://github.com/kurotu/VRCQuestTools/compare/v1.14.0...HEAD
[1.14.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.5...v1.14.0
[1.13.5]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.4...v1.13.5
[1.13.4]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.3...v1.13.4
[1.13.3]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.2...v1.13.3
[1.13.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.1...v1.13.2
[1.13.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.0...v1.13.1
[1.13.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.12.1...v1.13.0
[1.12.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.12.0...v1.12.1
[1.12.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.11.0...v1.12.0
[1.11.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.10.1...v1.11.0
[1.10.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.10.0...v1.10.1
[1.10.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.9.1...v1.10.0
[1.9.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.9.0...v1.9.1
[1.9.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.8.1...v1.9.0
[1.8.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.8.0...v1.8.1
[1.8.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.7.0...v1.8.0
[1.7.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.6...v1.7.0
[1.6.6]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.5...v1.6.6
[1.6.5]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.4...v1.6.5
[1.6.4]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.3...v1.6.4
[1.6.3]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.2...v1.6.3
[1.6.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.1...v1.6.2
[1.6.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.0...v1.6.1
[1.6.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.2...v1.6.0
[1.5.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.1...v1.5.2
[1.5.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.4.1...v1.5.0
[1.4.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.4.0...v1.4.1
[1.4.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.2.1...v1.3.0
[1.2.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.2...v1.2.0
[1.1.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.7.0...v1.0.0
[0.7.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.2...v0.6.0
[0.5.2]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.1...v0.5.2
[0.5.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.4.1...v0.5.0
[0.4.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.2.1...v0.3.0
[0.2.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.2...v0.2.0
[0.1.2]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/kurotu/VRCQuestTools/releases/tag/v0.1.0
