# Platform Component Remover

Set whether to remove the component depending on the build platform.
To override the platform to a specific one, use the [VQT Platform Target Settings](./platform-target-settings) component.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Component Settings

When the platform checkbox is checked, the component is removed on the selected platform.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Resolving Phase

Remove components whose checkboxes for the current build target in the component settings are checked.
