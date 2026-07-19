---
sidebar_position: 1
slug: /intro
---

# What is VRCQuestTools

VRCQuestTools is a Unity editor extension which converts PC VRChat avatars so they can also be used on Android (Quest, PICO) and iOS.
In this documentation, the Android and iOS versions of VRChat are collectively called **Mobile**.

Making an avatar work on Mobile has a reputation for being difficult.
Indeed, if you try to upload a PC avatar for Mobile as it is, the VRChat SDK stops with errors because shaders and some components are not supported.
To resolve the errors, you would have to rebuild the materials with Mobile-supported shaders and remove the unsupported components.
On top of that, the performance limits for PhysBones and other features are stricter on Mobile than on PC.
Mobile support is called difficult because doing all of this by hand takes time and knowledge.

VRCQuestTools automates most of this work.

## Main Features

- **Material conversion**: Reads the settings of PC shaders and automatically generates materials and textures for Mobile-supported shaders. You don't have to rebuild materials one by one.
- **Removal of unsupported components**: Removes components which can't be used on Mobile at build time.
- **Non-destructive conversion**: When the project has Non-Destructive Modular Framework (NDMF), avatars are automatically converted on upload without creating a copy in the scene.
- **PhysBone reduction support**: Helps you reduce PhysBones to fit within the performance limits.
- **Various utilities**: Provides tools which help Mobile support, such as copying blendshapes and applying recommended Unity settings.

However, the converted avatar does not look exactly the same as the PC version.
Mobile shaders can't fully reproduce the appearance of PC shaders.
[Check the Converted Avatar](./getting-started/check-avatar.md) explains what tends to change and how to check it before and after the upload.

## Requirements

- VRChat SDK - Avatars 3.9.0 or later

## How to Read This Documentation

If you are using the tool for the first time, read [Getting Started](./getting-started/set-up-environment.md) in order.
It follows the same order as the actual work, from installation to upload and checking the result.

Once you get used to the conversion, you can look up the settings of each component in the [component reference](./components/index.md).
Fine adjustments, such as changing the conversion method per material, are covered there.

If you run into a problem, check the [troubleshooting](./troubleshooting.md) page.
