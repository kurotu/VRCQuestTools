# Mesh Flipper

:::warning
このコンポーネントは実験的機能です。
:::

ポリゴンの反転したメッシュまたは両面になったメッシュを生成します。
VRChatのモバイルシェーダーは裏面を描画できないため、このコンポーネントは裏面を描画するために使用できます。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### メッシュの向き

- `反転` - メッシュのポリゴンを反転します。
- `両面` - メッシュのポリゴンを複製して両面にします。

:::warning
`両面` を選択すると、メッシュのポリゴンが複製されるためポリゴン数が2倍になります。
:::

### マスクを使用する

#### マスクテクスチャ

メッシュを生成する領域をテクスチャで指定します。
ポリゴンの3つの頂点全てがマスクテクスチャの対象になっているとき、そのポリゴンを処理対象にします。

#### マスクモード

マスクテクスチャの白い領域と黒い領域のどちらをメッシュの生成対象にするかを選択します。

### NDMFフェーズ

NDMFのビルド処理中にどのタイミングでメッシュを生成するか選択します。

- `After Polygon Reduction`: 他のポリゴン数削減ツールの前
- `Before Polygon Reduction`: 他のポリゴン数削減ツールの後

以下のポリゴン数削減ツールに対して処理順序を考慮します。

- [NDMF Mantis LOD Editor](https://hitsub.booth.pm/items/5409262)
- [lilNDMFMeshSimplifier](https://github.com/lilxyzw/lilNDMFMeshSimplifier)
- [Meshia Mesh Simplification](https://github.com/RamType0/Meshia.MeshSimplification)

### PCで有効

ビルドターゲットがPCの場合、このコンポーネントを有効にします。

### Androidで有効

ビルドターゲットがAndroidの場合、このコンポーネントを有効にします。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Transforming Phase

`NDMFフェーズ` が `Before Polygon Reduction` であるとき、アタッチされたオブジェクトのMeshFilterまたはSkinnedMeshRendererに対して新しいメッシュを生成します。

### Optimizing Phase

``NDMFフェーズ` が `After Polygon Reduction` であるとき、アタッチされたオブジェクトのMeshFilterまたはSkinnedMeshRendererに対して新しいメッシュを生成します。
