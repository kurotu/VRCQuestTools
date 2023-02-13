# 変更履歴

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Add Animator Controller conversion for Merge Animator of Modular Avatar.
- Add emission blend mode of lilToon 1.3.7.
- Add detailed reason to the warning panel when an avatar is not uploadable for Quest on Android build target.

### Changed
- Change updater info to prevent opening Booth link when using as a VPM package.
- Improve log messages to tell VRCSDK is not properly detected.

## [1.8.1] - 2022-09-29

- Existing release.
