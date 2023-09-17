# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[Unreleased]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.3...HEAD
[1.13.3]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.2...v1.13.3
[1.13.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.1...v1.13.2
[1.13.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.13.0...v1.13.1
[1.13.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.12.1...v1.13.0
[1.12.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.12.0...v1.12.1
[1.12.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.11.0...v1.12.0
[1.11.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.10.1...v1.11.0
[1.10.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.10.0...v1.10.1
[1.10.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.9.1...v1.10.0
[1.9.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.9.0...v1.9.1
[1.9.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.8.1...v1.9.0
[1.8.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.8.0...v1.8.1
[1.8.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.7.0...v1.8.0
[1.7.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.6...v1.7.0
[1.6.6]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.5...v1.6.6
[1.6.5]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.4...v1.6.5
[1.6.4]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.3...v1.6.4
[1.6.3]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.2...v1.6.3
[1.6.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.1...v1.6.2
[1.6.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.6.0...v1.6.1
[1.6.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.2...v1.6.0
[1.5.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.1...v1.5.2
[1.5.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.5.0...v1.5.1
[1.5.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.4.1...v1.5.0
[1.4.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.4.0...v1.4.1
[1.4.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.3.0...v1.4.0
[1.3.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.2.1...v1.3.0
[1.2.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.2...v1.2.0
[1.1.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/kurotu/VRCQuestTools/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.7.0...v1.0.0
[0.7.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.2...v0.6.0
[0.5.2]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.1...v0.5.2
[0.5.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.5.0...v0.5.1
[0.5.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.4.1...v0.5.0
[0.4.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.2.1...v0.3.0
[0.2.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.2.0...v0.2.1
[0.2.0]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.2...v0.2.0
[0.1.2]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/kurotu/VRCQuestTools/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/kurotu/VRCQuestTools/releases/tag/v0.1.0
