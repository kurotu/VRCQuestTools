# Fallback Avatar

This component automatically sets the avatar as a fallback avatar after upload if the avatar meets the performance requirements.

## How It Works

When you upload an avatar with this component attached:

1. The avatar is built and uploaded to VRChat
2. The performance rating for the uploaded platform is calculated
3. If the performance rating is Good or better, the avatar is automatically set as a fallback avatar
4. If the performance rating is lower than Good, a warning is logged but upload continues

## Requirements

For an avatar to be set as a fallback avatar, it must meet VRChat's performance requirements:

- Performance rating must be **Good** or **Excellent** on the platform you're uploading to
- Both PC and Android versions should ideally meet these requirements for full fallback functionality

:::info
According to VRChat's [Fallback Avatar System](https://docs.vrchat.com/docs/avatar-fallback-system), fallback avatars are used when another user's avatar fails to load or is blocked. Setting your avatar as a fallback helps other users have a better experience.
:::

## Usage

1. Add the `VQT Fallback Avatar` component to your avatar root object (the GameObject with the VRC Avatar Descriptor)
2. Upload your avatar using VRCQuestTools Avatar Builder or the standard VRCSDK control panel
3. The component will automatically attempt to set the avatar as fallback if performance requirements are met

## Notes

- This is a marker component with no configurable properties
- The component does not affect your avatar at runtime - it's removed during the build process
- You can still manually control the fallback setting using the toggle in the VRCQuestTools Avatar Builder window
- If your avatar doesn't meet the performance requirements, it will not be set as a fallback, but the upload will still succeed
