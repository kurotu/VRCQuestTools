---
sidebar_position: 1
---

# Convert Avatar for Quest

Convert your VRChat avatar to be uploadable for Quest (Android) platform.

![Convert Avatar for Quest](/img/convert_avatar_for_quest.png)

This feature converts avatars by following steps:
1. Generate textures for Quest.
2. Create `VRChat/Mobile/Toon Lit` materials then set textures which are generated at step 1.
3. Convert animation clips which replace materials by using materials which are converted at step 2.
4. Convert animator controllers with converted animation clips and resolve animator override controllers.
5. Duplicate the original avatar and set converted assets.
6. Remove unsupported components such as Constraints.
7. Deactivate the original avatar in the scene.

## Settings

### Avatar

Set an avatar to convert. Its GamebObject needs to have a `VRCAvatarDescriptor` component.

### Network IDs

Assign network IDs to the avatar. Network IDs are used to sync PhysBones between PC and Quest even if they have different PhysBone structures.
Upload the avatar for PC platform again after assinging network IDs. Converted avatar will have the same network IDs as the original avatar.

See [Network ID Utility](https://creators.vrchat.com/worlds/udon/networking/network-id-utility/) for more details.

### DynamicBone Conversion

Convert `DynamicBone` and `DynamicBoneCollider` components to `VRCPhysBoene` and `VRCPhysBoneCollider` components with VRCSDK's feature.
`DynamicBone` is not allowed for Quest avatars, but `VRCPhysBone` is allowed. So you can keep avatars' dynamics such as hair and skirt by converting components.

See [Manual Dynamic Bone Conversion](https://creators.vrchat.com/avatars/avatar-dynamics/physbones/#manual-dynamic-bone-conversion) for more details.

### Generate Textures for Quest

Generate textures for Quest when converting avatars.
Generated textures will reflect some original materials' shader parameters such as emission.

Supported shaders:
- `Standard`
- `Standard (Specular setup)`
- `Unlit/` shaders
- `UnityChanToonShader/` shaders
- `arktoon/` shaders
- `ArxCharacterShaders/` shaders
- `Sunao Shader` shaders
- `lilToon`

### Textures Size Limit

Choose max size of generated textures.

### Main Texture Brightness

Make generated textures darker than original textures because `Toon Lit` often cause blown out highlights.

### Update Only Quest Textures

Just generates textures for Quest. You can use this feature to just update textures when you modified original materials.

### Folder to Save

Select a folder to save converted assets. Folder structure will be like this:

```
FOLDER_TO_SAVE/
|-- AvatarName/
|   |-- Animations/
|   |-- AnimatorControllers/
|   |-- BlendTrees/
|   |-- Materials/
|   └-- Textures/
```

### Remove Vertex Color from Meshes

Converted avatars will have `VertexColorRemover` component. This component removes vertex colors from meshes.

### Animation Override

Generate new animator controllers which are overridden by animator override controllers.
