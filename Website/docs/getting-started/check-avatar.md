---
sidebar_position: 3
---

# Check the Converted Avatar

Material conversion can't perfectly reproduce the appearance of PC shaders.
The removal of unsupported components may also change how gimmicks behave.
The reliable way to know what changed is to actually display the avatar and see.
You can check the conversion result on PC even without a Mobile device.

## Check the Appearance on PC

When using NDMF, there are two ways:

- **Preview in the scene**: Press the "[NDMF] Enable temporary preview" button in the inspector of [VQT Avatar Converter Settings](../components/avatar-converter-settings.md). The converted materials are displayed in the Scene view. Press the button again to go back to the original appearance.
- **Check with a local test in VRChat**: Right-click the avatar in the Hierarchy and select "VRCQuestTools" → "[NDMF] Build and Test for PC with Mobile Settings". You can locally test the avatar converted with the Mobile settings in VRChat on PC.

With manual conversion, you can directly check the "(avatar name) (Android)" copy created in the scene.
Using the VRChat SDK local test (Build & Test), you can check the appearance in VRChat without uploading.

## Points to Check

- **Color and brightness**: Mobile shaders have different lighting, so the impression of brightness changes depending on the world.
- **Transparency**: Mobile shaders don't reproduce texture transparency. Expressions which depend on transparency, such as blushing or glasses lenses, are affected the most. See [troubleshooting](../troubleshooting.md#transparency) for workarounds.
- **Effects of removed components**: Check that gimmicks still work after unsupported components were removed.
- **PhysBone behavior**: Check the swaying of the reduced PhysBones.

## Check on a Mobile Device

After the upload, also check the avatar on an actual device such as Quest.
When the performance rank is Very Poor, other players see an impostor or your fallback avatar by default.
What you see on your own screen is not necessarily what others see.
