# Mesh Flipper

:::warning
This component is an experimental feature.
:::

This component generates a mesh that polygons are flipped or double-sided.
VRChat's mobile shaders can't render backfaces, so this component is useful for rendering backfaces.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Mesh Direction

Select the mesh direction to generate.

- `Flipped` - Flips the mesh's polygons.
- `Double Sided` - Duplicates the mesh's polygons to make it double-sided.

:::warning
Selecting `Double Sided` will double the polygon count because the mesh's polygons are duplicated.
:::

### Enable on PC

Enable this component when the build target is PC.

### Enable on Android

Enable this component when the build target is Android.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Transforming Phase

Geneates a new mesh for attached component's MeshFilter or SkinnedMeshRenderer.

This process is performed before NDMF Mantis LOD Editor.
