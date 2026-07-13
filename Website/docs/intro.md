---
sidebar_position: 1
slug: /intro
---

# What is VRCQuestTools

VRCQuestTools is a Unity editor extension which converts PC VRChat avatars so they can also be used on Android (Quest, PICO) and iOS.
In this documentation, the Android and iOS versions of VRChat are collectively called **Mobile**.

If you try to upload a PC avatar for Mobile as it is, the VRChat SDK stops with errors because shaders and some components are not supported.
VRCQuestTools automates most of the work needed to make the avatar compatible.

## Main Features

- **Material conversion**: Reads the settings of PC shaders and automatically generates materials and textures for Mobile-supported shaders.
- **Removal of unsupported components**: Removes components which can't be used on Mobile at build time.
- **Non-destructive conversion**: When the project has Non-Destructive Modular Framework (NDMF), avatars are automatically converted on upload. The original avatar is not modified.
- **PhysBone reduction support**: Helps you reduce PhysBones to fit within the performance limits.
- **Various utilities**: Provides tools which help Mobile support, such as copying blendshapes and applying recommended Unity settings.

## Requirements

- An avatar project created with VRChat Creator Companion (VCC)
- VRChat SDK - Avatars 3.9.0 or later

## How to Read This Documentation

If you are using the tool for the first time, follow the steps in [Getting Started](./getting-started/set-up-environment.md).
It explains everything from installation to upload in order.

To look up the settings of each component, see the [component reference](./components/index.md).

If you run into a problem, check the [troubleshooting](./troubleshooting.md) page.
