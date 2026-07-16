---
slug: /references/components/platform-target-settings
---

# Platform Target Settings

A component which forces the platform detection at build time to a specific platform.
Attach it to the avatar's root object.
NDMF is required.

VRCQuestTools components with settings like "Keep on PC" or "Enable on Mobile" normally switch their behavior based on Unity's target platform.
With this component, the avatar is built with the settings of the specified platform regardless of Unity's target platform.

![VQT Platform Target Settings Inspector](/img/platform-target-settings.png)

## Settings

| Setting | Description |
|---|---|
| Build Target | Specifies the platform at build time. With Auto, Unity's target platform is used. |

## Notes

This is convenient to check the Mobile behavior in PC builds.
For example, setting "Build Target" to Android builds the avatar with the Mobile settings even when Unity targets Windows.
