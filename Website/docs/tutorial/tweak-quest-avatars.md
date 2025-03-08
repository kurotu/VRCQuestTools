---
sidebar_position: 4
---

# Tweak Android Avatars

After testing, you may find some issues in Android avatars.
This page describes how to tweak Android avatars with examples.

:::note
This page requires knowledge of some basic technical terms of Unity.
:::

## Transparent Meshes

Android avatars don't support transparent materials.
So converted avatars may have issues with transparent materials.
For example, facial expressions (blush and pallor), glasses lens, and eyes (cornea).

There are various ways to solve issues though, this page describes three ways.

- [Edit Animations](#edit-animations)
- [Delete Transparent Meshes](#delete-transparent-meshes)
- [Tweak Textures](#tweak-textures)

:::caution
Do not use shaders which are under `VRChat/Mobile/Particles` as alternatives for transparent materials.
They are for particles, not for avatars.

Refer to [Quest Content Limitations](https://creators.vrchat.com/platforms/android/quest-content-limitations/#shaders).
:::

### Edit Animations

In most cases, problematic facial expressions are implemented as blendshapes animationss.
So you can suppress them by editing animations.

1. Find problematic animation clips in your project folder and duplicate them.
2. Open a duplicated animation clip in the Animation window.
3. Edit animation parameters not to use problematic blendshapes.
4. Duplicate the animator controller of the FX layer and open it.
5. Replace problematic animations with edited animations.
6. Set the duplicated animator controller to the FX layer.

:::tip
After creating new animation clips, you can create an **[Animator Override Controller](https://docs.unity3d.com/2019.4/Documentation/Manual/AnimatorOverrideController.html)** to override the original animations.
VRCQuestTools autocmatically creates new Animator Controller by resolving Animator Override Controllers when converting an avatar.
See [the reference page](../references/components/avatar-converter-settings.md) for more details.
:::

### Delete Transparent Meshes

In some cases, problematic parts of meshes can be removed instead of editing animations.
This page just suggests tools to edit meshes.

- [MeshDeleterWithTexture](https://gatosyocora.booth.pm/items/1501527) by gatosyocora
- [Avatar Optimizer](https://vpm.anatawa12.com/avatar-optimizer/en/) by anatawa12
- [Blender](https://www.blender.org/)

### Tweak Textures

In most cases, problematic facial expressions are implemented as transparent meshes and such meshes are superimposed on the surface of the avatar's face.
So you may suppress issues by filling transparent area with avatar's skin color.

### Samples

#### Pallor Face

Edit blendshapes animations or the mesh to suppress pallor.

Models:
- [I-s Ver.2.0](https://atelier-alca.booth.pm/items/2460693) by トクナガ

| PC Version | Converted Avatar | Tweaked Avatar |
|---|---|---|
| ![Is2_Darkness_PC](/img/Is2_Darkness_PC.png) | ![Is2_Darkness_Convert](/img/Is2_Darkness_Convert.png) | ![Is2_Darkness_Convert](/img/Is2_Darkness_Tweak.png) |

#### Glasses Lenses

Remove lenses by editing the glasses mesh.

Models:
- [桔梗](https://ponderogen.booth.pm/items/3681787) by ぽんでろ
- [Slim Wing](https://wotapacchin.booth.pm/items/1460758) by をたぱち

| PC Version | Converted Avatar | Tweaked Avatar |
|---|---|---|
| ![Kikyo_Glasses_PC](/img/Kikyo_Glasses_PC.png) | ![Kikyo_Glasses_Convert](/img/Kikyo_Glasses_Convert.png) | ![Kikyo_Glasses_Tweak](/img/Kikyo_Glasses_Tweak.png) |

#### Eyes (Corneas)

Remove corneas by editing the eyes mesh.

Models:
- [店員ちゃん](https://avatarchan.booth.pm/items/2704657) by コトブキヤ

| PC Version | Converted Avatar | Tweaked Avatar |
|---|---|---|
| ![DP001_Eyes_PC](/img/DP001_Eyes_PC.png) | ![DP001_Eyes_Convert](/img/DP001_Eyes_Convert.png) | ![DP001_Eyes_Tweak](/img/DP001_Eyes_Tweak.png) |

## Build Size

In general, textures and meshes are the main factors of build size.
Due to 10 MB limit of Android avatars, you may need to reduce the size of textures and meshes.

### Omit Unnecessary GameObjects

GameObjects which have **EditrOnly** tag are not included in the build. This means you can omit unnecessary meshes and materials from your avatar.
So you may be able to reduce the build size by tagging unnecessary GameObjects with **EditorOnly** tag.

![EditorOnly_Tag](/img/EditorOnly_Tag.png)

### Omit Unnecessary Shapekeys

Shapekeys (blend shapes) increase the size of meshes. It leads to increase the uncompressed size of the avatar.

You can remove unnecessary shapekeys by using the [`Trace and Optimize`](https://vpm.anatawa12.com/avatar-optimizer/en/docs/reference/trace-and-optimize/) component of Avatar Optimizer.

### Tweak Texture Compression Settings

In many cases, textures are the main factor of build size.
There are two approaches to reduce the size of textures.

- [Reduce texture resolution](#reduce-texture-resolution)
- [Tweak texture compression settings](#tweak-texture-compression-settings)

#### Reduce Texture Resolution

Just reducing texture resolution is the easiest and effective way to reduce the size of textures.
You can reduce texture resolution by changing **Max Size** in the inspector.

![Texture Max Size](/img/texture_max_size.png)

#### Tweak Texture Compression Settings

Unity uses **ASTC 6x6 block** for ASTC compression by default.
You can change texture quality by change ASTC block size.

| ASTC Block Size | Texture Quality | Size |
|---|---|---|
| 4x4 | High | Large |
| : | : | : |
| 6x6 | Default | Default |
| : | : | : |
| 12x12 | Low | Small |

You can set ASTC block size in the platform-specific override setting of the inspector.
Select the **Override for Android** checkbox and change **Format** drop-down menu.

![Texture Override](/img/texture_override_android.png)
