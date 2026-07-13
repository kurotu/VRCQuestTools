---
sidebar_position: 3
---

# Check the Converted Avatar

Material conversion can't perfectly reproduce the appearance of PC shaders.
It is a good idea to check the appearance and behavior of the converted avatar before and after the upload.

## Check the Appearance on PC

You can check the conversion result on PC even without a Mobile device.

With NDMF, there are two ways:

- **Check in the scene**: Right-click the avatar in the Hierarchy and select "VRCQuestTools" → "[NDMF] Manual Bake with Mobile Settings". A copy of the avatar converted with the Mobile settings is created in the scene.
- **Check with a local test in VRChat**: Right-click the avatar in the Hierarchy and select "VRCQuestTools" → "[NDMF] Build and Test for PC with Mobile Settings". You can locally test the avatar converted with the Mobile settings in VRChat on PC.

:::info Screenshot placeholder
A screenshot of the Hierarchy context menu will be placed here.
:::

With manual conversion, you can directly check the "(avatar name) (Android)" copy created in the scene.
Using the VRChat SDK local test (Build & Test), you can check the appearance in VRChat without uploading.

## Points to Check

- **Color and brightness**: Mobile shaders have different lighting, so the impression of brightness changes depending on the world.
- **Transparency**: Mobile shaders don't reproduce texture transparency. Check expressions which depend on transparency, such as blushing or glasses lenses. See [troubleshooting](../troubleshooting.md#transparency) for workarounds.
- **Effects of removed components**: Check that gimmicks still work after unsupported components were removed.
- **PhysBone behavior**: Check the swaying of the reduced PhysBones.

## Check on a Mobile Device

After the upload, also check the avatar on an actual device such as Quest.
When the performance rank is Very Poor, other players see an impostor or your fallback avatar by default.
