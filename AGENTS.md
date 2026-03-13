# VRCQuestTools

Unity editor extension for converting VRChat PC avatars to Android (Quest/PICO) platform.

## Commands

| Command | Description |
|---------|-------------|
| `dotnet build VRCQuestTools.sln` | Build the solution (requires Unity-generated .sln/.csproj) |
| `pwsh scripts/lint.ps1` | Lint on Windows (builds .sln then checks VRCQuestTools*.csproj) |
| `bash scripts/lint.sh` | Lint on Linux CI |

> **Note:** Building and testing require Unity 2022.3.22f1 and Unity-generated .sln/.csproj files. Tests run via Unity Test Runner (NUnit) in CI using `game-ci/unity-test-runner`, not via CLI.

### Website (Documentation)

| Command | Description |
|---------|-------------|
| `cd Website && pnpm install --frozen-lockfile` | Install docs dependencies |
| `cd Website && pnpm run build` | Build documentation site (Docusaurus) |

## Architecture

```
Packages/com.github.kurotu.vrc-quest-tools/   # Main VPM package
  Editor/                                      # Editor-only code (bulk of the project)
    Automators/        # Automated avatar conversion tasks
    I18n/              # Internationalization (en-US, ja-JP, ru-RU .po files)
    Inspector/         # Custom Inspector/PropertyDrawer UI classes
    Models/            # Data models and business logic
      MaterialGenerators/  # Per-shader material conversion
      Unity/               # Unity object wrappers
      VRChat/              # VRChat-specific models (avatar, dynamics)
    NDMF/              # Non-Destructive Modular Framework integration
      Passes/            # NDMF processing passes
      Errors/            # NDMF error reporting
    Services/          # Business logic services
    Utils/             # Utility classes (texture, SDK, animation, etc.)
    ViewModels/        # MVVM ViewModels
    Views/             # Editor windows and dialogs
  Runtime/                                     # Runtime components (on avatars)
    Components/        # MonoBehaviour components (AvatarConverterSettings, etc.)
    Models/            # Runtime data models (BuildTarget, TextureFormat)
  Shader/              # Custom shaders for material baking
    cginc/             # Shared shader includes
  ComputeShader/       # Compute shaders (normal map downsampling)
  Assets/              # Precomputed resources (blank normal maps)

Assets/
  VRCQuestTools-Tests/         # Test suite
    Editor/                    # NUnit editor tests
      Models/                  # Model tests
      Utils/                   # Utility tests
      Services/                # Service tests
    Fixtures/                  # Test data (materials, prefabs, textures, scenes)
  VRCQuestTools-DebugUtil/     # Debug utilities (editor menu)

Website/               # Docusaurus documentation site (pnpm, bilingual en/ja)
scripts/               # CI/automation scripts (bump, lint, release)
```

## Key Files

- `Packages/com.github.kurotu.vrc-quest-tools/package.json` — VPM package metadata, version, dependencies
- `Packages/com.github.kurotu.vrc-quest-tools/Editor/VRCQuestTools.cs` — Main entry point, version constant, export method
- `Packages/com.github.kurotu.vrc-quest-tools/Editor/Models/VRChat/VRChatAvatar.cs` — Core avatar model
- `Packages/com.github.kurotu.vrc-quest-tools/Editor/Utils/TextureUtility.cs` — Texture baking utilities
- `Packages/com.github.kurotu.vrc-quest-tools/Editor/Utils/VRCSDKUtility.cs` — VRChat SDK integration
- `Assets/Default.ruleset` — StyleCop analyzer rule suppressions

## Code Style

- **Namespaces:** `KRT.VRCQuestTools.<Feature>` (e.g., `KRT.VRCQuestTools.Models`, `KRT.VRCQuestTools.Utils`)
- **Access modifiers:** All types are `internal` unless they must be public
- **File headers:** C# source files in the main packages should include a copyright header (`// <copyright file="..." company="kurotu">`); new or modified files must follow this pattern, though some older files may not yet be updated.
- **Indentation:** 4 spaces for C# (`.editorconfig`)
- **XML docs:** `///` on all public/internal members with `<summary>`, `<param>`, `<returns>`
- **StyleCop:** Enforced via analyzers with project-specific suppressions (SA1101, SA1200, SA1307, SA1401, SA1407, SA1513, SA1633, SA1639)
- **Conditional compilation:** `#if VQT_HAS_NDMF` guards for optional NDMF features

## Testing

- **Framework:** NUnit (Unity Test Runner, EditMode tests)
- **Location:** `Assets/VRCQuestTools-Tests/Editor/`
- **Naming:** `<TargetClass>Tests` (e.g., `StandardMaterialTests`, `AvatarDynamicsTests`)
- **Fixtures:** Test data in `Assets/VRCQuestTools-Tests/Fixtures/` (materials, prefabs, textures, scenes, JSON)
- **CI matrix:** Tests run against multiple VRChat SDK versions and optional dependency combinations (NDMF, Modular Avatar, lilToon, AAO)

## Dependencies (Optional/Conditional)

The tool supports several optional VRChat community packages via conditional compilation:

| Package | Compile Symbol | Purpose |
|---------|---------------|---------|
| [NDMF](https://github.com/bdunderscore/ndmf) | `VQT_HAS_NDMF` | Non-destructive avatar processing |
| [Modular Avatar](https://modular-avatar.nadena.dev/) | — | Avatar component system |
| [lilToon](https://lilxyzw.github.io/lilToon/) | — | Toon shader conversion |
| [Avatar Optimizer (AAO)](https://vpm.anatawa12.com/) | — | Avatar optimization (MergePhysBone via reflection) |

## Gotchas

- **Cannot build/test without Unity:** The .sln and .csproj files are generated by Unity. Lint (`scripts/lint.ps1`) requires them to exist first.
- **`Graphics.Blit` overrides `_MainTex`:** `Graphics.Blit(source, dest, material)` always overrides `_MainTex` on the material. Textures set via `material.SetTexture("_MainTex", tex)` before `Blit` are overridden.
- **AAO reflection:** AAO MergePhysBone is detected via reflection (`AAOMergePhysBoneReflectionInfo.Default`). When AAO is not installed, `Default` is null — always null-check.
- **CI GPU limitations:** Linux CI has limited GPU support: `SystemInfo.supportsAsyncGPUReadback` returns true but `AsyncGPUReadback` fails for RenderTexture assets. Tests need `LogAssert.ignoreFailingMessages` and platform-specific expected values.
- **Version in two places:** Version is in both `package.json` and `VRCQuestTools.cs` (`Version` constant). Use `scripts/bump.sh` to keep them in sync.
- **Localization:** i18n uses `.po` files in `Editor/I18n/`. Crowdin manages translations (`crowdin.yml`). English is the fallback.
