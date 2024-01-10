# Platform Component Remover

Set whether to remove the component depending on the build platform.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Build Target

Select the build platform to apply.
Use `Auto` to automatically determine based on Unity's target platform settings.

### Component Settings

When the platform checkbox is checked, the component is removed on the selected platform.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Resolving Phase

Remove components whose checkboxes for the current build target in the component settings are checked.
