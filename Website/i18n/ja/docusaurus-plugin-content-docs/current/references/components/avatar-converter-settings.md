# Avatar Converter Settings

VRChatアバターをAndroid向けにアップロード可能に変換するための設定を保持します。

このコンポーネントはアバターのルートオブジェクトに追加して使用します。

## 基本設定

### 保存先フォルダ

変換時に生成したアセットを保存するフォルダが表示されます。

- `Assets/VRCQuestToolsOutput/<アバターのオブジェクト名>` になります。
- 変換時にアセットは上書きされます。

## マテリアル変換設定

### デフォルトのマテリアル変換設定

変換時にデフォルトで使用するマテリアル変換設定です。
マテリアルごとに個別に変換設定が必要な場合、**追加のマテリアル変換設定**を使用してください。

以下のシェーダーを使用しているマテリアルの変換に対応しています。非対応シェーダーの場合、Standardシェーダーの変換処理を適用します。

- `Standard`
- `Standard (Specular setup)`
- `Unlit/` のシェーダー
- ユニティちゃんトゥーンシェーダー 2.0 (UTS2)
- Arktoon-Shaders
- ArxCharacterShaders (AXCS)
- Sunao Shader
- lilToon
- Poiyomi Toon Shader

| マテリアル変換設定 | 説明 |
|---|---|
| Toon Lit | 変換後のマテリアルに `VRChat/Mobile/Toon Lit` シェーダーを使用します。 |
| MatCap Lit | 変換後のマテリアルに `VRChat/Mobile/MatCap Lit` シェーダーを使用します。Toon Litと同様の変換処理を適用した後、変換設定で指定したMatCapテクスチャを使用します。 |
| マテリアル置換 | 変換設定で指定したマテリアルに置き換えます。 |

| パラメーター | 説明 | 変換モード |
|---|---|---|
| Android用のテクスチャを生成する | アバターを変換する際にAndroid用のテクスチャを生成します。オフの場合、元のメインテクスチャを使用します。 | Toon Lit, MatCap Lit |
| 最大テクスチャサイズ | 生成されるテクスチャの最大サイズを選択します。 | Toon Lit, MatCap Lit |
| メインテクスチャの明るさ | 生成されるメインテクスチャの明るさを選択します。 | Toon Lit, MatCap Lit |
| ノーマルマップから影を生成する | ノーマルマップから疑似的な影を生成しテクスチャに反映します。 | Toon Lit, MatCap Lit |
| MatCapテクスチャ | MatCap Litシェーダーで使用するMatCapテクスチャを設定します。 | MatCap Lit |
| 置換マテリアル | マテリアル置換モードで使用するマテリアルを設定します。 | マテリアル置換 |

### 追加のマテリアル変換設定

マテリアルごとに個別に変換設定を行う場合に使用します。

| パラメーター | 説明 |
|---|---|
| 対象マテリアル | 変換設定を行うマテリアルを設定します。 |
| マテリアル変換設定 | 対象マテリアルに適用するマテリアル変換設定を設定します。 |

### 変換後のAndroid用テクスチャを更新

現在の設定でテクスチャ生成のみを実行します。

## Avatar Dynamics 設定

Avatar Dynamics (PhysBone, Collider, Contact) を設定します。
AndroidアバターではAvatar Dynamicsの上限がPoorであるため、変換後に残しておくコンポーネントを指定し、残りは変換時に削除します。

### Avatar Dynamics 設定

開いたウィンドウで、変換後に残しておくコンポーネントのチェックボックスを有効にします。
選択後、**適用**ボタンをクリックします。

### PhysBones, PhysBone Colliders, Contact Senders & Receivers

変換後に残しておくPhysBone, Collider, Contactの一覧です。

### 推定パフォーマンスランク

Avatar Dynamicsの推定パフォーマンスランクです。全ての項目がPoor以内になるように設定してください。

## 高度な変換設定

### アニメーションオーバーライド

Animator Override Controller で指定したアニメーションクリップを使って、変換時に新しいAnimator Controllerを作成します。

### メッシュから頂点カラーを削除

変換されたアバターに[Vertex Color Remover](./vertex-color-remover)コンポーネントを追加することで頂点カラーを削除します。

通常、このオプションをオフにする必要はありません。PC版アバターで頂点カラーを使用する特殊なシェーダーを使用している場合に、意図しない動作を防ぐためにオフにします。

### NDMF変換フェーズ

NDMFでアバターを変換するときの実行フェーズを選択します。

## 変換

アバターの変換を実行します。
概ね以下の手順でアバターを変換します。

1. Android向けにテクスチャとマテリアルを生成します。
2. マテリアルを変更するアニメーションクリップを1で作成したマテリアルを使用するように変換します。
3. 変換したアニメーションクリップを使用するようにAnimator Controllerを変換し、Animator Override Controllerを解決します。
4. 元のアバターを複製し、変換したアセットをセットします。
5. Constraintなどのサポートされていないコンポーネントを削除します。
6. [Platform Target Settings](./platform-target-settings)を追加し、ビルドターゲットをAndroidに設定します。
7. 特殊な変換処理(後述)を実行します。
8. シーン内の元のアバターを非アクティブにします。

### 特殊な変換処理

導入しているアセットに応じて、特殊な変換処理を実行します。

#### VirtualLens2

VirtualLensSettings コンポーネントの `Remote Only Mode` を `Force Enable` に設定します。

破壊的セットアップを使用している場合、`_VirtualLens_Root` および `VirtualLensOrigin` オブジェクトに `EditorOnly` タグを設定します。

## NDMF

Non-Destructive Modular Framework (NDMF) が導入されているプロジェクトではVRCQuestToolsプラグインによって以下の処理を実行します。
VRChat SDKのバリデーションを回避するために、Android向けにビルドするときはAvatar Builderウィンドウを使用してアバターをビルドしてください。

### Resolving Phase

VirtualLens2の設定をAndroid用に変更します。

### Transforming Phase

メッシュから頂点カラーを削除します。
この処理はNDMF Mantis LOD Editorの後に実行されます。

NDMF変換フェーズがTransformingであるときアバターの変換を実行します。
この処理はTexTransToolおよびModular Avatarの後で実行されます。

### Optimizing Phase

NDMF変換フェーズがOptimizingであるときアバターの変換を実行します。
この処理はTexTransToolの後、AAO: Avatar Optimizerの前に実行されます。
