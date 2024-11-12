# Platform Component Remover

Set whether to remove the component depending on the build platform.
To override the platform to a specific one, use the [VQT Platform Target Settings](platform-target-settings.md) component.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## Properties

### Component Settings

When the platform checkbox is unselected, the component is removed when building for the platform.

In the example below, the `RemoveMeshByBlendShape` component is removed when building for PC.

![Platform Component Remover](/img/platform-component-remover.png)

## NDMF

The VRCQuestTools plugin performs the following processes.

### Resolving Phase

Remove components by component settings.
