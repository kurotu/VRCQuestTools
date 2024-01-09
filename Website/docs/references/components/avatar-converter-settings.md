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
| Material Replacement | Replace with the specified material in the conversion settings. |

| Parameter | Description | Conversion Mode |
|---|---|---|
| Generate Textures for Android | Generate Android textures when converting avatar. If off, the original main texture is used. | Toon Lit, MatCap Lit |
| Textures Size Limit | Select the maximum size of the generated texture. | Toon Lit, MatCap Lit |
| Main Texture Brightness | Select the brightness of the generated main texture. | Toon Lit, MatCap Lit |
| Generate shadows from normal map | Generate pseudo shadows from normal maps and reflect them in textures. | Toon Lit, MatCap Lit |
| MatCap Texture | Set the MatCap texture used by the MatCap Lit shader. | MatCap Lit |
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

Add the [Vertex Color Remover](./vertex-color-remover) component to the converted avatar to remove vertex colors.

You don't usually need to turn this option off. Turn it off if you are using a special shader for PC avatars that uses vertex colors in order to prevent unintended behavior.

## Convert

Execute the conversion of the avatar.
Typically, the avatar is converted in the following steps.

1. Generate textures and materials for Android.
2. Convert the animation clip to use the material created in step 1.
3. Convert the Animator Controller to use the converted animation clip and resolve the Animator Override Controller.
4. Duplicate the original avatar and set the converted assets.
5. Delete unsupported components such as Constraints.
6. Execute special conversion process (described below).
7. Deactivate the original avatar in the scene.

### Special conversion process

Depending on the assets you are using, a special conversion process is executed.

#### Platform Component Settings

Set `Build Target` to `Android`.

#### VirtualLens2

Set `Remote Only Mode` of VirtualLensSettings component to `Force Enable`.

If you are using VirtualLens2 with the destructive setup, set `EditorOnly` tag to `_VirtualLens_Root` and `VirtualLensOrigin` objects.
