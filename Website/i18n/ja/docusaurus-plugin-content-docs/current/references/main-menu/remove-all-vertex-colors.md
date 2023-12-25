---
sidebar_position: 5
---

# Remove All Vertex Colors

選択したGameObjectに[VertexColorRemover](../components/vertex-color-remover)コンポーネントを追加し、その子のMesh RendererまたはSkinned Mesh Rendererに関連付けられているメッシュから頂点カラーをすべて削除します。

これにより、Toon Litシェーダーを使用すると一部のアバターでメインテクスチャが正しく適用されない問題が修正されます。

例:
- メッシュが真っ黒になる。
- メッシュに別の色が重ねて表示される。

![VertexColorRemover](/img/VertexColorRemover.png)
