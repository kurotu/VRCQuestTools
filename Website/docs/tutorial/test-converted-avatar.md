---
sidebar_position: 3
---

# Test Converted Avatar

Now your avatar is cross-platform, but it may not be perfect. Let's test.

## Test Quest Avatars in PC

For complete testing, you need Quest. Even if you have Quest, it would be hard because you need to go back and forth between PC and Quest.
However, in most cases you can test Quest avatars in PC because they use limited shaders and components which are fully supported in PC platform.

In VRCSDK3, there is a feature called [Local Avatar Testing](https://docs.vrchat.com/docs/avatars-30#local-avatar-testing). It allows you to test avatars in PC without uploading.
By using local testing, you can quickly test Quest avatars in PC.

## Build Offline Avatars

To build offline testing avatars, follow the steps below.

1. Select **File** > **Build Settings** from the menu bar.
2. Select **PC, Mac & Linux Standalone** from the platform list.
3. Click **Switch Platform**.
4. Select **VRChat SDK** > **Show Control Panel** from the menu bar.
5. Select **Builder** tab.
6. Select an avatar to test from the scene.
7. Click **Build & Test**.

After building, you can test the avatar in PC.

## Test Offline Avatars in VRChat

To use offline testing avatars in VRChat, follow the steps below.

1. Launch VRChat in PC.
2. Open the Main Manu and show **Avatars** tab.
3. Select **Other** section.
4. Change into the avatar, `SDK: <Name in the Unity Scene>`.

![Other Category](/img/other_avatars.png)

Now you can see the converted avatar in VRChat.
In general, you need to test below.
- Facial expressions
- Gimmicks from FX layer.
- Transparent/Cutout appearance

If you are testing in desktop mode, see tables below to control avatars by keyboard.

| Hand Gesture | Left Hand | Right Hand |
|---|---|---|
| Idle | <kbd>Left Shift</kbd> + <kbd>F1</kbd> | <kbd>Right Shift</kbd> + <kbd>F1</kbd> |
| Fist | <kbd>Left Shift</kbd> + <kbd>F2</kbd> | <kbd>Right Shift</kbd> + <kbd>F2</kbd> |
| Open | <kbd>Left Shift</kbd> + <kbd>F3</kbd> | <kbd>Right Shift</kbd> + <kbd>F3</kbd> |
| Point | <kbd>Left Shift</kbd> + <kbd>F4</kbd> | <kbd>Right Shift</kbd> + <kbd>F4</kbd> |
| Peace | <kbd>Left Shift</kbd> + <kbd>F5</kbd> | <kbd>Right Shift</kbd> + <kbd>F5</kbd> |
| RockNRoll | <kbd>Left Shift</kbd> + <kbd>F6</kbd> | <kbd>Right Shift</kbd> + <kbd>F6</kbd> |
| Gun | <kbd>Left Shift</kbd> + <kbd>F7</kbd> | <kbd>Right Shift</kbd> + <kbd>F7</kbd> |
| Thumbs Up | <kbd>Left Shift</kbd> + <kbd>F8</kbd> | <kbd>Right Shift</kbd> + <kbd>F8</kbd> |

| Action | Key |
|---|---|
| Action Menu | <kbd>R</kbd> |
| Jump | <kbd>Space</kbd> |
| Crouch | <kbd>C</kbd> |
| Crawl/Go Prone | <kbd>Z</kbd> |
