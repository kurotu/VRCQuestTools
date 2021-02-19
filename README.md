# VRCQuestTools
Unity editor extension to support uploading VRChat avatars for Oculus Quest. Compatible for both of VRCSDK2 and 3.

[![Booth](https://asset.booth.pm/static-images/banner/200x40_01.png)](https://kurotu.booth.pm/items/2436054)

![VRCQuestTools](.images/VRCQuestTools.png)

## Usage

Download the latest `.unitypackage` from [the release page](https://github.com/kurotu/VRCQuestTools/releases/latest) or [Booth](https://kurotu.booth.pm/items/2436054). Then import it to your avatar project.

Select your avatar from a scene, then use **VRCQuestTools** menu item from the menu bar.

## Features

### Convert Avatar for Quest

Convert a PC avatar to be ready to upload for Quest by automating below operations.

⚠ In many cases, the converted avatar would have **Very Poor** performance rank due to [Quest Limits](https://docs.vrchat.com/docs/avatar-performance-ranking-system#quest-limits).

- Duplicate the avatar and its materials.
- Change to "VRChat/Mobile/Toon Lit" shader.
- Generate new textures which applies color and emission of original materials.
- Remove prohibited components such as Dynamic Bone.

VRCQuestTools doesn't make any changes to the original avatar, so you can use the tool for an existing project as is.

### Remove Missing Components

Remove "Missing" components from a GameObject and its descendants.
You will often need to use this when Dynamic Bone asset is missing in your project.

### Tools/Remove Unsupported Components

Remove prohibited components such as Dynamic Bone.

### Tools/BlendShapes Copy

Copy BlendShape (Shape key) weights from a Skinned Mesh Renderer to another Skinned Mesh Renderer.
You will often use when PC version and Quest version use different models and need to have same BlendShape weights.

### Auto Remove Vertex Colors

Automatically remove vertex colors from scene's avatars. By using this, this would fix an issue where main textures are not correctly applied in some avatars.

![VertexColorRemover](.images/VertexColorRemover.png)

### Unity Settings for Quest

Enable useful settings of Unity.

## License

The MIT License.
