# Menu Icon Resizer

This component resizes, compresses, or removes the icons in the expressions menu at build time.

It would be useful when other tools generate a bunch of icons and you want to reduce the avatar's build size.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Resize Mode

Select the resize mode for each build target.

- `Do Not Resize` - Does not resize the icons.
- `Remove` - Removes the icons instead of resizing.
- `Max NxN` - Resizes the icons to the specified size.

### Compress Uncompressed Textures

Compress textures which are not compressed.

## NDMF

### Optimizing Phase

Resize, compress, or remove icons in the expressions menu.
