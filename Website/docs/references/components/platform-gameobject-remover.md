# Platform GameObject Remover

Set whether to remove the game object depending on the build platform.
To override the platform to a specific one, use the [VQT Platform Target Settings](./platform-target-settings) component.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Remove on PC

When the build target is PC, remove the game object this component is attached.

### Remove on Android

When the build target is Android, remove the game object this component is attached.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Resolving Phase

Remove the game object this component is attached depending on the current build target.
