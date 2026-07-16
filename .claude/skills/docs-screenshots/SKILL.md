---
name: docs-screenshots
description: "Re-capture or resize VRCQuestTools documentation screenshots (Website/static/img/*.png), produced by the Tools/VRCQuestTools/Debug/Screenshots menu, after a Unity Inspector/EditorWindow field, default value, or layout changes, or when adding a brand-new capture target. Use whenever a component gains/loses/renames a field, a default value changes, an Inspector grows or shrinks, an existing screenshot now shows clipped content, a scrollbar, or excess blank space, or a new component/window needs a doc screenshot."
---

# Documentation Screenshot Maintenance

Maintains the screenshots embedded in `Website/docs/` (en) and
`Website/i18n/ja/docusaurus-plugin-content-docs/current/` (ja), captured by the
debug tool at `Assets/VRCQuestTools-DebugUtil/Editor/Screenshots/` and exposed
under the Unity menu `Tools/VRCQuestTools/Debug/Screenshots/...`.

## Why this tool exists

- **Reproducible**: every window/inspector is captured at a fixed size and
  position, so screenshots don't vary run to run.
- **English UI**: capture always forces `DisplayLanguage.English` for the
  duration, regardless of the developer's own language setting, and restores
  it afterward.
- **Realistic sample data**: when nothing is selected, the tool creates a
  temporary GameObject and seeds its fields with plausible, procedurally
  generated data (materials, textures, a skinned mesh with a blend shape, a
  PhysBone/PhysBoneCollider/ContactReceiver) instead of leaving fields empty.
  It never touches a GameObject the developer has hand-selected in the
  Hierarchy.
- **OS-independent capture**: pixels come from Unity's internal GrabPixels
  rendering (`WindowPixelCapture.cs`, via uLoopMCP's
  `InternalEditorUtilityBridge`), not real OS screen pixels, so capture is
  unaffected by Remote Desktop session state or windows overlapping on
  screen.

## When to use this skill

- A component gained, lost, or renamed a serialized field.
- A field's default value changed (the sample screenshot may now show a
  stale or misleading value).
- An Inspector or EditorWindow's layout got taller/shorter/wider (a new
  warning box, an extra foldout section, more/fewer rows in a list, etc.).
- An existing screenshot now shows a scrollbar, clipped text/controls, or a
  noticeably large blank area at the bottom.
- A brand-new component or window needs its own documentation screenshot for
  the first time.

## Key files

| File | Role |
|---|---|
| `ScreenshotMenu.cs` | One `[MenuItem]` per target, plus a private `Capture...` method for any target that needs its own sample-data setup/teardown (materials, textures, avatars); simple targets call the shared generic `CaptureComponent<T>` inline from their `[MenuItem]` method instead. This is where sample data is seeded via a `populate`/`configure` lambda, and where `Capture All` chains every target — **note each `[MenuItem]` method's size/lambda is duplicated in the `Capture All` queue below it; when you change one, change both, or `Capture All` will silently regenerate the stale version.** |
| `ScreenshotSettings.cs` | Fixed `Vector2` size per capture target, plus the shared `CaptureOrigin` and output directory. |
| `SampleContentFactory.cs` | Procedural, in-memory-only sample data helpers (`CreateSampleMaterial`, `CreateSampleTexture`, `CreateSampleSkinnedMeshRenderer`, `CreateAdditionalMaterialConvertSettings`). Reuse these before writing a new one. |
| `AvatarFixtures.cs` | Instantiates the `SimpleCubeAvatar` test fixture prefab as a temporary avatar (`InstantiateSimpleCubeAvatar`), plus a variant with sample PhysBone/PhysBoneCollider/ContactReceiver children (`InstantiateSimpleCubeAvatarWithDynamics`). |
| `ComponentScreenshotCapture.cs` / `WindowScreenshotCapture.cs` | The actual capture plumbing (open, wait a couple of frames, grab pixels, save PNG, clean up). Rarely need touching. |
| `CaptureEnvironmentScope.cs` / `AvatarConverterSettingsFoldoutScope.cs` | `IDisposable` save-force-restore scopes: the former forces English + suppresses the update banner globally, the latter forces one Inspector's persisted foldout state open. Follow this pattern if a new target has similar persisted UI state to manage. |

## Procedure

1. **Identify the affected target(s).** Find the target's file name, its
   `populate`/`configure` lambda in `ScreenshotMenu.cs`, and its size constant
   in `ScreenshotSettings.cs`.

2. **Update sample data if a field needs it.** Add or adjust the
   `populate`/`configure` lambda so the new/changed field shows plausible,
   non-empty data. Two hard rules:
   - Only mutate inside the temp-GameObject fallback path — never overwrite
     a GameObject the developer selected themselves
     (`ComponentScreenshotCapture.Capture`'s `populate` callback already only
     fires on that path; keep it that way).
   - Prefer reusing a `SampleContentFactory` helper. If you need a new kind
     of sample object, follow the existing convention: build it
     procedurally (no new binary assets in the repo), mark temporary
     GameObjects `HideFlags.HideInHierarchy | HideFlags.DontSave`, and have
     the caller destroy everything it created in the capture's `onDone`
     callback.

