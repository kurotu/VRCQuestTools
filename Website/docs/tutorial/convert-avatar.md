---
sidebar_position: 2
---

# Convert Avatar

Convert your avatar to Android compatible.

First, you should upload your avatar for PC platform. Then convert your avatar and upload it for Android platform.

:::info
Avatars are managed by Blueprint ID in VRChat, and you can upload avatar data for each platform with the same Blueprint ID.
So PC users will see the original avatar and Android users will see the converted avatar.
:::

:::caution
VRoid Studio avatars use a lot of transparent materials, and they are not supported on Android.
VRCQuestTools can't help such avatars, so you need to modify them manually.
:::

## Prerequisite Knowledge

- Basic knowledge of Unity for VRChat
- How to upload your avatar to VRChat

## Upload PC Avatar

Before converting your avatar, you should upload your avatar for PC platform.
But before uploading your avatar for PC platform, you should do additional work.

## Prepare Avatar Converter Settings

Set up settings to convert your avatar to Android compatible.

1. Select your avatar in Hierarchy.
2. Right-click on the avatar and select **VRCQuestTools** > **Convert Avatar for Quest**.
3. **Convert Avatar for Android** window appears.
4. Click **Begin Converter Settings** button of the window.
5. **VQT Avatar Converter Settings** component is added to your avatar and settings appear.

### Assign Network IDs to PhysBones

Network ID is an ID which is used to synchronize PhysBones between players.
Normally, it is automatically assigned by VRChat SDK and you don't need to care about it.
But the number of PhysBones is different between PC and Android, so Network IDs may be changed and not synchronized correctly.

In the previous step 5, **VQT Network ID Assigner** component is attached to your avatar, and you can assign Network IDs required for synchronization between PC and Android.
Upload your avatar for PC platform as usual in this state.

## Avatar Converter Settings

Do settings for the following items.

### Reduce Avatar Dynamics Components

Android avatar has a hard cap for Avatar Dynamics (PhysBones, Colliders and Contacts). If your avatar has too many PhysBones, you should reduce them.

https://docs.vrchat.com/docs/avatar-performance-ranking-system#quest-limits

- 8 PhysBone components
- 64 PhysBones affected transforms
- 16 PhysBones colliders
- 64 PhysBones collider checks
- 16 Avatar Dynamics Contacts

In other words, even if your avatar is ranked as "Very Poor", you must pass "Poor" performance rank in Avatar Dynamics category at least.

1. Expand **Avatar Dynamics Settings** section, then click **Avatar Dynamics Settings** button.
2. **Avatar Dynamics Selector** window appears.
3. Select the checkbox of PhysBones and others you want to keep. Worst estimated performance rank should be within Poor.
4. Click **Apply** button.

## Convert Avatar

:::tip
You can convert your avatar non-destructively if you are using NDMF-based non-destructive tools such as Modular Avatar.
(Reference: [Tutorial: Non-Destructive Conversion](non-destructive-workflow.md))
:::

:::tip
It's good time to create a prefab (or prefab variant) if you are using prefab workflow.
VRCQuestTools keeps references to prefabs after conversion.
And you can reuse the converter settings.
:::

1. Scroll down the settings and click **Convert** button.
2. The converted avatar is created in the same scene.
    - Its name has the suffix ` (Android)`.
    - Generated assets are saved in `Assets/VRCQuestTools/<Avatar's object name>` folder.

:::note
The original avatar is deactivcated after conversion. You can restore it from the inspector.
:::

:::info
VRCQuestTools never optimize avatar performance.
So in most cases, converted avatars would have "Very Poor" performance rank in Android platform.

In Android platform, minimum displayed performance rank of safety can be "Good" or "Poor". This means "Very Poor" avatars are always blocked and replaced with your fallback avatar or imposter unless other people individually change Avatar Display setting (as known as "Show Avatar" functionality).

And smartphone users can't see Very Poor avatars.

References:
- [Quest Limits - Performance Ranks](https://creators.vrchat.com/avatars/avatar-performance-ranking-system/#quest-limits)
- [Avatar Fallback System](https://docs.vrchat.com/docs/avatar-fallback-system)
- [Impostors](https://creators.vrchat.com/avatars/avatar-impostors)
:::


## Switch Platform to Android

To upload your avatar to Android platform, you should switch Unity's target platform to Android.

1. Select **File** > **Build Settings** in the menu bar.
2. Select **Android** in the Platform list.
3. Click **Switch Platform** button.
4. Wait for a while until the process is finished. At first time, it takes a long time.

:::note
Result of the switching platform is saved in the project since Unity 2019. So you don't need to wait for a long time from next time.
:::

:::tip
In some situation, you can use [Unity Accelerator](https://docs.unity3d.com/Manual/UnityAccelerator.html) to speed up the switching platform process.
See [Unity Documentation](https://docs.unity3d.com/Manual/UnityAccelerator.html) for details.
:::

## Upload Android Avatar

Now you can upload the converted avatar to Android platform. Make sure the original avatar and the converted avatar uses the same Blueprint ID in Pipeline Manager.

After uploading, make sure the avatar thumbnail has the green **Mobile** icon.
