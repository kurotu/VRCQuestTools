# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- [NDMF] Add `NDMF Phase` option to `VQT Mesh Flipper` component.
    - Before Decimation
    - After Decimation
- [NDMF] NDMF Preview for `VQT Mesh Flipper`.

## [2.8.2] - 2025-04-01

### Fixed
- MatCap Lit conversion not working.

## [2.8.1] - 2025-03-31

### Changed
- Allow to directly set target materials in the additional material conversion settings of `VQT Avatar Converter Settings`.
- Stopped multiplexed texture generation process and changed it to be processed sequentially as in version 2.7.2.

### Fixed
- `VQT Menu Icon Resizer` was added in manual conversion when NDMF was not installed.
- Error when converting VirtualLens2 materials.
- [NDMF] Error when there was no mesh for `VQT Mesh Flipper` to target.
- [NDMF] Error when the same replacement material was set multiple times in `VQT Material Swap`.

## [2.8.0] - 2025-03-16

### Added
- Feature to configure `VQT Menu Icon Resizer` during manual conversion when `VQT Avatar Converter Settings` is set to compress menu icons.
- Cache to accelerate the texture generation process.
- VRCQuestTools Settings screen to Unity preferences.
- [NDMF] Support offline testing on Android in VQT Avatar Builder.

### Changed
- Accelerated the texture generation process.
- Accelerated the texture generation process for lilToon materials.
- Accelerated the type resolution for optional dependencies.
- [NDMF] Disabled "Read/Write Enabled" for generated textures.

### Fixed
- [NDMF] Fixed an issue where an exception would occur when building an avatar for a build target not supported by VRCSDK.

## [2.7.2] - 2025-03-02

### Added
- Support to convert Animator Override Controllers.

### Fixed
- Fixed an issue where the original Blend Tree was deleted when converting a Blend Tree embedded in an Animator Controller.
- Fixed an issue where a converted Blend Tree embedded in an Animator Controller was not saved properly afterward.

## [2.7.1] - 2025-02-26

### Changed
- Reverted the texture generation process in Unity 2019 to be the same as in versions prior to 2.6.2.

## [2.7.0] - 2025-02-25

### Added
- [NDMF] Feature to control operational area of `VQT Mesh Flipper` by mask textures.
- [NDMF] (Experimental) `VQT Material Swap` component to swap materials on Android build. (by @Amoenus)
- [NDMF] `VQT Menu Icon Resizer` component to resize menu icons for Android build.
- [NDMF] Ability to track when materials subject to Additional Material Conversion Settings have been modified by other plugins.
- Error for unsupported materials in Material Replacement settings.
- Feature to remove extra material slots in the `VQT Avatar Converter Settings` component.
- Feature to compress menu icons in the `VQT Avatar Converter Settings` component.
- Feature to convert materials which use lilToon custom shaders. (as well as regular lilToon materials)
- Support for local-only contact senders.

### Changed
- Accelerated the texture generation process.
- [NDMF] `VQT Mesh Flipper` works before `NDMF Mantis LOD Editor` in the transforming phase.

## [2.6.2] - 2025-01-21

### Fixed
- Failed to convert lilToon FakeShadow materials.

## [2.6.1] - 2025-01-19

### Added
- Additional information to logs when incorrect material property type is detected in lilToon baking.

### Fixed
- Fixed an issue where an error would occur when right-clicking on the hierarchy without selecting a GameObject.
- Fixed an issue where nested Blend Trees were not converted properly.
- Invalid localization files. (by @Amoenus)

## [2.6.0] - 2024-12-14

### Added
- Support for local-only contact receivers.
- Buttons to back to the scene in warning about prefab stage.
- [NDMF] (Experimental) `VQT Mesh Flipper` component to flip the mesh of the avatar.
- [NDMF] `[NDMF] Manual Bake with Android Settings` menu to the right-click menu of the avatar.
- [NDMF] Feature to select an avatar to upload in the `VQT Avatar Builder`.
- [NDMF] Messages and a dialog to suggest to use `MA Convert Constraints` component and non-destructive conversion.

### Changed
- [NDMF] Check texture format after TexTransTool.
- [NDMF] Change the inspector of `VQT Avatar Converter Settings` to prioritize non-destructive conversion.
- [NDMF] Convert avatars after lilycalInventory in the transforming phase.

### Fixed
- Fallback for untranslated phrases not working.
- Fixed an issue where inactive Avatar Dynamics components were not selected by default in `VQT Avatar Converter Settings`.
- [NDMF] Fixed an issue where unsupported components were removed when using Android build target even if the avatar was not converted.

## [2.5.5] - 2024-11-16