3. **Handle collapsed/foldout fields.** If the field lives behind a
   collapsed foldout, it won't show up in the screenshot just because you
   populated it — check which of these three patterns the target's Editor
   class uses, and handle it before capture:
   - **`SerializedProperty.isExpanded`** (default array/list drawn via
     `EditorGUILayout.PropertyField`): force it open with
     `new SerializedObject(component).FindProperty("fieldName").isExpanded = true;`
     then `ApplyModifiedProperties()` (see `CaptureMaterialSwap` in
     `ScreenshotMenu.cs` for a working example).
   - **A local bool field on the Editor class itself** (not a nested
     `ScriptableSingleton`): check its default value first — if it already
     defaults to `true`, nothing to do.
   - **A `ScriptableSingleton`-backed persisted state** (survives across the
     whole Editor session — this is more common than it looks: e.g. both
     `AvatarConverterSettingsEditorState` and `MaterialConversionSettingsEditor`'s
     private nested `EditorState` are `ScriptableSingleton`s, even though the
     latter's `foldOutAdditionalMaterialSettings` happens to default `true`).
     This is the developer's own real, currently-open-or-closed Inspector
     state — never just flip it and leave it. Add a dedicated
     save/force/restore `IDisposable` scope modeled on
     `AvatarConverterSettingsFoldoutScope.cs`, and dispose it in the
     capture's `onDone` so the developer's own UI state is restored
     afterward. (If the field you need already defaults `true` and you're
     not forcing anything else on that Editor, you can skip adding a scope —
     but if the developer ever collapses it during their own work, the next
     capture will silently show it collapsed until someone notices.)

4. **Compile.** Use the `uloop-compile` skill (force recompile). Expect 0
   errors. A couple of pre-existing warnings unrelated to this code are
   normal — don't chase those, but do make sure no *new* warnings appear.

5. **Capture just the changed target(s) first.** Find the exact menu path in
   the `[MenuItem(Root + "...")]` attribute for that target in
   `ScreenshotMenu.cs` — component targets live under a `Components/`
   submenu (e.g. `"Components/Material Swap"`), windows are directly under
   the root; note the menu label doesn't always match the output PNG file
   name (e.g. `"Setup Avatar for Mobile Window"` writes `convert-avatar.png`
   — check the method body for the actual file name). Trigger it via
   `uloop-execute-dynamic-code`:
   ```csharp
   using UnityEditor;
   EditorApplication.ExecuteMenuItem("Tools/VRCQuestTools/Debug/Screenshots/<menu path from ScreenshotMenu.cs>");
   ```
   Then use `uloop-get-logs` to confirm a `"Saved screenshot: ..."` log with
   no warnings or errors.

6. **Visually inspect the resulting PNG(s).** Check for: clipped
   content/scrollbars (increase the size in `ScreenshotSettings.cs`),
   noticeably excess blank space (decrease it), sample data still showing
   `None`/empty/0-count (revisit the `populate` lambda), and any leaked
   internal temp-object name (rename the temp GameObject in the `populate`
   lambda before it's used in an ObjectField — see how
   `MenuCapturePlatformComponentRemover`'s lambda renames its temp
   GameObject to `"Hair"` before the field is shown).

   **Delegate this inspection step to a low-cost subagent** (e.g. the
   `Agent` tool with `model: "haiku"`) rather than reading every PNG
   directly in the main conversation — visually checking a batch of
   screenshots consumes a large number of image tokens and doesn't need a
   top-tier model. Have the subagent report back a pass/fail plus a short
   description of any problem per image; only pull an image into the main
   context yourself if you need to make a judgment call the subagent
   flagged as ambiguous.

7. **Iterate** steps 2–6 until every affected screenshot looks right.

8. **Run a full regression pass.** Trigger `Tools/VRCQuestTools/Debug/Screenshots/Capture All`
   once everything looks right individually. Unaffected screenshots are
   byte-for-byte deterministic across runs — if `git status`/`git diff`
   shows a PNG changing that you didn't intend to touch, something else
   regressed; investigate before proceeding.

9. **Verify the docs site still builds**: `cd Website && pnpm run build`
   (both `en` and `ja` locales must succeed with no broken links).

10. **Stage only the intended files and commit.** Exclude `Packages/*.json`,
    `ProjectSettings/*`, and any unrelated scratch files/directories that
    may be sitting in the working tree — these are Unity/VPM-managed and
    should only be committed when a change explicitly intends to touch
    them.

## Known gotchas

- **A capture comes back blank.** Since capture uses Unity's internal
  GrabPixels rendering (not real OS screen pixels), this is no longer an
  environment/focus/RDP issue — treat it as a real bug and check the Unity
  console for an exception during that capture.
- **`PlatformComponentRemoverEditor` auto-rebuilds its list every draw.**
  Its `OnInspectorGUIInternal` calls `TargetComponent.UpdateComponentSettings()`
  unconditionally on every repaint, which resyncs `componentSettings`: it
  drops any entry whose `component` reference is no longer present on the
  GameObject, and appends a fresh `removeOnPC = false, removeOnAndroid =
  false` entry for any sibling component not yet tracked — but an entry
  whose `component` is still present is *kept as-is*, flags and all, so
  edits to an already-tracked entry are not lost across redraws. The
  practical implication for this tool: pre-seed `componentSettings` with
  your desired flags *before* the first draw, right after adding the
  sibling component (see `MenuCapturePlatformComponentRemover`). If you
  only add the sibling and let the first draw run before setting flags,
  `UpdateComponentSettings()` will already have created a fresh
  default-`false` entry for it, which is just extra work to find and edit
  afterward.

## Adding a brand-new capture target

1. Add a `Vector2` size constant in `ScreenshotSettings.cs`.
2. Add a `[MenuItem]` + a private `Capture...` method in `ScreenshotMenu.cs`,
   following the pattern of an existing target of the same kind (component
   vs. window).
3. Add it to the `Capture All` queue in `ScreenshotMenu.cs`.
4. If the doc page doesn't have a screenshot placeholder yet, add one to
   both the English (`Website/docs/...`) and Japanese
   (`Website/i18n/ja/docusaurus-plugin-content-docs/current/...`) pages
   before embedding the image.
