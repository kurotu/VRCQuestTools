# Network ID Assigner

This component assigns network IDs to PhysBones at build time.
Unlike the VRChat SDK, the component assigns IDs based on the hash value of the hierarchy path, so the same ID is assigned even if objects are added or removed.

:::info
This component requires Non-Destructive Modular Framework (NDMF).
:::

## NDMF

The VRCQuestTools plugin performs the following processes.

### Generating Phase

Assigns network IDs to PhysBones which don't have network IDs.
