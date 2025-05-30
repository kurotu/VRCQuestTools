# Avatar Converter Settings

Avatar Converter Settings component holds settings for converting VRChat avatars to Android.

This component should be attached to the root object of the avatar.

## Basic Settings

### Folder to Save

Show the folder where the generated assets are saved when converting.

- The folder is `Assets/VRCQuestToolsOutput/<Avatar object name>`.
- Assets are overwritten when converting.

## Material Conversion Settings

### Default Material Conversion Settings

Default material conversion settings used during conversion.
If you need to convert each material individually, use **Additional Material Conversion Settings**.

The following shaders are supported for material conversion. For unsupported shaders, the conversion process of the Standard shader is applied.

- `Standard`
- `Standard (Specular setup)`
- `Unlit/` shaders
- Unity-Chan Toon Shader 2.0 (UTS2)
- Arktoon-Shaders
- ArxCharacterShaders (AXCS)
- Sunao Shader
- lilToon
- Poiyomi Toon Shader

| Material Conversion Settings | Description |
|---|---|
| Toon Lit | Use the `VRChat/Mobile/Toon Lit` shader for the converted material. |
| MatCap Lit | Use the `VRChat/Mobile/MatCap Lit` shader for the converted material. After applying the same conversion process as Toon Lit, use the MatCap texture specified in the conversion settings. |
| Toon Standard | Use the `VRChat/Mobile/Toon Standard` shader for the converted material. |
| Material Replacement | Replace with the specified material in the conversion settings. |

| Parameter | Description | Conversion Mode |
|---|---|---|
| Generate Textures for Android | Generate Android textures when converting avatar. If off, the original main texture is used. | Toon Lit, MatCap Lit, Toon Standard |
| Textures Size Limit | Select the maximum size of the generated texture. | Toon Lit, MatCap Lit, Toon Standard |
| Main Texture Brightness | Select the brightness of the generated main texture. | Toon Lit, MatCap Lit, Toon Standard |
| Compression Format | Select the compression format of the generated textures. DXT5 is used for PC platform. | Toon Lit, MatCap Lit, Toon Standard |
| Generate shadows from normal map | Generate pseudo shadows from normal maps and reflect them in textures. | Toon Lit, MatCap Lit |
| MatCap Texture | Set the MatCap texture used by the MatCap Lit shader. | MatCap Lit |
| Fallback Shading | Select the Ramp texture used when converting unsupported materials. | Toon Standard |
| Replaced Material | Set the material used in Material Replacement mode. | Material Replacement |

### Additional Material Conversion Settings

Use this when you want to set conversion settings for each material.

| Parameter | Description |
|---|---|
| Target Material | Set the material to which the conversion settings are applied. |
| Material Conversion Settings | Set the material conversion settings to apply to the target material. |

### Update Converted Android Textures

Update the converted Android textures with the current settings.

## Avatar Dynamics Settings

Set the Avatar Dynamics (PhysBone, Collider, Contact) for the avatar.
Android avatars have a maximum of Poor for Avatar Dynamics, so specify the components to keep after conversion and delete the rest during conversion.

### Remove Avatar Dynamics

Remove avatar dynamics components which are not selected in the **Avatar Dynamics Settings**.

### Avatar Dynamics Settings

Select checkboxes for the components to keep after conversion in the window that opens when you click the **Avatar Dynamics Settings** button.
After selecting, click the **Apply** button.

### PhysBones, PhysBone Colliders, Contact Senders & Receivers

Lists of PhysBones, Colliders, and Contacts to keep after conversion.

### Estimated Performance Rank

Estimated performance rank of Avatar Dynamics.
Set all items to Poor or better.

## Advanced Converter Settings

### Animation Override

Use the animation clips specified in Animator Override Controller to create a new Animator Controller during conversion.

### Remove Vertex Color from Meshes

Add the [Vertex Color Remover](vertex-color-remover.md) component to the converted avatar to remove vertex colors.

You don't usually need to turn this option off. Turn it off if you are using a special shader for PC avatars that uses vertex colors in order to prevent unintended behavior.

### Remove Extra Material Slots

Remove material slots that are greater than the number of submeshes for each renderer.

This option is useful when the original avatar uses additional material slot for some reason (for example, lilToon's Fake Shadow).

### Compress Menu Icons

Compress expressions menu icons which are not compressed.

This options is useful to reduce avatar's build size when other tools generate a bunch of icons.

### NDMF Phase to Convert

Select a NDMF phase to convert the avatar.

If you select Auto, it's treated as following phases:

- Normal: Optimizing
- VRCFury components exist: Transforming

## Convert

Execute the conversion of the avatar.
Typically, the avatar is converted in the following steps.

1. Generate textures and materials for Android.
2. Convert the animation clip to use the material created in step 1.
3. Convert the Animator Controller to use the converted animation clip and resolve the Animator Override Controller.
4. Duplicate the original avatar and set the converted assets.
5. Delete unsupported components such as Constraints.
6. Add [Platform Target Settings](platform-target-settings.md) and set the build target to Android.
7. Execute special conversion process (described below).
8. Deactivate the original avatar in the scene.

### Special conversion process

Depending on the assets you are using, a special conversion process is executed.

#### VirtualLens2

Set `Remote Only Mode` of VirtualLensSettings component to `Force Enable`.

If you are using VirtualLens2 with the destructive setup, set `EditorOnly` tag to `_VirtualLens_Root` and `VirtualLensOrigin` objects.

## NDMF

The VRCQuestTools plugin performs the following processes in the project which have Non-Destructive Modular Framework (NDMF).
To avoid VRChat SDK validation, use the Avatar Builder window to build the avatar when building for Android.

### Resolving Phase

Change VirtualLens2 settings for Android.

### Transforming Phase

Remove vertex colors from the meshes.
This process is performed after NDMF Mantis LOD Editor.

Convert the avatar for Android when the NDMF conversion phase is Transforming.
This process is performed after TexTransTool and Modular Avatar.

### Optimizing Phase

Convert the avatar for Android when the NDMF conversion phase is Optimizing.
This process is performed after TexTransTool and before AAO: Avatar Optimizer.

Compress menu icons which are not compressed.
