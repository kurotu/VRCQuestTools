---
slug: /references/components
---

# Components

VRCQuestTools provides Unity components to attach to avatars.
You can add them from the "VRCQuestTools" category with the "Add Component" button in the Inspector.

All components work only in the Unity editor.
They are not included in the uploaded avatar, so they don't affect the avatar's performance.

## Component List

| Component | Purpose |
|---|---|
| [VQT Avatar Converter Settings](./avatar-converter-settings.md) | Settings to convert the avatar for Mobile |
| [VQT Converted Avatar](./converted-avatar.md) | Marker for converted avatars |
| [VQT Fallback Avatar](./fallback-avatar.md) | Sets the avatar as a fallback avatar on upload |
| [VQT Material Conversion Settings](./material-conversion-settings.md) | Material conversion settings (independent from avatar conversion) |
| [VQT Material Swap](./material-swap.md) | Replaces materials with other materials in Mobile builds |
| [VQT Menu Icon Resizer](./menu-icon-resizer.md) | Resizes expressions menu icons |
| [VQT Mesh Flipper](./mesh-flipper.md) | Makes a mesh double-sided or flips its faces |
| [VQT Network ID Assigner](./network-id-assigner.md) | Assigns network IDs to PhysBones and other components |
| [VQT Platform Component Remover](./platform-component-remover.md) | Removes components depending on the platform |
| [VQT Platform GameObject Remover](./platform-gameobject-remover.md) | Removes GameObjects depending on the platform |
| [VQT Platform Target Settings](./platform-target-settings.md) | Forces the platform detection at build time |
| [VQT Vertex Color Remover](./vertex-color-remover.md) | Removes vertex colors from meshes |

## Components Requiring NDMF

The following components use Non-Destructive Modular Framework (NDMF) for their build-time processing.

- VQT Material Conversion Settings
- VQT Material Swap
- VQT Menu Icon Resizer
- VQT Mesh Flipper
- VQT Platform Component Remover
- VQT Platform GameObject Remover
- VQT Platform Target Settings

When the project doesn't have NDMF, these components don't work and a warning appears in the Inspector.
To install NDMF, add the repository with the "Download" button at the top of the [Modular Avatar](https://modular-avatar.nadena.dev/) website.

## About Build Targets

Components with settings like "Keep on PC" or "Enable on Mobile" switch their behavior based on the target platform at build time.
Normally, Unity's target platform is used as is (PC for Windows, Mobile for Android and iOS).
By adding [VQT Platform Target Settings](./platform-target-settings.md) to the avatar, you can force this detection.
