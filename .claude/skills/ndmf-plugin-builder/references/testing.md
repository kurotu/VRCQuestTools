# Testing NDMF Plugins (EditMode)

As distributed via UPM, `nadena.dev.ndmf` ships **no test framework or
test-only helpers** -- there is no `TestBase`, no `AvatarBuilder`. (Those
exist only in the project's own GitHub source repo, in folders stripped out
of the distributed package.) You build your own minimal test avatars in code
and drive NDMF's public API directly with Unity Test Framework (NUnit)
EditMode tests.

## Wiring up the test assembly

- Add a `Tests/Editor/<your-package>.tests.asmdef` referencing your
  Runtime/Editor asmdefs, `nadena.dev.ndmf`, `nadena.dev.ndmf.runtime`,
  `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, and (if you touch
  VRChat-specific components) `VRC.SDKBase`/`VRC.SDK3A`. If you get
  `CS0246: type or namespace not found` for a VRChat type despite referencing
  the asmdef, check whether the precompiled DLLs (`VRCSDKBase.dll`,
  `VRCSDK3A.dll`) need to be listed explicitly under `precompiledReferences`
  -- this is required when the asmdef sets `"overrideReferences": true`.
- Add your package name to the **top-level** `"testables"` array in
  `Packages/manifest.json` (create the array if it doesn't exist yet).
  Without this, Unity Test Runner won't discover tests inside a local or
  embedded package at all -- you'll see zero tests reported, not a helpful
  error pointing at the missing entry.

## Driving a pass without a full project build

```csharp
var context = new BuildContext(avatarRoot, null); // null assetRootPath -> NullAssetSaver; AssetSaver.SaveAsset becomes a no-op
context.ActivateExtensionContextRecursive<AnimatorServicesContext>(); // NOT ActivateExtensionContext<T> -- see api-reference.md
YourPass.Instance.RunForTest(context); // or call your internal logic directly (InternalsVisibleTo the test assembly)
context.DeactivateAllExtensionContexts(); // commits virtualized controllers back onto the avatar; read results only after this returns
```

`avatarRoot` must be a plain scene `GameObject`, not a prefab instance --
`BuildContext` rejects prefab instances.

For a full-pipeline sanity check that exercises plugin discovery, ordering,
and every registered pass (not just yours), use
`AvatarProcessor.ProcessAvatar(avatarRoot)` instead of constructing a
`BuildContext` by hand.

## Building a minimal test avatar

A workable minimum: a root `GameObject` with a `VRCAvatarDescriptor` whose
`customizeAnimationLayers = true` and `baseAnimationLayers` is a fully
populated 5-element array (Base / Additive / Gesture / Action / FX -- VRChat
does not auto-populate this from a bare `AddComponent`, you must build it
yourself), plus a child with a `SkinnedMeshRenderer` and whatever test
materials/blend shapes you need.

**Also set `specialAnimationLayers` to a non-null (empty is fine) array.**
Like `baseAnimationLayers`, it's `null` on a bare `AddComponent`. If you drive
`AnimatorServicesContext` (directly or via `[DependsOnContext]`) and leave it
`null`, the test doesn't fail where you'd expect -- it compiles, activates, and
runs your pass fine, then throws `NullReferenceException` inside NDMF's own
`VRChatPlatformAnimatorBindings.CommitControllers` the moment
`context.DeactivateAllExtensionContexts()` tries to commit the virtualized
controllers back onto the avatar (`EditLayers(vrcAvatarDescriptor
.specialAnimationLayers)` indexes into it unconditionally, VRChat platform
only). The stack trace points into NDMF/VRCSDK code, not your test or pass, so
it's easy to mistake for an NDMF bug rather than an incomplete test avatar.

For animation-rewriting tests, build an `AnimatorController` in memory
(`new AnimatorController()`, `.AddLayer("...")`, a state whose `motion` is an
`AnimationClip`), and set object-reference or float curves on it directly
with `AnimationUtility.SetObjectReferenceCurve(...)` /
`AnimationUtility.SetEditorCurve(...)` against an
`EditorCurveBinding.PPtrCurve(path, typeof(SkinnedMeshRenderer), "m_Materials.Array.data[N]")`
-style binding, then assign that controller into the FX slot of
`baseAnimationLayers` before running the pass.

Track every object you create (materials, clips, controllers, the root
GameObject) in a list and `Object.DestroyImmediate(obj, true)` all of them in
`[TearDown]`. EditMode tests run inside the actual open Editor session, so
leaked objects accumulate across test runs and pollute the scene and asset
database for whatever runs next.

## Two easy-to-miss blockers when running tests through Editor tooling

How you compile and run tests is project-specific (see SKILL.md Step 5), but
whatever tooling drives the Editor, two situations reliably block a test run
before it ever reaches your tests -- and both look like mysterious failures
if you don't recognize them:

- **Unsaved scene or prefab changes.** Automation tooling that drives Unity
  Test Runner commonly refuses to start while the open scene has unsaved
  modifications (from you, a previous tool call, anything), failing up front
  with a message about the unsaved/dirty scene; the interactive Test Runner
  window may prompt to save instead. Either way, discard or save the scene
  first (e.g. reload it from disk) rather than treating this as a test
  failure to debug.
- **A domain reload takes a moment to settle after compiling.** If you run
  tests immediately after a compile and get an error saying Unity is busy
  with a domain reload (exact wording depends on the tooling), wait briefly
  and retry rather than assuming the test runner itself is broken.

## After tests pass: verify by hand once, and get a second look

Automated tests only validate behavior against what you *assumed* the API
does. The two gotchas documented in `common-patterns.md` (a lossy-getter
round-trip, and a silently-ignored shader assignment on a Material Variant)
both compiled fine and could easily pass a test that didn't happen to probe
that specific state. Two cheap habits are worth applying to any nontrivial
pass, beyond what automated tests cover:

1. Actually open a real scene with a real avatar, add your settings component,
   run the build (Play Mode, or a manual `AvatarProcessor.ProcessAvatar` call),
   and inspect the result -- a screenshot of the Inspector or Console is cheap
   and catches things assertions don't (e.g. a duplicated section header from
   a `[Header]` colliding with a custom inspector's own heading). If you need
   to see *intermediate* state rather than just the final result -- e.g. to
   pin down which pass in the pipeline introduces a bug -- NDMF 1.14.1+ has a
   single-step build debugger for exactly this; see `references/debugging.md`.
2. Run a code-review pass over the diff before calling the plugin done.
   Reviewing the diff line by line, rather than only re-reading the parts you
   already believe are correct, is what actually catches the kind of
   quiet-by-design Unity API behavior described in `common-patterns.md` --
   it's the review step that surfaces it, not the initial implementation or
   the first test pass.
