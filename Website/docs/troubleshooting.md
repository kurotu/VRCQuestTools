---
sidebar_position: 4
---

# Troubleshooting

## Cannot Switch Unity to Android

### Install Android Build Support

To upload avatars for Android, you need to switch Unity's build settings to Android (Switch Platform).
If Android Build Support is not installed in Unity, you cannot switch the build settings to Android.

[Tutorial: Set up Environment](./tutorial/set-up-environment.mdx)

## Uncompressed Size is Too Large

`Avatar uncompressed size is too large for the target platform. XX.XX MB > 40.00 MB`

The main causes of large uncompressed size include:

- Unused shape keys
- Unused meshes
- Unused textures

### Remove unnecessary data by optimization

You can remove these unnecessary data by using avatar optimization tools.
For example, using the [Trace And Optimize](https://vpm.anatawa12.com/avatar-optimizer/en/docs/reference/trace-and-optimize/) component from [Avatar Optimizer](https://anatawa12.booth.pm/items/4885109) can automatically remove unnecessary data during upload.

## Download Size is Too Large

`Avatar download size is too large for the target platform. XX.XX MB > 10.00 MB`

The main causes of large download size include:

- Too many textures
- Low texture compression ratio
- Large texture resolution
- Too many meshes

### Adjust texture size

While reducing the `Textures Size Limit` in the [Avatar Converter Settings](./references/components/avatar-converter-settings.md) component can decrease download size, it significantly degrades texture quality.
It is recommended to adjust the `Compression Format` before changing the textures size limit.

### Atlas textures

You can also reduce download size by atlasing textures and reducing the total resolution.
For example, you can use [TexTransTool](https://rs-shop.booth.pm/items/4833984) for [atlasing](https://ttt.rs64.net/en/docs/Tutorial/ReductionTextureMemoryByAtlasing).

### Reduce menu icon size

When using tools that add facial expressions or pose animations, a large number of menu icons can occupy the download size.
You can use the [Menu Icon Resizer](./references/components/menu-icon-resizer.md) component to reduce the resolution of menu icons or remove them.

### Reduce outfit change gimmicks

When implementing gimmicks like outfit changes, the download size increases due to additional meshes and textures for the outfit variations.
Reduce gimmicks and, if possible, upload with only one outfit per avatar.
To prevent gimmicks like outfit changes from being in unintended states on one platform due to sync issues, using the same avatar configuration for both PC and Android is recommended.

## Upload Succeeded but Shows "Security checks failed"

If the security check fails on VRChat's server side, the avatar may not be usable even if the upload succeeds.

### Switch platform again and upload

There have been reports of cases where the security check failed even though there was no actual problem, due to Unity's build target switching failure.
Try switching the platform again or restart Unity before attempting to upload.
At this time, make sure Unity's build target is set to the platform you're uploading to.

![How to check Unity's build target](/img/unity_titlebar_android.png)

## Gimmicks Don't Sync Between PC and Android

If the order of Expression Parameters doesn't match between PC and Android, incorrect values will be synchronized, leading to gimmick malfunctions.

### Use the same avatar configuration for PC and Android

This is particularly likely to occur when certain gimmicks exist only on PC and not on Android, so using the same avatar configuration for both PC and Android is recommended.

### Match the order of Expression Parameters

You can use the [Sync Parameter Sequence](https://modular-avatar.nadena.dev/docs/reference/sync-parameter-sequence) component from [Modular Avatar](https://modular-avatar.nadena.dev/) to match the order of Expression Parameters.

## PhysBones Don't Sync Between PC and Android

If the number or order of PhysBones differs between PC and Android, the behavior when grabbing PhysBones will not sync.

### Match network IDs

You can synchronize PhysBones by using the [Network ID Assigner](./references/components/network-id-assigner.md) component and uploading again to both PC and Android.
