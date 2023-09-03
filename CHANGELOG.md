# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Added estimated performance stats to PhysBones Remover window.
- Added update notification for pre-release version. (Only when using pre-release version)

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
