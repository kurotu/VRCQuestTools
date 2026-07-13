---
slug: /references/components/material-swap
---

# Material Swap

A component to replace materials with other materials in Mobile builds.
Use it when you have your own materials for Mobile.
NDMF is required.

Renderers of the GameObject with this component and its children are the targets of the replacement.

:::info Screenshot placeholder
A screenshot of the VQT Material Swap Inspector will be placed here.
:::

## Settings

| Setting | Description |
|---|---|
| Original Material | The material to replace. You can pick it from the targets with the "Select from children" button. |
| Replacement Material | The material to replace with. It must use a shader allowed for Mobile avatars. |

Add pairs of materials to the "Material Mappings" list.

## Notes

- When the shader of the replacement material is not allowed for Mobile avatars, an error appears in the Inspector.
- If you just want to change the conversion per material, you can also select "Material Replacement" in "Additional Material Conversion Settings" of [VQT Avatar Converter Settings](./avatar-converter-settings.md).
