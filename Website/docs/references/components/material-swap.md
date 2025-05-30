# Material Swap

The VQT Material Swap component provides a simple way to swap materials for mobile platforms.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Overview

- Allows defining mappings between original materials and their mobile replacements.
- Defined mappings take effect whole of the avatar.
- Only activates material swaps when building for mobile platform

## Usage

Create material mapping pairs by:
- Setting the Original Material (material used on PC)
- Setting the Replacement Material (optimized material for Android)

## Properties

### Material Mappings

List of original -> replacement material pairs.

## NDMF

Replace materials when the target platform is mobile.

NDMF Phase is managed by [Avatar Converter Settings] component or the root level Material Conversion Settings component.

When there are no root level components, auto is used.

[Avatar Converter Settings]: avatar-converter-settings.md
