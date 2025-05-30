# Mesh Flipper

This component generates a mesh that polygons are flipped or double-sided.
The most of VRChat's mobile shaders can't render backfaces, so this component is useful for rendering backfaces.

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

### Use Mask

#### Mask Texture

Select a texture to specify the area to generate the mesh.
Polygons that their three vertices are all specified by the mask texture are processed.

#### Mask Mode

Select whether to generate the mesh in the white or black area of the mask texture.

### NDMF Phase

Select when to generate the mesh during the NDMF build process.

- `After Polygon Reduction`: Before other polygon reduction tools
- `Before Polygon Reduction`: After other polygon reduction tools

Following polygon reduction tools are considered for the order of processing.

- [NDMF Mantis LOD Editor](https://hitsub.booth.pm/items/5409262)
- [lilNDMFMeshSimplifier](https://github.com/lilxyzw/lilNDMFMeshSimplifier)
- [Meshia Mesh Simplification](https://github.com/RamType0/Meshia.MeshSimplification)
- [Overall NDMF Mesh Simplifier](https://github.com/Tliks/OverallNDMFMeshSimplifier)

### Enable on PC

Enable this component when the build target is PC.

### Enable on Android

Enable this component when the build target is Android.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Transforming Phase

Geneates a new mesh for attached object's MeshFilter or SkinnedMeshRenderer when `NDMF Phase` is `Before Polygon Reduction`.

### Optimizing Phase

Generates a new mesh for attached object's MeshFilter or SkinnedMeshRenderer when `NDMF Phase` is `After Polygon Reduction`.
