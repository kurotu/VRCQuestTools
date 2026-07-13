---
slug: /references/components/platform-component-remover
---

# Platform Component Remover

A component which removes components on the same GameObject at build time depending on the platform.
Attach it to the GameObject with the components you want to remove.
NDMF is required.

Use it when there are components you want to use on PC but remove on Mobile (or the other way around).
For example, you can remove specific PhysBones only when uploading for Mobile.

:::info Screenshot placeholder
A screenshot of the VQT Platform Component Remover Inspector will be placed here.
:::

## Settings

"Components to Keep" lists the components on the same GameObject.

| Setting | Description |
|---|---|
| PC | Keeps the checked components in PC builds. |
| Mobile | Keeps the checked components in Mobile builds. |

Components are removed in builds for the platforms you unchecked.

## Notes

When you select the components to keep in "Avatar Dynamics Settings" of [VQT Avatar Converter Settings](./avatar-converter-settings.md), this component is automatically configured on the target GameObjects.
