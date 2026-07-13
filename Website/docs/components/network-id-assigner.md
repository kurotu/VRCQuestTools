---
slug: /references/components/network-id-assigner
---

# Network ID Assigner

A component which assigns network IDs to components such as PhysBones when the avatar is built.
Attach it to the avatar's root object.

To synchronize PhysBone swaying between PC and Mobile, the PhysBones must have the same network IDs on both avatars.
This component decides the IDs based on the hierarchy path from the avatar root, so the same IDs are assigned to the PC and Mobile avatars as long as they have the same structure.

## Usage

1. Attach this component to the avatar's root object.
2. Upload both the PC and Mobile avatars again.

There are no settings.

## Notes

When "Assign Network IDs" of [VQT Avatar Converter Settings](./avatar-converter-settings.md) is enabled, you don't need to add this component separately.
