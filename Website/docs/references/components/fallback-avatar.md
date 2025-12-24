# VQT Fallback Avatar

Automatically sets this avatar as a fallback avatar after upload if both PC and Android versions meet performance requirements.

## What is a Fallback Avatar?

VRChat's fallback avatar system allows you to designate an avatar as a fallback option. When users have safety settings that hide your avatar, they will see your fallback avatar instead of a generic robot avatar. This provides a better experience for both you and other users.

To use an avatar as a fallback, it must meet VRChat's performance requirements:
- Both PC and Android versions must be rated **Good** or **Excellent**
- The avatar must be uploaded for both platforms

## How to Use

1. Add the **VQT Fallback Avatar** component to the root of your avatar GameObject
2. Upload your avatar for both PC and Android
3. If both versions meet the performance requirements (Good or better), VRCQuestTools will automatically set the avatar as a fallback avatar after upload

:::tip
To ensure your avatar meets the performance requirements, check the performance rating in the VRChat SDK Control Panel before uploading.
:::

:::info
This component is a marker component with no properties. It simply signals to VRCQuestTools that this avatar should be set as a fallback after upload.
:::

## Behavior

- The component checks performance ratings after each upload
- If the performance requirements are not met, a warning will be logged in the Unity console
- The component only processes Android/iOS builds - PC-only uploads are skipped
- If the avatar is already set as a fallback, no changes are made

## Requirements

- VRChat SDK3 - Avatars
- Both PC and Android avatar versions uploaded
- Performance rating of **Good** or **Excellent** on both platforms
