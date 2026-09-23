---
name: changelog
description: "Use this skill whenever you add, rewrite, translate, or review entries in `CHANGELOG.md` or `CHANGELOG_JP.md` — including \"update the changelog\", \"変更履歴に追記\", \"changelog に書いて\", finalizing the `[Unreleased]` section before a release, porting an entry between the English and Japanese files, or checking existing entries for consistent formatting. It defines how UI names and labels are quoted in changelog entries (backticks, quotation marks, or nothing), which is easy to get wrong because the choice depends on whether the text is localized. Read it before writing a changelog line in any language."
---

# Changelog

`CHANGELOG.md` (English) and `CHANGELOG_JP.md` (Japanese) follow Keep a Changelog. New work goes under `## [Unreleased]` in both files, using the headings `Added` / `Changed` / `Fixed` / `Removed` (`追加` / `変更` / `修正` / `削除`). Both files are CRLF; edit lines in place and do not rewrite whole files.

Changelogs are for end users, so the two files are not word-for-word translations of each other. Write each entry so an avatar creator understands what changed on screen, and drop implementation detail that only matters to developers.

When writing or revising Japanese entries, also apply the `japanese-tech-writing` skill. Entries are single bullets, so its rules on paragraphs, headings, and narrative pacing do not apply; the wording rules do: no 中黒 (・) or dashes for enumerations, no empty modifiers, verbs that say what changed rather than bare 「改善」「最適化」, and end-user terms instead of implementation details. Read it before editing `CHANGELOG_JP.md`, not only when a line looks awkward. Two of its formatting rules are overridden here:

- One sentence per line does not apply. An entry that needs a second sentence (a consequence, or what to use instead) keeps it on the same line.
- Parentheses are half-width `( )` in both files, separated from the surrounding words by a space like a backtick term: 設定 (圧縮形式と最大テクスチャサイズ) を反映, astcenc (Windows/Linux). This matches the released sections and the `(実験的機能)` / `(by @user)` markers.

## Quoting principle (all languages)

The editor UI is a mix of stable identifiers (component names, window titles, menus, enum values) and localized text (inspector labels, buttons, dialogs) that differs by UI language. Quote by **whether the text is a stable identifier or localized text**, so a reader in any UI language knows whether to look for the exact string as written or for its translation. This matters for the English changelog too: it is read by users of the Russian and Chinese UI as well as the English one.

- **Backticks** for fixed names that are identical in every UI language, and for anything code-like.
  - Component names as shown in the Add Component menu, always with the `VQT ` prefix: `VQT Avatar Converter Settings`, `VQT Platform Component Remover`. Third-party components keep their own prefix: `MA Convert Constraints`, `AAO Trace and Optimize`.
  - Window titles: `Avatar Dynamics Selector`, `PhysBones Remover`, `Unsupported Components`.
  - Menu items and menu paths: `Setup Avatar for Mobile`, `Tools/VRCQuestTools/Settings`. Keep the `[NDMF]` prefix inside the backticks when it is part of the menu label: `[NDMF] Build and Test for PC with Mobile Settings`.
  - Dropdown values and enum-style options: `Auto`, `No Override`, `ASTC 8x8`.
  - Hard-coded labels that are not localized: `Texture Cache Size (MB)` in Project Settings.
  - File paths, extensions, URLs, code identifiers, and literal strings: `.vqtmesh`, `Assets/VRCQuestToolsOutput`, `InvalidMaterialSwapNullException`, ` (Mobile)`.
- **Quotation marks of the changelog's language** for UI text that is localized, written exactly as it appears in the `.po` file for that language, for quoted text from other tools, and for quoting a word as a word.
  - Setting labels, buttons, dialog choices, and messages of VRCQuestTools: the "Test avatar on PC" button, 「PCでアバターをテスト」ボタン.
  - Message text of other tools quoted verbatim, such as the VRChat SDK's "Copyright ownership agreement" confirmation.
  - A word as a word, such as the unified term "Mobile".
- **No quoting** for names of products or concepts rather than UI elements.
  - Products, shaders, libraries: lilToon, Toon Standard, Modular Avatar, VRChat SDK, NDMF.
  - Unity's own windows and settings: Project Settings, Preferences, Auto Referenced.
  - Shader properties of other tools: Occlusion, Reflection Color, Min Brightness.
  - General concepts and Unity types: Avatar Dynamics, PhysBone, GameObject, RenderTexture.
  - Performance ranks, numbers, and units: Very Poor, 128MB, 1GB.

Do not use bold for UI names. It appears in old (1.x/2.0) entries only; never reintroduce it, and do not reformat released sections retroactively.

To check whether a label is localized, look it up in the `.po` files under `Packages/com.github.kurotu.vrc-quest-tools/Editor/I18n/`. The `msgid` values there are identifier keys shared by every language (for example `NdmfBuildAndTestLabel`), not English text, so:

1. Search `en-US.po` for the English text as a `msgstr`.
2. Note the `msgid` key of that entry.
3. Look up the same key in the target language's file (`ja-JP.po` for Japanese) and use its `msgstr` inside that language's quotation marks.

No hit in `en-US.po` (menus, window titles, `AddComponentMenu` names, enum values, hard-coded labels such as `Texture Cache Size (MB)`) means the label is a fixed name: use backticks with the English text.

## Per-language conventions

| | Japanese (`CHANGELOG_JP.md`) | English (`CHANGELOG.md`) |
|---|---|---|
| Quotation marks for localized UI text | 「」 | Straight double quotes `"..."` |
| Space between a backtick term and adjacent words | Half-width space: `Auto` を追加 | Normal word spacing |
| Space next to punctuation | None: 設定を、`VQT Avatar Converter Settings` 内の | Normal English punctuation |
| Space around quotation marks | None: に「PCでアバターをテスト」ボタンを追加 | Normal word spacing |
| Parentheses | Half-width with spaces: astcenc (Windows/Linux) を使用 | Half-width, normal spacing |

## Examples

```markdown
- [NDMF] Added "Test avatar on PC" button to `VQT Avatar Converter Settings`.
- Added `No Override` option to texture compression format settings.
- Added `Enable Debug Log` menu under `Tools/VRCQuestTools/Settings`.
- Removed the "Animation Override" feature from `VQT Avatar Converter Settings`.
- Unified Android/iOS terminology to "Mobile".
```

```markdown
- [NDMF] `VQT Avatar Converter Settings` に「PCでアバターをテスト」ボタンを追加。
- テクスチャの圧縮形式に `No Override` オプションを追加。
- `Tools/VRCQuestTools/Settings` に `Enable Debug Log` メニューを追加。
- `VQT Avatar Converter Settings` の「アニメーションオーバーライド」機能を削除。
- Android/iOS の表記を「Mobile」に統一。
```

- Line 1: component name in backticks, localized button label in the language's quotation marks.
- Line 2: an enum value stays in backticks even though the sentence calls it an option.
- Line 3: menu path and menu item are both fixed names, so both use backticks.
- Line 4: the removed feature is referred to by its localized inspector label.
- Line 5: a word quoted as a word.

Released sections written before these rules (English 2.x entries put localized labels in backticks and Unity settings in double quotes; 1.x/2.0 entries used bold) are left as they are.
