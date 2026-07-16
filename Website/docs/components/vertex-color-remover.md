---
slug: /references/components/vertex-color-remover
---

# Vertex Color Remover

A component which removes vertex colors from the meshes associated with the GameObject.
Some PC avatars have vertex colors, and texture colors may not be displayed correctly with Mobile shaders.

Unlike other components, it removes vertex colors from the meshes in the scene as soon as it is added, not at build time.

![VQT Vertex Color Remover Inspector](/img/vertex-color-remover.png)

## Settings

| Setting | Description |
|---|---|
| Include Children | Also removes vertex colors from the meshes of child objects. |

Press the "Restore Vertex Color" button to re-import the meshes and restore the vertex colors.

## Notes

- When "Remove Vertex Color from Meshes" of [VQT Avatar Converter Settings](./avatar-converter-settings.md) is enabled, vertex colors are handled during conversion and you don't need this component separately.
- If your PC avatar uses a special shader which requires vertex colors, disable this component.
