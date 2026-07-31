---
sidebar_position: 6
slug: /migration-to-v3
---

# Migration Guide from v2.x to v3.0.0

VRCQuestTools v3.0.0 is a major update.
Updating can affect scenes and prefabs you created under v2.x, and part of the conversion procedure.

Non-Destructive Modular Framework (NDMF) conversion doesn't rewrite avatars in the scene, so your existing settings are applied the same way at build time after the update.
If you don't meet the required versions below or use a removed feature, you need to take action when migrating.

## Requirement Changes

v3.0.0 requires the following versions.

- Unity 2022.3 or later
- VRChat SDK - Avatars 3.9.0 or later
- lilToon 1.10.0 or later (if used)
- NDMF 1.5.0 or later (if used)

For lilToon and NDMF, an outdated version aborts the build or conversion with an error, so check these versions in VCC or ALCOM before updating.

## Removal of the VQT Avatar Builder Window

The **VQT Avatar Builder** window has been removed.
What it used to do is replaced by one of the following.

- For a regular upload, build and upload directly from the VRChat SDK's Control Panel.
- To test locally only, right-click the avatar and select "VRCQuestTools" → "[NDMF] Build and Test for PC with Mobile Settings".
    It builds for PC with Mobile settings applied, so you can test it right away.

## Behavior Changes

### Where Avatar Dynamics Settings Are Stored

The PhysBone/PhysBone Collider/Contacts settings chosen in the **Avatar Dynamics Selector** are now stored in a **Platform Component Remover** component instead of inside the **Avatar Converter Settings** component.

This migration doesn't happen automatically.
An avatar already set up under v2.x keeps working from the settings stored in Avatar Converter Settings, unless you press the "Apply" button in the Avatar Dynamics Selector.
So updating the package alone never loses your existing settings.
Press the "Apply" button only when you want to move your settings to the new location.

### How Vertex Color Removal Works

Vertex color removal during conversion changed from using the **Vertex Color Remover** component to generating a dedicated mesh (a `.vqtmesh` asset) for the converted avatar.
In v2.x, vertex colors were removed directly from the original mesh, so for avatars that use vertex colors to control their outlines, the pre-conversion avatar's appearance could also change.
Conversion in v3.0.0 no longer causes this problem.
The Vertex Color Remover component itself is still available, and attached components keep working as before.

### Avatar Active State After Manual Conversion

When you manually convert from Avatar Converter Settings, the original avatar is no longer deactivated.
Note that both avatars remain active in the scene.

### Platform GameObject Remover and Platform Component Remover in Manual Conversion

Manual conversion now also applies **Platform GameObject Remover** and Platform Component Remover settings for Mobile.
These settings weren't reflected in v2.x manual conversion, so if your avatar specifies removal targets for Mobile, the conversion result differs from v2.x.
For avatars whose Avatar Dynamics settings haven't been migrated to Platform Component Remover, components are still removed based on the settings in Avatar Converter Settings, as before.

## Migration Checklist

1. Check that your project's Unity, VRChat SDK, lilToon, and NDMF versions meet the required versions above.
2. Update VRCQuestTools to v3.0.0 with VCC or ALCOM.
3. If needed, press "Apply" in the Avatar Dynamics Selector to finish migrating to Platform Component Remover.

The [Changelog](./changelog.md) covers every change in v3.0.0.
