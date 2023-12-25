---
sidebar_position: 2
---

# Vertex Color Remover

Remove vertex colors from meshes which are associated with the attached GameObject and its children's mesh renderers or skinned mesh renderers.

This fixes an issue where main textures are not correctly applied in some avatars when using Toon Lit shader.

Example:
- Mesh becomes black.
- Mesh is displayed with another color.

![VertexColorRemover](/img/VertexColorRemover.png)

:::info
Since vertex colors are removed from the shared mesh, it also affects other avatars that use the same mesh asset.
:::

## Buttons and Properties

### Remove Vertex Color
Activate the component to remove vertex colors.

### Restore Vertex Color
Deactivate the component.
Then restore vertex colors by reloading mesh assets.

### Include Children
Select whether the component removes vertex colors from children's mesh renderers or skinned mesh renderers or not.