### Fixed
- [NDMF] ArgumentNullException on play mode when the avatar has a newly created Pipeline Manager.
- [NDMF] Fixed an issue where the plugin might fail to load textures which were generated by other plugins.

## [2.5.4] - 2024-10-09

### Changed
- Use same UI layout for Avatar Dynamics Selector and PhysBones Remover.

### Fixed
- Missing column for root transform in the Avatar Dynamics Selector.
- [NDMF] Error in the texture format check when the avatar has no expression menu.

## [2.5.3] - 2024-10-06

### Changed
- Rewrite the duplication process of animator controllers.
  - This makes better duplication for complicated animator controllers.

### Fixed
- Conversion error of FX layers which use GoGo Loco.

## [2.5.2] - 2024-09-25

### Fixed
- [NDMF] Stack overflow error when using recursive expressions menu.

## [2.5.1] - 2024-09-23

### Changed
- [NDMF] Speed up texture format checking.

### Fixed
- Accidentally modified original animator controllers when they have wrong object references.
- [NDMF] Failed to convert animator controllers which have sub state machines in some cases.

## [2.5.0] - 2024-09-07

### Added
- `VQT Network ID Assigner` works without NDMF.
- Add `NDMF Phase to Convert` option to `VQT Avatar Converter Settings` component.
    - Transforming
    - Optimizing
- New localization mechanism based on .po files.
- Russian (Русский) translation. (by @CoderCoV)
- [NDMF] Warning when an avatar which has `VQT Avatar Converter Settings` doesn't have `VQT Network ID Assigner`.

### Changed
- `Convert Avatar for Android` window attaches `VQT Network ID Assigner` to the avatar.
- [NDMF] Changed the default execution order of the avatar conversion process to match the behavior prior to 2.4.3 (Transforming).

### Removed
- Warning about unassigned Network IDs.

## [2.4.3] - 2024-08-25

### Changed
- Texture generation now uses Unity's imported textures when using other than PNG or JPEG.
- [NDMF] Convert avatars in the optimizing phase for TexTransTool interoperability.

### Fixed
- [NDMF] VQT Avatar Builder might put an error when launching Unity.
- [NDMF] Error when replacing multiple materials with the same one in material conversion settings.
- [NDMF] Texture generation error when a material uses other than PNG or JPEG textures.
- [NDMF] Exception occured in the preview configuration window of NDMF 1.5.0 when reloading scripts. (by @ReinaS-64892)

## [2.4.2] - 2024-08-16

### Removed
- Button to convert Unity constraints to VRChat constraints in `VQT Avatar Converter Settings`.

## [2.4.1] - 2024-08-15

### Added
- Error message that the converter can't perform in the prefab mode.

### Fixed
- [NDMF] Known texture formats were reported as unknown texture formats when using unsupported texture formats for the current platform.

## [2.4.0] - 2024-08-14

### Added
- Support for iOS platform (as well as Android).
- Add warning to suggest converting Unity constraints to VRChat constraints in `VQT Avatar Converter Settings`.
- [NDMF] Feature to set as a fallback avatar in the VQT Avatar Builder.
- [NDMF] Logo to the NDMF Console.

### Changed
- `MA Visible Head Accessory` and `MA World Fixed Object` components are no longer removed when using Modular Avatar 1.9.0 or later.
- [NDMF] Changed to display a warning instead of an error when unknown texture formats are used.
- [NDMF] Changed to not check texture formats on unsupported platforms.

### Fixed
- Compilation error when using VRChat SDK 3.6.2-constraints.3 or later.

## [2.3.5] - 2024-07-27

### Fixed
- Fix `VQT Avatar Converter Settings` not working when Final IK exists.

## [2.3.4] - 2024-07-17

### Fixed
- Fix an issue where `Audio Source` might not be removed when removing unsupported components.

## [2.3.3] - 2024-06-03

### Changed
- Improved error message when failed to convert lilToon material.

### Fixed
- Material animation might not be converted in some cases.

## [2.3.2] - 2024-05-27

### Fixed
- [NDMF] VQT Avatar Builder failed to detect active avatars when multiple scenes are loaded in the hierarchy.
- Automated avatar validation was not working for all avatars when multiple scenes are loaded in the hierarchy.

## [2.3.1] - 2024-05-09

### Fixed
- [NDMF] Fixed an issue where entry transitions for sub state machines were not duplicated correctly when duplicating animator controllers.
- Fix an issue where material animations of sub state machines were not converted.

## [2.3.0] - 2024-05-05

