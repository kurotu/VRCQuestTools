---
name: worktree-setup
description: "Use this skill FIRST, before anything else, whenever work is about to happen in a brand-new VRCQuestTools checkout — right after `git clone` or `git worktree add`, or when the user mentions a fresh/new worktree, clone, or checkout and wants to launch Unity, run EditMode/PlayMode tests, or build there. A fresh checkout is always missing untracked dependencies; this skill performs the two mandatory setup steps (resolve VPM packages, download astcenc binaries). Also use when a fresh checkout misbehaves in ways that indicate skipped setup: Unity won't open the project or spams missing-package errors in the console, compilation fails with \"type or namespace could not be found\" for VRC/VRCSDK/NDMF, Packages/com.vrchat.base doesn't exist, or all astcenc tests are skipped. Do not use for upgrading package versions, packages-lock.json questions, CI workflow tuning, or explaining how git worktrees work."
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
