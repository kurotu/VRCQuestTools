---
slug: /references/components/platform-gameobject-remover
---

# Platform GameObject Remover

A component which removes the GameObject at build time depending on the platform.
Attach it to the GameObject you want to remove.
NDMF is required.

For example, you can remove a PC-only accessory entirely on Mobile.

![VQT Platform GameObject Remover Inspector](/img/platform-gameobject-remover.png)

## Settings

| Setting | Description |
|---|---|
| Keep on PC | When checked, keeps this GameObject in PC builds. |
| Keep on Mobile | When checked, keeps this GameObject in Mobile builds. |

The GameObject and its children are removed in builds for the platforms you unchecked.

When NDMF preview is enabled, meshes under the GameObject to be removed are hidden in the Scene view.
Particles and other non-mesh rendering are not reflected in the preview, but they are removed with the GameObject at build time.
