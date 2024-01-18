# Platform GameObject Remover

Set whether to remove the game object depending on the build platform.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Build Target

Select the build platform to apply.
Use `Auto` to automatically determine based on Unity's target platform settings.

### Remove on PC

When the build target is PC, remove the game object this component is attached.

### Remove on Android

When the build target is Android, remove the game object this component is attached.

## NDMF

The VRCQuestTools plugin performs the following processes.

### Resolving Phase

Remove the game object this component is attached depending on the current build target.
