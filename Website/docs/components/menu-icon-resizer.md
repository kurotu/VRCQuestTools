---
slug: /references/components/menu-icon-resizer
---

# Menu Icon Resizer

A component to resize expressions menu icons at build time.
It reduces the texture size of the icons and the download size of the avatar.
Attach it to the avatar's root object.
NDMF is required.

![VQT Menu Icon Resizer Inspector](/img/menu-icon-resizer.png)

## Settings

| Setting | Description |
|---|---|
| Resize Mode (PC) | How to handle the icons in PC builds. |
| Resize Mode (Mobile) | How to handle the icons in Mobile builds. |
| Compress Uncompressed Textures | Compresses the icons if they are uncompressed. |
| Compression Format | The texture compression format for Android. |

The resize modes are:

- **Do Not Resize**: Keeps the icons.
- **Remove**: Removes the icons.
- **Max 64x64**: Shrinks the icons to fit in 64×64 pixels.
- **Max 128x128**: Shrinks the icons to fit in 128×128 pixels.

## Notes

"Menu Icon Settings" of [VQT Avatar Converter Settings](./avatar-converter-settings.md) does the same processing.
Use this component when you want to process only the icons without avatar conversion.
