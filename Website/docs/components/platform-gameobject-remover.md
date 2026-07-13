---
slug: /references/components/platform-gameobject-remover
---

# Platform GameObject Remover

A component which removes the GameObject at build time depending on the platform.
Attach it to the GameObject you want to remove.
NDMF is required.

For example, you can remove a PC-only accessory entirely on Mobile.

:::info Screenshot placeholder
A screenshot of the VQT Platform GameObject Remover Inspector will be placed here.
:::

## Settings

| Setting | Description |
|---|---|
| Keep on PC | When checked, keeps this GameObject in PC builds. |
| Keep on Mobile | When checked, keeps this GameObject in Mobile builds. |

The GameObject and its children are removed in builds for the platforms you unchecked.