### Added
- [NDMF] `Show Avatar Builder for Android` menu to upload an avatar for Android build target with NDMF.
- [NDMF] Non-destructive avatar conversion with `VQT Avatar Converter Settings` component.
- [NDMF] `VQT Platform Target Settings` component to specify the target platform for `VQT Platform Component Remover` and `VQT Platform GameObject Remover` components.
- [NDMF] Warning message for using unsupported texture formats.
- [NDMF] `VQT Network ID Assigner` component to assign Network IDs to the avatar.

### Changed
- [NDMF] Duplicate meshes when removing vertex colors. Original meshes keep vertex colors.
- Invert the meaning of checkboxes for `VQT Platform Component Remover` and `VQT Platform GameObject Remover` components. Select checkboxes to keep components or objects.

### Removed
- [NDMF] `Build Target` parameter from `VQT Platform Component Remover` and `VQT Platform GameObject Remover` components.

### Fixed
- Small textures were generated as 2x2 instead of 4x4 so they were not properly compressed.

## [2.2.2] - 2024-04-15

### Fixed
- Fixed color adjust mask not applied to the converted texture when using lilToon shader.

## [2.2.1] - 2024-03-29

### Added
- Added description for `VQT Platform Component Remover`, `VQT Platform GameObject Remover` and `VQT Vertex Color Remover` components.

### Fixed
- Fixed an issue where the converter fails when the avatar object name ends with dots.

## [2.2.0] - 2024-02-09

### Added
- Add `VQT Platform GameObject Remover` component to remove game objects depending on the build platform (NDMF is required).
- Add warning when using NDMF-required components but NDMF is not installed.
- Add component icon (Unity 2022).

### Fixed
- Fixed an issue where the converter fails when the avatar object name starts or ends with whitespaces.

## [2.1.2] - 2024-01-29

### Fixed
- Fix an issue where NDMF assembly might fail to be built. (by anatawa12)
- Fix compilation error when using NDMF 1.3.0. (by anatawa12)

## [2.1.1] - 2024-01-11

### Fixed
- Fixed compile error when using NDMF below 1.3.0.

## [2.1.0] - 2024-01-11

### Added
- Add `VQT Platform Component Remover` component to remove components depending on the build platform (NDMF is required).
- Add warning when MatCap Lit conversion setting has no MatCap texture assigned.
- Add help url for each component.

## [2.0.1] - 2024-01-05

### Fixed
- Fixed an issue that could be added to projects with VRChat SDK below 3.3.0.

## [2.0.0] - 2024-01-05

### Added
- Add `VQT Avatar Converter Settings` component to save converter settings to the avatar.
- Add `VQT Converted Avatar` component to indicate that the avatar is converted with VRCQuestTools.
- Add feature to remove unsupported components generated by NDMF at build time.
- Add feature to change material conversion settings for each material.
- Add material conversion settings other than Toon Lit shader.
    - MatCap Lit
    - Material Replacement
- Add **Generate shadow from normal map** option to material conversion settings.
- Add material conversion from Poiyomi Toon shader. Supported features are:
    - Main Color
    - Shading by normal map
    - Emission [0-3]
- Add feature to show game objects in the warning panel for missing components.
- Integrate VRCQuestTools Extra features into VRCQuestTools.
    - FinalIK Support
    - VirtualLens2 Support
- Add feature to remove unsupported Modular Avatar components on avatar conversion.

### Changed
- Require VRChat SDK 3.3.0 or later.
- Change unitypackage's import path to `Packages/com.github.kurotu.vrc-quest-tools` instead of `Assets/KRT/VRCQuestTools`.
- Change UPM import URL to `https://github.com/kurotu/VRCQuestTools.git?path=Packages/com.github.kurotu.vrc-quest-tools`.
- Change output folder of converted avatars to `Assets/VRCQuestToolsOutput` instead of `Assets/KRT/QuestAvatars`.
- Change suffix of converted avatar's object name to ` (Android)` instead of ` (Quest)`.
- Change the order of removing custom components in build process to run before Anatawa12's Avatar Optimizer.
- Show update notification in inspector instead of scene view.
- Change update check interval to 1 day.
- Move **VRCQuestTools** menu into **Tools** menu.
- Rearrange menu items.
- Show a detailed list of unsupported components in **Remove Unsupported Components** menu.

### Removed
- Remove **Check for Updates** menu. Use VCC or inspector to check for updates.
- Remove automated detection for vertex colors. Use **Remove All Vertex Colors** menu instead.

### Fixed
- Fix conversion failure when GameObjects which were assigned to Network IDs are actually missing.
- Fix linear textures are loaded as sRGB when generating textures.
- Fix VRC Spatial Audio Source is not detected as an unsupported component.
- Fix an issue that could cause performance rank estimation to fail.

