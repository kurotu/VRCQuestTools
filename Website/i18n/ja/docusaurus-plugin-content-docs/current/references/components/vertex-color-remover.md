---
sidebar_position: 2
---

# Vertex Color Remover

アタッチされたGameObjectとその子のMesh RendererまたはSkinned Mesh Rendererに関連付けられているメッシュから頂点カラーをすべて削除します。
これにより、Toon Litシェーダーを使用すると一部のアバターでメインテクスチャが正しく適用されない問題が修正されます。

例:
- メッシュが真っ黒になる。
- メッシュに別の色が重ねて表示される。

![VertexColorRemover](/img/VertexColorRemover.png)

## ボタンとプロパティ

### 頂点カラーを削除
`OnReset()` および `OnValidate()` で頂点カラーを削除します。
**Active** チェックボックスが有効になります。

### 頂点カラーを復元
メッシュアセットを再読み込みすることで頂点カラーを復元します。
**Active** チェックボックスが無効になります。

### Active
コンポーネントが頂点カラーを削除するかどうかを示します。

### Include Children
コンポーネントが子のMesh RendererまたはSkinned Mesh Rendererから頂点カラーを削除するかどうかを示します。
