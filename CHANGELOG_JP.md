# 変更履歴

このプロジェクトのすべての重要な変更はこのファイルに記録されます。

このフォーマットは [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) に基づいており、このプロジェクトは [Semantic Versioning](https://semver.org/spec/v2.0.0.html) に準拠しています。

## [Unreleased]

### 追加
- [NDMF] `VQT Mesh Flipper` にマスクテクスチャによって動作範囲を制御する機能を追加。
- [NDMF] (実験的機能) `VQT Material Swap` コンポーネントによる Android ビルド時のマテリアル置換設定を追加。 (by @Amoenus)
- [NDMF] `VQT Menu Icons Resizer` によるメニューアイコンのサイズ変更機能を追加。
- [NDMF] 追加のマテリアル変換設定の対象となるマテリアルが他のプラグインによって変更された場合に追跡する機能を追加。
- マテリアル置換設定で非対応マテリアルを指定したときのエラーを追加。
- `VQT Avatar Converter Settings` に余分なマテリアルスロットを削除する機能を追加。
- `VQT Avatar Converter Settings` にメニューアイコンを圧縮する機能を追加。
- lilToon のカスタムシェーダーを使用するマテリアルの変換に対応。(通常の lilToon と同様の処理)

### 変更
- テクスチャの生成処理を高速化。
- [NDMF] Transforming Phase で `VQT Mesh Flipper` が `NDMF Mantis LOD Editor` より先に動作するように変更。

## [2.6.2] - 2025-01-21

### 修正
- lilToon FakeShadowのマテリアルの変換に失敗する問題を修正。

## [2.6.1] - 2025-01-19

### 追加
- lilToon のベイク処理でマテリアルのプロパティの型に不整合がある場合に追加情報を表示する機能を追加。

### 修正
- GameObject を選択していない状態でヒエラルキーを右クリックするとエラーが発生する問題を修正。
- 入れ子になった Blend Tree が正しく変換されない問題を修正。
- 翻訳ファイルの誤りを修正。 (by @Amoenus)

## [2.6.0] - 2024-12-14

### 追加
- Local-Only Contact Receiver に対応。
- Prefab ステージに関する警告にシーンに戻るためのボタンを追加
- [NDMF] (実験的機能) `VQT Mesh Flipper` コンポーネントによるメッシュの向きを反転する機能を追加。
- [NDMF] アバターの右クリックメニューに `[NDMF] Manual Bake with Android Settings` メニューを追加。
- [NDMF] `VQT Avatar Builder` にアップロード対象のアバターを選択する機能を追加。
- [NDMF] `MA Convert Constraints` コンポーネントと非破壊変換を提案するメッセージとダイアログを追加。

### 変更
- [NDMF] テクスチャフォーマットのチェックを TexTransTool の後で実行するように変更。
- [NDMF] 非破壊変換の優先度が高くなるように `VQT Avatar Converter Settings` のインスペクターを変更。
- [NDMF] Transforming Phase でのアバターの変換を lilycalInventory の後で実行するように変更。

### 修正
- 未翻訳の文言に対するフォールバックが動作していない問題を修正。
- `VQT Avatar Converter Settings` の初期状態で非アクティブの Avatar Dynamics コンポーネントが選択されていない問題を修正。
- [NDMF] ビルドターゲットがAndroidの場合、変換していないアバターであっても非対応コンポーネントを削除する問題を修正。

## [2.5.5] - 2024-11-16

### 修正
- [NDMF] 作成したばかりの Pipeline Manager を持つアバターがあるとき Play Mode で ArgumentNullException が発生する問題を修正。
- [NDMF] 他のプラグインが生成したテクスチャの読み込みに失敗することがある問題を修正。

## [2.5.4] - 2024-10-09

### 変更
- Avatar Dynamics Selector と PhysBones Remover の UI レイアウトを統一

### 修正
- Avatar Dynamics Selector ウィンドウに Root Transform の列が表示されていない問題を修正。
- [NDMF] アバターに Expression Menu がないときテクスチャフォーマットのチェックで発生するエラーを修正。

## [2.5.3] - 2024-10-06

### 変更
- Animator Controller の複製処理をリライト。
  - 複雑な Animator Controller の複製が改善されます。

### 修正
- GoGo Loco を使用したFXレイヤーの変換に失敗する問題を修正。

## [2.5.2] - 2024-09-25

### 修正
- [NDMF] 再帰的な Expressions Menu を使用しているとスタックオーバーフローが発生する問題を修正。

## [2.5.1] - 2024-09-23

### 変更
- [NDMF] テクスチャフォーマットのチェックを高速化。

### 修正
- オブジェクト参照の正しくないAnimator Controllerがあると変換元のAnimator Controllerを変更してしまう問題を修正。
- [NDMF] サブステートマシンを持つAnimator Controllerの変換に失敗することがある問題を修正。

## [2.5.0] - 2024-09-07

### 追加
- `VQT Network ID Assigner` が NDMF なしで動作する機能を追加
- `VQT Avatar Converter Settings` に `NDMF変換フェーズ` 設定を追加
    - Transforming
    - Optimizing
- .po ファイルをベースにしたローカライズ機構を追加
- ロシア語 (Русский) 翻訳 (by @CoderCoV)
- [NDMF] `VQT Avatar Converter Settings` のあるアバターに `VQT Network ID Assigner` がないときに警告を表示

### 変更
- `Convert Avatar for Android` ウィンドウで変換設定を始めるときに `VQT Network ID Assigner` をアバターに追加するように変更
- [NDMF] アバターの変換処理の実行順のデフォルト設定を 2.4.3 より前と同じに変更 (Transforming)

### 削除
- ネットワークIDの未割り当てに関する警告を削除

## [2.4.3] - 2024-08-25

### 変更
- テクスチャ生成時に PNG および JPEG 以外の画像に対しては Unity がインポートしたテクスチャを使用するように変更
- [NDMF] TexTransTool との相互運用性のために Optimizing Phase でアバターを変換するように変更

### 修正
- [NDMF] Unity の起動時に VQT Avatar Builder がエラーを出すことがある問題を修正
- [NDMF] マテリアル置換で複数のマテリアルを同じマテリアルに置換するとエラーになる問題を修正
- [NDMF] マテリアルに PNG および JPEG テクスチャ以外を使用しているとテクスチャ生成エラーになる問題を修正
- [NDMF] スクリプトをリロードするとき NDMF 1.5.0 のプレビュー設定用ウィンドウで例外が発生する問題を修正 (by @ReinaS-64892)

## [2.4.2] - 2024-08-16

### 削除
- `VQT Avatar Converter Settings` の Unity Constraints を VRChat Constraints へ変換するボタンを削除

## [2.4.1] - 2024-08-15

### 追加
- Prefab モードではアバターを変換できないことに対してのエラーメッセージを追加

### 修正
- [NDMF] 現在のプラットフォームに非対応のテクスチャフォーマットを使用しているとき、既知のテクスチャフォーマットが未知のテクスチャフォーマットとして報告される問題を修正

## [2.4.0] - 2024-08-14

### 追加
- iOS プラットフォームのサポート (Android と同様)
- `VQT Avatar Converter Settings` に Unity Constraints を VRChat Constraints へ変換することを提案する警告を追加
- [NDMF] VQT Avatar Builder でフォールバックアバターを設定する機能を追加
- [NDMF] NDMFコンソールにロゴを追加

### 変更
- Modular Avatar 1.9.0 以降を使用している場合 `MA Visible Head Accessory` と `MA World Fixed Object` コンポーネントを削除しないように変更
- [NDMF] 未知のテクスチャフォーマットが使用されているときにエラーではなく警告を表示するように変更
- [NDMF] 非対応のプラットフォームではテクスチャフォーマットをチェックしないように変更

### 修正
- VRChat SDK 3.6.2-constraints.3 以降でコンパイルエラーになる問題を修正

## [2.3.5] - 2024-07-27

### 修正
- Final IK が存在するときに `VQT Avatar Converter Settings` が正常に動作しない問題を修正

## [2.3.4] - 2024-07-17

### 修正
- 非対応コンポーネントを削除するとき `Audio Source` が削除されないことがある問題を修正

## [2.3.3] - 2024-06-03

### 変更
- lilToon マテリアルの変換時に発生したエラーの表示を改善

### 修正
- マテリアルを変更するアニメーションが変換されない場合がある問題を修正

## [2.3.2] - 2024-05-27

### 修正
- [NDMF] 複数のシーンをヒエラルキーにロードしているとVQT Avatar Builderがアクティブなアバターを検出できないことがある問題を修正
- 複数のシーンをヒエラルキーにロードしているとアバターの自動バリデーションが全てのアバターに対して動作していない問題を修正

## [2.3.1] - 2024-05-09

### 修正
- [NDMF] Animator Controllerの複製時にサブステートマシンへのEntry Transitionの複製に失敗する問題を修正。
- サブステートマシン内のマテリアル変更アニメーションが変換されない問題を修正。

## [2.3.0] - 2024-05-05

### 追加
- [NDMF] `Show Avatar Builder for Android` メニューを追加して NDMF で Android ビルドターゲット用にアバターをアップロードする機能を追加
- [NDMF] `VQT Avatar Converter Settings` コンポーネントによる非破壊的なアバター変換機能を追加
- [NDMF] `VQT Platform Target Settings` コンポーネントを追加して `VQT Platform Component Remover` および `VQT Platform GameObject Remover` コンポーネントの対象プラットフォームを指定する機能を追加
- [NDMF] 非対応のテクスチャフォーマットを使用しているときに警告を表示
- [NDMF] `VQT Network ID Assigner` コンポーネントにより Network ID を割り当てる機能を追加

### 変更
- [NDMF] 頂点カラーを削除するときにメッシュを複製するように変更。元のメッシュは頂点カラーを維持
- `VQT Platform Component Remover` と `VQT Platform GameObject Remover` コンポーネントのチェックボックスを反転し、維持する場合にチェックを入れるように変更

### 削除
- [NDMF] `VQT Platform Component Remover` と `VQT Platform GameObject Remover` コンポーネントからビルドターゲットパラメータを削除

### 修正
- 小さいテクスチャが 4x4 ではなく 2x2 で作成され適切に圧縮できない問題を修正

## [2.2.2] - 2024-04-15

### 修正
- lilToon の色調補正マスクが変換後のテクスチャに反映されない問題を修正

## [2.2.1] - 2024-03-29

### 追加
- `VQT Platform Component Remover`, `VQT Platform GameObject Remover` `VQT Vertex Color Remover` に説明文を追加

### 修正
- アバターのオブジェクト名の末尾にドットが含まれていると変換に失敗する問題を修正

## [2.2.0] - 2024-02-09

### 追加
- ビルドプラットフォームによって GameObject を削除する `VQT Platform GameObject Remover` コンポーネントを追加 (要NDMF)
- NDMFの必要なコンポーネントを使用しているがプロジェクトにNDMFがインストールされていないときの警告を追加
- コンポーネントにアイコンを追加 (Unity 2022)

### 修正
- アバターのオブジェクト名の先頭や末尾が半角スペースのとき変換に失敗する問題を修正

## [2.1.2] - 2024-01-29

### 修正
- NDMF アセンブリのビルドに失敗することがある問題を修正 (by anatawa12)
- NDMF 1.3.0 でコンパイルエラーになる問題を修正 (by anatawa12)

## [2.1.1] - 2024-01-11

### 修正
- NDMF 1.3.0 未満を使用しているとコンパイルエラーになる問題を修正

## [2.1.0] - 2024-01-11

### 追加
- ビルドプラットフォームによってコンポーネントを削除する `VQT Platform Component Remover` コンポーネントを追加 (要NDMF)
- MatCap Lit 変換設定に MatCap テクスチャが割り当てられていないときの警告を追加
- 各コンポーネントのヘルプ URL を追加

## [2.0.1] - 2024-01-05

### 修正
- VRChat SDK 3.3.0 未満のプロジェクトに導入できる問題を修正

## [2.0.0] - 2024-01-05

### 追加
- アバターに変換設定を保存する `VQT Avatar Converter Settings` コンポーネントを追加
- アバターがVRCQuestToolsで変換されたことを示す `VQT Converted Avatar` コンポーネントを追加
- アバターのビルド中にNDMFで生成された非対応コンポーネントを削除する機能を追加
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
- アバターの変換時に非対応のModular Avatarのコンポーネントを削除する機能を追加

### 変更
- VRChat SDK 3.3.0 以降が必要
- unitypackage のインポートパスを `Assets/KRT/VRCQuestTools` から `Packages/com.github.kurotu.vrc-quest-tools` に変更
- UPM のインポート URL を `https://github.com/kurotu/VRCQuestTools.git?path=Packages/com.github.kurotu.vrc-quest-tools` に変更
- 変換後のアバターの保存先を `Assets/KRT/QuestAvatars` から `Assets/VRCQuestToolsOutput` に変更
- 変換後のアバターのオブジェクト名に付与する接尾語を ` (Quest)` から ` (Android)` に変更
- ビルド時の独自コンポーネントの削除処理を Anatawa12's Avatar Optimizer による最適化の前に実行するように変更
- 更新通知をシーンビューの代わりに Inspector に表示するように変更
- 最新バージョンの確認処理を1日1回に変更
- **VRCQuestTools** メニューを **Tools** メニュー内に移動
- メニュー項目の配置を変更
- **Remove Unsupported Components** メニューで削除対象のコンポーネントを一覧表示するように変更

### 削除
- **Check for Updates** メニューを削除。代わりに VCC または Inspector を使用
- シーンから頂点カラーを検出して自動削除する機能を削除。代わりに **Remove All Vertex Colors** メニューを使用

### 修正
- Network ID に割り当てられていた GameObject が実際には存在しないとき変換に失敗する問題を修正
- テクスチャ生成時にリニアテクスチャを sRGB で読み込んでいた問題を修正
- VRC Spatial Audio Source が非対応コンポーネントとして検出されない問題を修正
- パフォーマンスランクの推定に失敗する可能性がある問題を対策

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
