---
sidebar_position: 3
---

# Vertex Color Remover

アタッチされたGameObjectとその子のMesh RendererまたはSkinned Mesh Rendererに関連付けられているメッシュから頂点カラーをすべて削除します。
これにより、Toon Litシェーダーを使用すると一部のアバターでメインテクスチャが正しく適用されない問題が修正されます。

例:
- メッシュが真っ黒になる。
- メッシュに別の色が重ねて表示される。

![VertexColorRemover](/img/VertexColorRemover.png)

:::info
Shared Meshから頂点カラーを削除するため、同じメッシュアセットを使用する他のアバターにも影響します。
:::

## ボタンとプロパティ

### 頂点カラーを削除
コンポーネントをアクティブにして頂点カラーを削除します。

### 頂点カラーを復元
コンポーネントを非アクティブに変更します。
メッシュアセットを再読み込みすることで頂点カラーを復元します。

### Include Children
子のMesh RendererまたはSkinned Mesh Rendererからも頂点カラーを削除するかどうかを選択します。
