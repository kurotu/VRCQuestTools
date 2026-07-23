# VRCQuestTools

Unity editor extension for converting VRChat PC avatars to Android (Quest/PICO) platform.

## Commands

### Unity Editor Operations (via uloop skills)

**Preferred** for compilation and testing when Unity Editor is running. Uses [unity-cli-loop](https://github.com/hatayama/unity-cli-loop)-based skills:

> First run `uloop-launch` to start Unity, then run other `uloop-*` commands after the editor is ready.

| Skill | Purpose |
|-------|---------|
| `uloop-launch` | Launch Unity project with matching Editor version |
| `uloop-compile` | Compile the project and report errors/warnings |
| `uloop-run-tests` | Run Unity Test Runner (EditMode/PlayMode) and get results |
| `uloop-get-logs` | Retrieve Unity Console logs |
| `uloop-clear-console` | Clear Unity Console before a fresh run |

Note: When you need to execute `npm` and `npx`, use `pnpm` and `pnpm dlx` instead.

### Lint

| Command | Description |
|---------|-------------|
| `pwsh scripts/lint.ps1` | Lint on Windows (builds .sln then checks VRCQuestTools*.csproj) |
| `bash scripts/lint.sh` | Lint on Linux CI |

### Fallback (no Unity Editor)

> `dotnet build VRCQuestTools.sln` — build only; requires Unity-generated .sln/.csproj files (Unity 2022.3.22f1). Tests are not runnable without Unity; CI uses `game-ci/unity-test-runner`.

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
- **Line endings:** Preserve each existing file's current line endings when editing. Only newly added files should use LF (`\n`). Do not bulk-convert existing CRLF files to LF because this repository still contains many CRLF files and mass normalization creates large, noisy diffs.
- **XML docs:** `///` on all public/internal members with `<summary>`, `<param>`, `<returns>`
- **StyleCop:** Enforced via analyzers with project-specific suppressions (SA1101, SA1200, SA1307, SA1401, SA1407, SA1513, SA1633, SA1639)
- **Conditional compilation:** `#if VQT_HAS_NDMF` guards for optional NDMF features

## Testing

- **Implementation changes:** Always run relevant tests after implementing code changes.
- **Framework:** NUnit (Unity Test Runner, EditMode tests)
- **Location:** `Assets/VRCQuestTools-Tests/Editor/`
- **Naming:** `<TargetClass>Tests` (e.g., `StandardMaterialTests`, `AvatarDynamicsTests`)
- **Fixtures:** Test data in `Assets/VRCQuestTools-Tests/Fixtures/` (materials, prefabs, textures, scenes, JSON)
- **CI matrix:** Tests run against multiple VRChat SDK versions and optional dependency combinations (NDMF, Modular Avatar, lilToon, AAO)

## Documentation Website (`Website/`)

Guidelines for updating the Docusaurus documentation site:

- **Audience:** The website is for end users. Keep technical explanations minimal; write plain, easy-to-understand text.
- **Workflow:** Update the Japanese docs first, then port the changes to the English pages. English is the default locale (`Website/docs/`); Japanese translations live in `Website/i18n/ja/docusaurus-plugin-content-docs/current/` and must mirror the same file paths, frontmatter (slugs), and explicit heading anchors (`{#id}`).
- **Writing skills:** Always apply the `japanese-tech-writing` skill when writing or revising Japanese pages. On pages meant to be read from start to finish (intro, getting-started), additionally apply the `cognitive-rhythm-writing` skill on top of it, but keep the procedural skeleton (headings, numbered steps, anchors, admonitions) intact and put the pacing in the connecting prose; each page must still work for readers who land on it directly. Reference pages (components, troubleshooting, menu-reference) must stay scannable, so use `japanese-tech-writing` alone there. The English pages port the resulting content and structure in plain natural English; do not translate the Japanese rhythm devices word for word.
- **UI labels:** Quote UI strings from the .po files (`Editor/I18n/ja-JP.po` for Japanese, `en-US.po` for English). Do not invent labels.
- **Source of truth:** Verify every statement against the source code (components, inspectors, menus, i18n messages) so the manual stays trustworthy. Do not document behavior you have not confirmed.
- **Ordering:** List components in dictionary order. Component pages have no `sidebar_position` (the autogenerated sidebar sorts by file name); keep tables and lists sorted the same way.
- **URL compatibility:** Do not break URLs referenced by `[HelpURL]` attributes and `package.json`: `/docs/references/components/*`, `/docs/changelog`, `/docs/tutorial/set-up-environment`. They are pinned with explicit `slug:` frontmatter. The `?lang=auto` redirect in `src/theme/Root.tsx` must keep working.
- **Structure notes:** `docs/changelog.md` and its ja counterpart are dummy files replaced by CI with `CHANGELOG.md` / `CHANGELOG_JP.md`. Category labels in `_category_.json` are English; their Japanese translations go in `i18n/ja/docusaurus-plugin-content-docs/current.json`. Mark missing screenshots with a "Screenshot placeholder" / 「スクリーンショット準備中」 info admonition.
- **Verification:** Run `cd Website && pnpm run build` and make sure both locales build without broken links or anchors.
- **Deployment:** GitHub Pages builds the site from the `latest-docs` branch and overlays only `docs/` and `i18n/ja/.../current/` from master (`.github/workflows/deploy-pages.yml`). Changes to `docusaurus.config.js`, `src/`, `sidebars.ts`, or `static/` do not go live until `latest-docs` is updated. Versioned docs snapshots are created at release time with `docs:version`.
- **New components:** When adding a component, create its reference page at `/docs/references/components/<kebab-case-name>` (en + ja) and set the matching `[HelpURL]` attribute on the component class (with the `?lang=auto` suffix).
- **Terminology:** The docs call the Android and iOS versions of VRChat collectively "Mobile" (defined in intro.md). Match the current UI wording (Mobile/PC); do not write "Quest" except for device names.
- **Links:** Use file-relative Markdown links (`./page.md`) so they resolve within each locale. Give any heading that is linked from elsewhere an explicit ID (`{#id}`), because auto-generated anchors differ between Japanese and English headings.
- **Images:** Screenshots live in `Website/static/img/` and are shared by both locales. Reuse existing images only when they still match the current UI; otherwise use a screenshot placeholder.

## Dependencies (Optional/Conditional)

The tool supports several optional VRChat community packages via conditional compilation:

| Package | Compile Symbol | Purpose |
|---------|---------------|---------|
| [NDMF](https://github.com/bdunderscore/ndmf) | `VQT_HAS_NDMF` | Non-destructive avatar processing |
| [Modular Avatar](https://modular-avatar.nadena.dev/) | — | Avatar component system |
| [lilToon](https://lilxyzw.github.io/lilToon/) | — | Toon shader conversion |
| [Avatar Optimizer (AAO)](https://vpm.anatawa12.com/) | — | Avatar optimization (MergePhysBone via reflection) |

## Gotchas

- **Leave `Packages/*.json` and `ProjectSettings/*` alone:** Unity/VPM rewrites these as a side effect of normal operations (opening the project, compiling, resolving packages). Do not stage/commit them unless a PR explicitly intends to update Unity/VPM project configuration, and do not `git checkout`/revert them either — they are legitimate local state, not stray edits to clean up. Simply don't touch them.
- **Cannot build/test without Unity:** The .sln and .csproj files are generated by Unity. Lint (`scripts/lint.ps1`) requires them to exist first.
- **`Graphics.Blit` overrides `_MainTex`:** `Graphics.Blit(source, dest, material)` always overrides `_MainTex` on the material. Textures set via `material.SetTexture("_MainTex", tex)` before `Blit` are overridden.
- **AAO reflection:** AAO MergePhysBone is detected via reflection (`AAOMergePhysBoneReflectionInfo.Default`). When AAO is not installed, `Default` is null — always null-check.
- **CI GPU limitations:** Linux CI has limited GPU support: `SystemInfo.supportsAsyncGPUReadback` returns true but `AsyncGPUReadback` fails for RenderTexture assets. Tests need `LogAssert.ignoreFailingMessages` and platform-specific expected values.
- **Version in two places:** Version is in both `package.json` and `VRCQuestTools.cs` (`Version` constant). Use `scripts/bump.sh` to keep them in sync.
- **Localization:** i18n uses `.po` files in `Editor/I18n/`. Crowdin manages translations (`crowdin.yml`). English is the fallback.
