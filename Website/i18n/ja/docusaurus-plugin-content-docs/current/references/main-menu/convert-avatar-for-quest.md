---
sidebar_position: 1
---

# Convert Avatar for Quest

VRChatアバターをQuest（Android）向けにアップロード可能に変換します。

![Convert Avatar for Quest](/img/convert_avatar_for_quest.png)

この機能は以下の手順でアバターを変換します。
1. Quest向けにテクスチャを生成します。
2. `VRChat/Mobile/Toon Lit`マテリアルを作成し、1で生成したテクスチャを設定します。
3. マテリアルを変更するアニメーションクリップを2で作成したマテリアルを使用するように変換します。
4. 変換したアニメーションクリップを使用するようにAnimator Controllerを変換し、Animator Override Controllerを解決します。
5. 元のアバターを複製し、変換したアセットをセットします。
6. Constraintなどのサポートされていないコンポーネントを削除します。
7. シーン内の元のアバターを非アクティブにします。

## 設定項目

### アバター

変換するアバターを設定します。アバターのGameObjectには`VRCAvatarDescriptor`コンポーネントが必要です。

### ネットワークID

アバターにネットワークIDを割り当てます。ネットワークIDはPCとQuestで異なるPhysBone構造を持っていてもPhysBoneを同期するために使用されます。
ネットワークIDを割り当てた後、PC向けにアバターを再アップロードしてください。変換されたアバターは元のアバターと同じネットワークIDを持ちます。

詳細は[Network ID Utility](https://creators.vrchat.com/worlds/udon/networking/network-id-utility/)を参照してください。

### DynamicBoneの変換

`DynamicBone`と`DynamicBoneCollider`コンポーネントをVRCSDKの機能で`VRCPhysBoene`と`VRCPhysBoneCollider`コンポーネントに変換します。
Questアバターでは`DynamicBone`は使用できませんが、`VRCPhysBone`は使用できます。そのため、コンポーネントを変換することで髪やスカートなどのアバターの揺れ物を保持することができます。

詳細は[Manual Dynamic Bone Conversion](https://creators.vrchat.com/avatars/avatar-dynamics/physbones/#manual-dynamic-bone-conversion)を参照してください。

### Quest用のテクスチャを生成する

アバターを変換する際にQuest用のテクスチャを生成します。
生成されたテクスチャは元のマテリアルのエミッションなどのパラメータを反映します。

サポートされているシェーダー:
- `Standard`
- `Standard (Specular setup)`
- `Unlit/` のシェーダー
- `UnityChanToonShader/` のシェーダー
- `arktoon/` のシェーダー
- `ArxCharacterShaders/` のシェーダー
- `Sunao Shader/` のシェーダー
- `lilToon`

### 最大テクスチャサイズ

生成されるテクスチャの最大サイズを選択します。

### メインテクスチャの明るさ

`Toon Lit`マテリアルは元のマテリアルよりも明るくなりがちなため、生成されるテクスチャを元のテクスチャよりも暗くします。

### Quest用テクスチャのみ更新

Quest用テクスチャを生成します。この機能は元のマテリアルを編集したときにテクスチャを更新するために使用できます。

### 保存先フォルダ

変換されたアセットを保存するフォルダを選択します。フォルダ構造は以下のようになります。

```
保存先フォルダ/
|-- AvatarName/
|   |-- Animations/
|   |-- AnimatorControllers/
|   |-- BlendTrees/
|   |-- Materials/
|   └-- Textures/
```

### メッシュから頂点カラーを削除

変換されたアバターに`VertexColorRemover`コンポーネントが追加されます。このコンポーネントはメッシュから頂点カラーを削除します。

### アニメーションオーバーライド

Animator Override Controllerによってオーバーライドされた新しいAnimator Controllerを生成します。
