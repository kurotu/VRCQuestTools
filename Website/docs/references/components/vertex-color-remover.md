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

## Buttons and Properties

### Remove Vertex Color
Remove vertex colors on `OnReset()` or `OnValidate()`.
**Active** checkbox is enabled.

### Restore Vertex Color
Restore vertex colors by reloading mesh assets. **Active** checkbox is disabled.

### Active
Indicates whether the component removes vertex colors or not.

### Include Children
Indicates whether the component removes vertex colors from children's mesh renderers or skinned mesh renderers or not.