## [1.14.0] - 2023-12-09

### Added
- Support for Unity 2022. (Still supports 2019)
- Support for texture conversion from material variants.

## [1.13.5] - 2023-12-03

### Fixed
- Fixed the texture generation error when the lilToon main texture is not Texture2D.

## [1.13.4] - 2023-10-12

### Fixed
- Fixed the issue where there may be compile errors when switching platform to Android in VRChat SDK's control panel.

## [1.13.3] - 2023-09-17

### Changed
- Do not show warning when an avatar in the scene is not uploadable for Quest on Android build target in VRCSDK 3.3.0.

### Fixed
- Fixed false detection of vertex color.
- Fixed the issue where avatar cannot be uploaded when VertexColorRemover component exists in VRCSDK 3.3.0.
- Fixed the issue where PhysBone's collision check count does not reflect Endpoint Position and Multi Child Type.

## [1.13.2] - 2023-09-15

### Changed
- Change to use underscores when the default destination folder name contains invalid characters.

### Fixed
- Fixed the issue where conversion fails when the destination folder name contains invalid characters.
- Fixed the issue where conversion fails when the destination folder name ends with a space.

## [1.13.1] - 2023-09-12

### Fixed
- Fixed the issue where performance stats is not displayed when PhysBone's Ignore Transforms contains None.

## [1.13.0] - 2023-09-04

### Added
- Added estimated performance stats to PhysBones Remover window.
- Added update notification for pre-release version. (Only when using pre-release version)
- Added documentation site. (https://kurotu.github.io/VRCQuestTools/)

### Fixed
- Fixed invalid cast error when an original texture is an .asset file.
- Fixed issue where "Emission - Main Color Power" of lilToon is not reflected to converted textures.

## [1.12.1] - 2023-07-02

### Fixed
- Fixed import error of NewtonSoft Json.

## [1.12.0] - 2023-06-30

### Added
- Added "Remove All Vertex Colors" menu to main menu.

### Changed
- Use VPM repository to check for updates instead of GitHub API.
- Check for updates only on edit mode.
- Do not remove vertex color on play mode.
- Do not validate avatars on play mode.

### Removed
- Removed "Auto Remove Vertex Colors" menu. Use "Remove All Vertex Colors" menu to avatar instead.
- Removed redundant logs from vertex color remover.

### Fixed
- Fixed invalid cast error when an original texture is not Texture2D.
- Fixed unnecessary error log when failed to convert an avatar.

## [1.11.0] - 2023-05-22

### Added
- Add description and feature to assign Network IDs to PhysBones. (VRCSDK 3.2.0 or later)
- Remove missing components when building avatar.
- Add changelog button to update notification.
- Add stack trace to the error message when failed to convert avatar.

### Changed
- Deprecate support for VRCSDK2, Legacy VRCSDK3 and Unity 2018.
- Avatar's prefabs are no longer unpacked in conversion.
- Missing components are no longer removed in conversion.
- Changed validation message when missing components are detected.
- **Auto Remove Vertex Colors** setting is saved as `ProjectSettings/VRCQuestToolsSettings.json`.

### Fixed
- Fixed the issue where some prameters are not reflected to converted BlendTrees.

## [1.10.1] - 2023-03-28

### Fixed
- Fixed the issue where alpha parameters of lilToon emission are not reflected to converted textures.
- Fixed failed conversion when material and animation name contain "/".

## [1.10.0] - 2023-03-04

### Added
- (VRCSDK3) Add *Remove Vertex Color from Meshes* option to the converter window.
- Add VertexColorRemover component in order to control removing/restoring vertex color.

### Changed
- (VRCSDK3) Vertex colors are no longer removed automatically. Use VertexColorRemover component instead.

## [1.9.1] - 2023-02-13

### Changed
- Change not to create prefab for converted avatar.

### Fixed
- Fixed the following issues caused by unexpectedly referencing prefabs when deleting referenced objects in an avatar.
   - PhysBones Collision Check Count calculated higher than actual.
   - Increased build size when using Modular Avatar.

## [1.9.0] - 2023-01-21

### Added
- Support for VRChat Package Manager.
- [Experimental] VPM repository: https://kurotu.github.io/VRCQuestTools/index.json
- Add Animator Controller conversion for Merge Animator of Modular Avatar.
- Add emission blend mode of lilToon 1.3.7.
- Add detailed reason to the warning panel when an avatar is not uploadable for Quest on Android build target.

### Changed
- Change updater info to prevent opening Booth link when using as a VPM package.
- Improve log messages to tell VRCSDK is not properly detected.

## [1.8.1] - 2022-09-29

- Existing release.
