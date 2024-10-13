:::warning
このコンポーネントは実験的機能です。
:::

# Mesh Flipper

ポリゴンの反転したメッシュまたは両面になったメッシュを生成します。
VRChatのモバイルシェーダーは裏面を描画できないため、このコンポーネントは裏面を描画するために使用できます。

:::info
このコンポーネントには Non-Destructive Modular Framework (NDMF) が必要です。
:::

## プロパティ

### メッシュの向き

- `反転` - メッシュのポリゴンを反転します。
- `両面` - メッシュのポリゴンを複製して両面にします。

### PCで有効

ビルドターゲットがPCの場合、このコンポーネントを有効にします。

### Androidで有効

ビルドターゲットがAndroidの場合、このコンポーネントを有効にします。

## NDMF

VRCQuestToolsプラグインによって以下の処理を実行します。

### Transforming Phase

アタッチされたコンポーネントのMeshFilterまたはSkinnedMeshRendererに対して新しいメッシュを生成します。

この処理は NDMF Mantis LOD Editor の後に実行されます。
ただしlilNDMFMeshSimplifierが存在する場合は、lilNDMFMeshSimplifierの後・AAO: Avatar Optimizerの前に遅延されます。
