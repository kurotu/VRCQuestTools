---
sidebar_position: 6
---

# Remove All Vertex Colors

Attach [VertexColorRemover](../components/vertex-color-remover) component to selected GameObject and remove all vertex colors from meshes which are associated with the selected GameObject and its children's mesh renderers or skinned mesh renderers.

This fixes an issue where main textures are not correctly applied in some avatars when using Toon Lit shader.

Example:
- Mesh becomes black.
- Mesh is displayed with another color.

![VertexColorRemover](/img/VertexColorRemover.png)
