---
name: worktree-setup
description: "Set up a fresh VRCQuestTools clone or git worktree before launching Unity: resolve VPM packages with vrc-get and download astcenc binaries. Use whenever a new worktree or clone was just created (git worktree add, EnterWorktree), before the first uloop-launch in a working copy, or when Packages/com.vrchat.base is missing, Unity fails to open the project, or compilation reports missing VRChat SDK/NDMF types."
---

# New Worktree / Clone Setup

A fresh working copy of VRCQuestTools needs two setup steps before Unity work can start.

## 1. Resolve VPM packages (required before launching Unity)

Some packages under `Packages/` (e.g. `com.vrchat.avatars`, `com.vrchat.base`) are VPM dependencies
resolved into the working copy and are not tracked in git — a fresh worktree starts without them. Check
whether `Packages/com.vrchat.base` exists; if it's missing, run `vrc-get resolve` (see the
`vrc-get` skill) to install all VPM dependencies **before** launching Unity with `uloop-launch`.
Launching Unity first can cause it to fail or generate broken package state.

## 2. Download astcenc binaries (once per clone/worktree)

The `astcenc` CLI binaries used for fast ASTC texture compression are not committed to the
repository; download them once per clone/worktree:

| Command | Description |
|---------|-------------|
| `bash scripts/download-astcenc.sh` | Download astcenc binaries (Windows + Linux) on Linux/macOS/Git Bash |
| `pwsh scripts/download-astcenc.ps1` | Download astcenc binaries (Windows + Linux) on Windows |

Both accept `--platform windows|linux|all` (default `all`) and are idempotent (re-running when
the binaries already match the recorded SHA256 is a no-op). If this step is skipped, VRCQuestTools
still works — texture compression automatically falls back to Unity's built-in compressor — but
the astcenc code path is not exercised and its tests are skipped.
