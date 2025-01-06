# Material Swap Component

The VQT Material Swap component provides a simple way to swap materials on GameObjects based on the target platform (PC/Android). This is an **experimental** component that demonstrates the concept of platform-specific material optimization.

## Overview

- Allows defining mappings between original materials and their Android replacements
- Can operate on a single GameObject or include all child renderers
- Only activates material swaps when building for Android platform
- Part of the NDMF build pipeline

## Usage

1. Add the component to a GameObject that has a Renderer (MeshRenderer, SkinnedMeshRenderer, etc.)
2. Create material mapping pairs by:
   - Setting the Original Material (material used on PC)
   - Setting the Replacement Material (optimized material for Android)
3. Toggle "Enable on Android" to control when swaps occur
4. Toggle "Include Child Objects" to affect all child renderers

## Limitations

- Naive implementation that directly swaps material references
- No validation of material compatibility or properties
- May not handle all material slot configurations correctly
- Material references must be manually maintained
- Experimental feature that may change significantly in future versions

## Properties

| Property | Description |
|----------|-------------|
| Material Mappings | List of original -> replacement material pairs |
| Enable on Android | Whether to perform swaps on Android builds |
| Include Children | Whether to process child renderer components |

## Best Practices

- Test material swaps thoroughly on both platforms
- Keep material pairs organized and clearly labeled
- Consider using material property modifications instead for simple changes
- Back up materials before creating swap configurations

## Related Components

- [Avatar Converter Settings](avatar-converter-settings.md) - Main avatar optimization component
- [Platform Component Remover](platform-component-remover.md) - Remove components per platform

## Future Development

This is a proof-of-concept implementation. Future versions may:

- Add validation of material compatibility
- Support property-based material modifications
- Improve handling of material slots and instances