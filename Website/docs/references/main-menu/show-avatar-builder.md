---
sidebar_position: 2
---

# Show Avatar Builder

Show the window to build avatars.

When using NDMF to non-destructively convert avatars, only the avatar before conversion exists in the scene.
You cannot build avatars for Android as is, but you can start building avatars by using this window.

Selecting and entering information for the avatar to build is done in the VRChat SDK Control Panel.

:::tip
VRChat SDK allows you to start uploading avatars even if the avatar in the scene does not meet the Android-compatible conditions.
:::

![Avatar Builder](/img/avatar-builder.png)

## Offline Testing (PC/Android)

You can test avatars on local device by using Avatars 3.0's local testing.
The built avatars will be displayed in the Others category of the avatar list.

## Online Publishing (Android)

You can build avatars and upload them for Android.
Start building by ignoring the validation of the VRChat SDK Control Panel.

## NDMF Manual Bake

Perform a manual bake of NDMF.
Since the texture format is compressed for the current build target, execute it after switching to the platform you want to upload to.
