---
name: vrc-get
description: Install, remove, and manage VRChat VPM packages using the vrc-get CLI. Use this skill whenever the user wants to install or add VPM packages (e.g. NDMF, lilToon, Modular Avatar, Avatar Optimizer, VRChat SDK), search for packages, manage VPM repositories, resolve Unity project dependencies, or run any vrc-get command. Trigger on mentions of package IDs like nadena.dev.ndmf, jp.lilxyzw.liltoon, com.vrchat.avatars, com.anatawa12.avatar-optimizer, VPMパッケージ, vrc-get, or requests to add/update/remove packages in a Unity VRChat project.
---

# vrc-get — VRChat Package Manager CLI

`vrc-get` is an open-source VPM (VRChat Package Manager) client for managing packages in Unity VRChat projects.

## Quick Reference

| Goal | Command |
|------|---------|
| Install latest | `vrc-get install -y <package-id>` |
| Install specific version | `vrc-get install -y <package-id> <version>` |
| Remove a package | `vrc-get remove <package-id>` |
| Upgrade a package | `vrc-get upgrade <package-id>` |
| Resolve all deps | `vrc-get resolve` |
| Show installed packages | `vrc-get info project` |
| Search for a package | `vrc-get search <query>` |
| Add a repository | `vrc-get repo add <url>` |
| List repositories | `vrc-get repo list` |

## Installation Workflow

### Step 1: Add the repository (if not yet added)

Third-party packages need their repository registered first. Check with `vrc-get repo list` — if the vendor isn't listed, add it:

```bash
vrc-get repo add <repository-url>
```

### Step 2: Install the package

```bash
vrc-get install -y <package-id>               # latest stable version
vrc-get install -y <package-id> <version>     # specific version (e.g. 1.5.0)
vrc-get install -y --prerelease <package-id>  # include prerelease builds
```

The `-y` flag skips the interactive confirmation prompt — always include it in scripts or automated contexts.

### Step 3: Verify

```bash
vrc-get info project
```

This shows locked packages (pinned in `vpm-manifest.json`) and other installed packages.

---

## Common Repositories & Packages

### Official VRChat (built-in, no `repo add` needed)
- `com.vrchat.base` — VRChat Base SDK
- `com.vrchat.avatars` — VRChat Avatars SDK

### Nadena (bd_)
Repository: `https://vpm.nadena.dev/vpm.json`
- `nadena.dev.ndmf` — Non-Destructive Modular Framework (NDMF)
- `nadena.dev.modular-avatar` — Modular Avatar

Prerelease channel: `https://vpm.nadena.dev/vpm-prerelease.json`

### lilxyzw
Repository: `https://lilxyzw.github.io/vpm-repos/vpm.json`
- `jp.lilxyzw.liltoon` — lilToon shader

### anatawa12
Repository: `https://vpm.anatawa12.com/vpm.json`
- `com.anatawa12.avatar-optimizer` — Avatar Optimizer (AAO)

### kurotu (VRCQuestTools)
Repository: `https://kurotu.github.io/vpm-repos/vpm.json`
- `com.github.kurotu.vrc-quest-tools` — VRCQuestTools

---

## Tips

- **Version format:** Semantic versioning without `v` prefix — e.g., `1.5.0`, `3.9.0`.
- **"latest":** Simply omit the version argument to get the latest stable release.
- **After `git pull`:** Run `vrc-get resolve` to reinstall all packages defined in `vpm-manifest.json`.
- **Outdated packages:** `vrc-get outdated` lists packages with newer versions available.
- **CI helper script:** This project includes `scripts/vrc-get-install.sh <id> <version|"latest">` which wraps `vrc-get install -y` and handles the `"latest"` string correctly for use in CI matrix jobs.

---

## Example: Install NDMF 1.5.0

```bash
vrc-get repo add https://vpm.nadena.dev/vpm.json
vrc-get install -y nadena.dev.ndmf 1.5.0
vrc-get info project
```

## Example: Install the latest lilToon

```bash
vrc-get repo add https://lilxyzw.github.io/vpm-repos/vpm.json
vrc-get install -y jp.lilxyzw.liltoon
```

## Example: Search and install an unknown package

```bash
vrc-get search "avatar optimizer"
# Find the package ID from results, then:
vrc-get install -y <found-package-id>
```
