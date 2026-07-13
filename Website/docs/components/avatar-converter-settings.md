---
slug: /references/components/avatar-converter-settings
---

# Avatar Converter Settings

A component to configure the conversion which makes the avatar uploadable for Mobile (Android, iOS).
Attach it to the avatar's root object (the object with VRC Avatar Descriptor).

Usually you add it from "Tools" → "VRCQuestTools" → "Setup Avatar for Mobile" in the menu bar.
See [Convert Your Avatar](../getting-started/convert-avatar.md) for the steps.

When the project has NDMF, the avatar is automatically converted in Mobile builds.
Without NDMF, press the "Convert" button in the Inspector to create a converted copy of the avatar.

:::info Screenshot placeholder
A screenshot of the VQT Avatar Converter Settings Inspector will be placed here.
:::

## Material Conversion Settings

### Default Material Conversion Settings

Selects how to convert the materials in the avatar.
The default is "Toon Standard".

| Conversion | Description |
|---|---|
| Toon Lit | Converts to the VRChat/Mobile/Toon Lit shader. The most lightweight, but the expression is simple. |
| MatCap Lit | Converts to the VRChat/Mobile/MatCap Lit shader. Adds MatCap expression on top of Toon Lit. |
| Toon Standard | Converts to the VRChat/Mobile/Toon Standard shader. Supports expressions closer to PC shaders, such as shading and rim lighting. |
| Material Replacement | Replaces the material with a specified one instead of converting. |

The settings of each conversion are as follows.

#### Toon Lit

| Setting | Description |
|---|---|
| Generate Textures for Mobile | Generates textures using the material's parameters to make the avatar closer to the PC appearance. |
| Max Size | The maximum resolution of generated textures (256 / 512 / 1024 / 2048 / No Limit). |
| Compression | The texture compression format for Android. Smaller ASTC block sizes give higher quality and larger texture size. |
| Brightness | Adjusts the color of the main texture. Because the Toon Lit shader is brightened by ambient light, the default is 0.83. |
| Generate shadow from normal map | Bakes the unevenness of the normal map into the texture as shading. |

#### MatCap Lit

In addition to the same settings as Toon Lit:

| Setting | Description |
|---|---|
| MatCap Texture | The MatCap texture used for the material's expression. Required. |

#### Toon Standard

| Setting | Description |
|---|---|
| Generate Textures for Mobile | Generates textures using the material's parameters to make the avatar closer to the PC appearance. |
| Main Texture Settings | Max size and compression format of the main texture. |
| Mask Texture Settings | Max size and compression format of the mask textures of each feature. |
| MatCap Texture Settings | Max size and compression format of the MatCap texture. |
| Generate shadow ramp | Generates a shadow ramp from the shading settings of the original material. When disabled, the shadow ramp specified in "Fallback Shading" is used. |
| Features | Selects whether to include normal map, emission, occlusion, specular, MatCap, and rim lighting in the conversion. |

The "Mode" of the features decides how new features added in the future are handled.
With opt-in, new features stay disabled; with opt-out, they are automatically enabled.

#### Material Replacement

| Setting | Description |
|---|---|
| Replaced Material | The target material is replaced with this material. It must use a shader allowed for Mobile avatars. |

### Additional Material Conversion Settings

Add these when you want specific materials to use a conversion different from the default.
Specify the original material in "Target Material" and select the conversion.

## Avatar Dynamics Settings

| Setting | Description |
|---|---|
| Remove Avatar Dynamics | Keeps only the selected PhysBones and Contacts, and removes the others during conversion. |

Press the "Avatar Dynamics Settings" button to open the selection window.
Check the components you want to keep, then press "Apply".
Because Avatar Dynamics must fit within the Poor performance rank on Mobile, reduce the components while checking the estimated performance rank.

## Advanced Converter Settings

| Setting | Description |
|---|---|
| Animation Override | Converts Animator Controllers using the animations specified by Animator Override Controllers. |
| Remove Vertex Color from Meshes | Removes vertex colors to display texture colors correctly. Usually keep this enabled. |
| Remove Extra Material Slots | Removes material slots which exceed the number of submeshes. |
| Resize Menu Icons | Resizes expressions menu icons in Mobile builds. |
| Compress Uncompressed Textures | Compresses expressions menu icons if they are uncompressed. |
| Assign Network IDs | Assigns network IDs to PhysBones at build time. Used to synchronize PhysBones between PC and Mobile. |
| NDMF Phase to Convert | Specifies in which NDMF phase the avatar is converted. Usually keep it Auto. |
| Enable Material Preview | Previews converted materials in the Scene view. |

## Notes

- Components which can't be used on Mobile (such as Constraints) are removed during conversion. The components to be removed are shown as warnings in the Inspector.
- Dynamic Bone is not converted to PhysBone. Migrate to PhysBones beforehand.
- Materials and textures generated by manual conversion are saved in the folder shown as "Folder to Save".
