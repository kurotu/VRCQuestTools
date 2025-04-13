# Material Conversion Settings

:::warning
This component is an experimental feature.
:::

Material Conversion Settings component holds settings for converting materials for mobile platform.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

To use settings which are disabled in the inspector, place the component on the root object of the avatar.
But [Avatar Converter Settings] takes priority.

## Properties

Please refer to the [Avatar Converter Settings] for details.

### Default Material Conversion Settings
### Additional Material Conversion Settings
### Remove Extra Material Slots
### NDMF Phase to Convert

## NDMF

Convert materials when the target platform is mobile.

NDMF Phase is managed by [Avatar Converter Settings] component or the root level Material Conversion Settings component.

When there are no root level components, transforming phase is used.

[Avatar Converter Settings]: avatar-converter-settings.md
