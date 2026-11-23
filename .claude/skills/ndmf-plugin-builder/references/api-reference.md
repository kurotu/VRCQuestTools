# NDMF API Reference

Verified against `nadena.dev.ndmf` 1.14.0, with the API names re-checked
against 1.15.0 source. Method names and behavior are
generally stable across versions, but if something here doesn't match what you
see in `Packages/nadena.dev.ndmf` (or wherever the package is vendored in the
project), trust the installed source over this file and re-derive from there.
Several APIs described below were added at different points between 1.3.0 and
1.15.0, not all at once -- see `references/version-compatibility.md` if the
project's installed NDMF might be older than what this file assumes.

Contents:

- **Plugin & Pass registration** -- `Plugin<T>`/`Pass<T>`, phases, ordering
  relative to other plugins
- **BuildContext** -- what's available inside `Execute()`: `Extension<T>`,
  `GetState<T>`, `AssetSaver` (and when it's actually needed),
  `ObjectRegistry`, `ErrorReport`; plus a gotcha about the build-wide
  `StartAssetEditing` scope (why asset-modifying `AssetDatabase` calls like
  `CopyAsset` don't belong in a pass) and persistence timing
- **Animation-safe editing** -- `AnimatorServicesContext`, rewriting object
  and float curves, path remapping when moving/removing objects, unit-test
  activation
- **VRChat build pipeline integration & VRCFury ordering** -- the split
  callback "sandwich", ApplyOnPlay, `ProcessAvatar` phase clamping
- **Package & component conventions** -- `INDMFEditorOnly`, assembly layout,
  destroying settings components

## Plugin & Pass registration

`Plugin<T> where T : Plugin<T>, new()`:

- Override `QualifiedName` (a unique dotted id -- reverse-DNS of your package
  is a safe default, e.g. `"com.example.my-plugin"`) and `DisplayName`.
- Override `protected override void Configure()` -- the only required method.
- Put `[assembly: ExportsPlugin(typeof(YourPlugin))]` at the top of one file in
  the assembly. NDMF discovers plugins this way; there is no separate manual
  registration step.

Inside `Configure()`:

- `InPhase(BuildPhase phase)` returns a `Sequence` and starts one. Phase order
  is `Resolving` -> `Generating` -> `Transforming` -> `Optimizing`; pick the
  phase using the semantics described on the best-practices page (fetched in
  Step 1), not by guessing from the name alone.
- `sequence.Run(YourPass.Instance)` registers a pass (or use
  `sequence.Run("display name", ctx => { ... })` for a short inline pass that
  doesn't need its own class).
- Ordering relative to other plugins:
  - `.AfterPlugin("nadena.dev.modular-avatar")` takes the *qualified name as a
    string*, so it works even when the other plugin's assembly isn't
    referenced or installed at all -- this is a soft dependency and is safe to
    declare unconditionally, even in a project where that other plugin is
    absent.
  - A generic `.AfterPlugin<T>()` overload also exists but requires a hard
    compile-time reference to the other plugin's type. Prefer the string
    overload unless your plugin already depends on that other plugin's types
    for some other reason.
  - Mirror APIs: `.BeforePlugin(...)`, `.AfterPass(...)`, `.WaitFor(...)`.
  - The other qualified name that comes up constantly is Avatar Optimizer's,
    `"com.anatawa12.avatar-optimizer"`, which does its work in Optimizing. An
    Optimizing-phase pass that removes your own components must declare
    `.BeforePlugin("com.anatawa12.avatar-optimizer")` (SKILL.md Step 3,
    item 5 explains why).

`Pass<T> where T : Pass<T>, new()`:

- Singleton instance via `Pass<T>.Instance`.
- Override `protected abstract void Execute(BuildContext context)`.
- `QualifiedName` defaults to the C# type's full name, which is fine for most
  passes.
- Add `[DependsOnContext(typeof(SomeExtensionContext))]` on the class to have
  NDMF activate that extension context before your pass runs and deactivate it
  afterward automatically. This is the normal way to use the animator-services
  layer below -- see that section for the one place this attribute isn't
  enough on its own (unit tests). **Version gate: 1.6.0+** -- see
  `references/version-compatibility.md`.

## BuildContext -- what you get inside Execute()

- `GameObject AvatarRootObject` / `Transform AvatarRootTransform`.
- `T Extension<T>()` -- fetch an active extension context. Throws if the
  context isn't active, so pair it with `[DependsOnContext]` on your pass (or
  explicit activation -- see below).
- `T GetState<T>()` / `GetState<T>(Func<BuildContext, T> init)` -- lazily
  creates (on first call) and returns a single instance of `T`, keyed by type,
  that lives for the whole `BuildContext` (i.e. across every phase/pass of the
  current build). This is the mechanism for passing data between two of your
  own passes in different phases -- prefer it over a lambda capture or a
  static/instance field on your `Plugin`/`Pass`, since a plugin instance can be
  reused across multiple builds. Define your own small state class per plugin
  (not a generic `Dictionary` or similar) so the type itself namespaces the
  state and can't collide with another plugin's call to `GetState`.
- `IAssetSaver AssetSaver` -- explicit persistence for a newly created
  `UnityEngine.Object`. **Version gate: 1.6.0+** for `IAssetSaver` itself and
  the related `SerializationScope`/`context.OpenSerializationScope()` API
  below -- see `references/version-compatibility.md` for what an older
  project has instead. **Usually unnecessary.** Once all passes have run,
  `BuildContext.Finish()` walks every object transitively reachable from the
  avatar root (component references, material texture slots, animation
  object-reference curves, animator-graph structure) and persists anything
  not already persistent, through the same container machinery `SaveAsset`
  itself uses -- so an object you clone and wire into a live reference or
  animation curve (the common-patterns.md recipe) gets saved automatically
  with no extra call. Call `AssetSaver.SaveAsset(newObject)` yourself only
  when:
  - the object must already register as persistent (`EditorUtility.IsPersistent`)
    *before* the build finishes -- unlike the raw-file-I/O case in the gotcha
    below, `SaveAsset` doesn't hit the deferred-reimport problem: it calls
    `AssetDatabase.AddObjectToAsset` into an already-created container asset,
    a direct API mutation of the in-memory asset graph, not a file write that
    Unity has to notice and reimport, so `IsPersistent`/`GetAssetPath` become
    valid right away. The on-disk flush is still deferred to `Serialize()`'s
    checkpoint, and that *does* block same-session path-based loads --
    `AssetDatabase.LoadAssetAtPath` for that path still returns `null` right
    after `SaveAsset` returns, even though `GetAssetPath` already reports a
    non-empty path (true both inside the build-wide `StartAssetEditing`
    scope, via `AvatarProcessor.ManualProcessAvatar`, and without it, via
    Play Mode entry). If your pass needs to read the object back right
    away, keep using the object reference you already hold; don't round-trip
    it through `GetAssetPath` + `LoadAssetAtPath`;
  - it must ship in the build output but is intentionally not reachable from
    the avatar root by design (rare); or
  - it's reachable only through an edge the traversal doesn't follow for
    performance reasons -- e.g. an `AnimationEvent`'s object parameter, or a
    `Material`'s shader/parent reference (only its texture slots are walked).

  Don't call it reflexively "just in case": an object that never ends up
  wired to anything the avatar actually uses is a wiring bug, and `SaveAsset`
  doesn't fix that bug -- it just makes the orphan also persist as inert,
  unused data in the asset container.
- `ObjectRegistry` -- call the static
  `ObjectRegistry.RegisterReplacedObject(oldObj, newObj)` whenever you swap one
  asset for another, so NDMF, other plugins, and the error-reporting UI can
  trace the replacement back to the object the user actually authored.
  **Version gate: 1.3.0+** -- see `references/version-compatibility.md`.
- `ErrorReport` -- report user-facing problems via
  `ErrorReport.ReportError(localizer, severity, key, ...args)` instead of
  throwing. **Version gate: 1.3.0+** -- see
  `references/version-compatibility.md`.
  - **Why not throw:** NDMF catches an exception from a pass body
    (`BuildContext.ExecutePassBody`), reports it as an `InternalError` with a
    raw stack trace, and moves on to the next pass. So the rest of your pass
    never runs, including cleanup such as destroying your settings
    component (unless it's in a `finally`), and the upload is blocked
    anyway. Exceptions from an extension context's `OnActivate`/`OnDeactivate`
    are different: they're rethrown and do abort the build.
  - **Severity** (`ErrorSeverity`) decides what the user can do next.
    `Information` and `NonFatal` show in the report but don't block anything.
    `Error` and `InternalError` make `BuildContext.Successful` false, and the
    VRChat build hook then blocks the upload. Use `NonFatal` for "skipped this
    one material, the rest is fine" and `Error` for a configuration the user
    has to fix before uploading.
  - **`Localizer`** (`nadena.dev.ndmf.localization`). Create one per plugin and
    keep it in a static field. The constructor that takes a lookup function is
    the simplest:

    ```csharp
    using System;
    using System.Collections.Generic;
    using nadena.dev.ndmf.localization;

    internal static class MyLocalization
    {
        private static readonly Dictionary<string, string> En = new Dictionary<string, string>
        {
            ["MyPlugin:ShaderMissing"] = "Shader {0} was not found",
            ["MyPlugin:ShaderMissing:description"] = "Material {1} was left unconverted.",
            ["MyPlugin:ShaderMissing:hint"] = "Import the shader package, then rebuild.",
        };

        public static readonly Localizer L = new Localizer("en-US",
            () => new List<(string, Func<string, string>)>
            {
                ("en-US", key => En.TryGetValue(key, out var s) ? s : null),
            });
    }

    // in a pass:
    ErrorReport.ReportError(MyLocalization.L, ErrorSeverity.NonFatal,
        "MyPlugin:ShaderMissing", "MyToonOutlineVariant", material);
    ```

    The other constructor, `Localizer(string, Func<List<LocalizationAsset>>)`,
    loads Unity `LocalizationAsset`s (`.po` files) when you ship translations.
    `key` is the title; the details and hint come from `key + ":description"`
    and `key + ":hint"`, and both are optional. `{0}`, `{1}`, ... are filled
    from `args` in order. A `UnityEngine.Object` in `args` also becomes a
    clickable reference in the report. A key with no translation is shown as
    `<key>` and nothing else complains, so check the report text once in the
    Editor.
- `context.VRChatAvatarDescriptor()` (extension method from the
  `nadena.dev.ndmf.vrchat` assembly) is the current way to get the avatar's
  `VRCAvatarDescriptor` -- the older `context.AvatarDescriptor` property is
  obsolete.

### Gotcha: the whole build is meant to run inside `AssetDatabase.StartAssetEditing`

**Intended behavior:** NDMF means for the entire pass pipeline to run inside a
single `AssetDatabase.StartAssetEditing()` / `StopAssetEditing()` pair.

**What NDMF actually does (checked in 1.14.8):** only the manual bake
(NDMF's "Manual bake avatar" menu item, i.e.
`AvatarProcessor.ManualProcessAvatar`) wraps the whole pipeline that way.
Every other entry point -- Play Mode entry, the VRChat SDK build hook, a
direct `AvatarProcessor.ProcessAvatar` call -- runs passes *outside* any such
scope, and only batches inside NDMF's own saving code (`AssetSaver`,
`context.OpenSerializationScope()`, the animator-services commit). The NDMF
maintainers treat that as a bug in those paths, not as a second supported
mode
([bdunderscore/ndmf#816](https://github.com/bdunderscore/ndmf/issues/816),
[#817](https://github.com/bdunderscore/ndmf/issues/817)). Both issues were
still open at the time of writing and closing them may affect existing
plugins, so don't assume which version fixes it, or where. To see what the
installed version does, search its source for `StartAssetEditing` call sites
outside `ManualProcessAvatar`.

**What this means for a pass:** write it to be correct under the intended
behavior, and treat the manual bake as the reference run.

- Don't call asset-modifying `AssetDatabase` APIs directly from a pass; go
  through `AssetSaver` instead. NDMF's best-practices page ("Don't call
  AssetDatabase.SaveAssets or related methods during builds") rules out
  `AssetDatabase.SaveAssets`, `Refresh`, and anything else that triggers
  asset saving, because temporary build assets may still reference
  non-persisted objects mid-build and saving can corrupt them. A
  contributor's comment on #816 extends the same reasoning to other
  asset-modifying calls such as `AssetDatabase.CopyAsset`.
- The concrete failure inside the scope: anything that needs an import to
  finish before you read the result back by path. `CopyAsset` followed by
  `LoadAssetAtPath<T>(newPath)`, or `File.WriteAllBytes` +
  `AssetDatabase.ImportAsset(path)` followed by `LoadAssetAtPath`, can return
  `null`. Unity defers the import until the outermost `StopAssetEditing()`,
  which runs after your pass returns.
- "It works in Play Mode" is not evidence that the code is fine. Today it
  works there only because that path lacks the scope. If your pass touches
  `AssetDatabase` at all, run a manual bake before trusting it.
- Instead, work on in-memory copies and keep the references you hold. Let
  reachability-based saving persist them (see `AssetSaver` above), or call
  `context.AssetSaver.SaveAsset(newObject)` when you need one persistent
  sooner. `SaveAsset` is safe inside the scope because it adds an object you
  already hold to a container, with no import round-trip.
  - For a self-contained asset (a `Material`, a `VRCExpressionParameters`),
    `Object.Instantiate` the template and wire the clone in.
  - For a graph asset (an `AnimatorController`, a `BlendTree`), don't.
    `Instantiate` copies only the top-level object; the state machines,
    states and blend trees underneath are still shared with the template, so
    editing the "copy" edits the template asset. Use the NDMF animator API
    instead (see "Animation-safe editing" below). Don't `CopyAsset` a
    controller either: one referenced from the avatar may be non-persistent,
    a sub-asset of a bundle, or share its asset file with other controllers,
    so a file-level copy doesn't reliably do what you expect.

This compounds with the auto-save behavior noted above: new objects only
become persistent assets at specific checkpoints, not continuously, via
`BuildContext.Serialize()` (public, despite the name it's not purely internal
plumbing). NDMF guarantees it runs once at the very end of the whole build
(`Finish()` calls it), but in a VRChat build it *also* runs mid-build --
`BuildFrameworkPreprocessHook` calls `context.Serialize()` right after the
Transforming phase, before VRCFury and before Optimizing (see the "VRChat
build pipeline integration" section below). So a VRChat-build Optimizing-phase
pass *can* see a Transforming-phase pass's objects as persistent already, but
nothing earlier in the same phase-group can rely on that, and non-VRChat
builds (including a manual test bake) only get the one checkpoint at the very
end. This is the automatic, reachability-based path -- if you're relying on
it (i.e. you didn't call `AssetSaver.SaveAsset` yourself), don't write code
that depends on `AssetDatabase.GetAssetPath`/`LoadAssetAtPath` succeeding for
an object your own pass just created; persistence timing then depends on
which build path is running. If you need it sooner, call `AssetSaver.SaveAsset`
explicitly instead of working around the timing (see above).

A pass normally doesn't need its own `StartAssetEditing` scope, and
`context.OpenSerializationScope()` is normally unnecessary. NDMF already
batches the saving it does for you: the end-of-build `Serialize()` passes
everything it found to `AssetSaver.SaveAssets`, which wraps the whole batch
in `StartAssetEditing`. Under the intended behavior, the build-wide scope
covers everything else too. The scope affects speed only, never correctness.
Whether `GetAssetPath` works right after `SaveAsset` doesn't depend on it,
and inside it a `CopyAsset`/`ImportAsset` result *stops* being readable (see
above).

The one case where it can matter today: a pass that calls
`AssetSaver.SaveAsset` explicitly for many objects, running on a path that
lacks the build-wide scope (Play Mode, upload). Each call made outside a
scope then breaks up NDMF's batching
([#817](https://github.com/bdunderscore/ndmf/issues/817); the cost is that
issue's claim, not measured here). NDMF's own animator-services commit
handles exactly this case by wrapping its save loop in
`using (context.OpenSerializationScope()) { ... }`, and a pass can do the
same as an optional optimization. Unity's Start/Stop pairs nest, so the
scope stays harmless once the build-wide scope exists.

If you do open a scope, use `OpenSerializationScope()` rather than raw
`AssetDatabase.StartAssetEditing()`/`StopAssetEditing()`: it's `IDisposable`,
so `StopAssetEditing` still runs if your code throws. An *unbalanced* raw
call -- `StartAssetEditing` without a `finally`-guaranteed `StopAssetEditing`
-- can leave asset importing stalled for the rest of the Editor session.

## Animation-safe editing -- `nadena.dev.ndmf.animator`

Any pass that changes materials, transforms, or other values that *could* also
be driven by an animation must go through this layer, or it will only fix the
static/default state and silently leave broken references inside animation
clips -- see the "Why this shape of problem bites people" note in SKILL.md.

**Version gate: `AnimatorServicesContext` and this whole layer ship in
1.7.0+.** There is no clean equivalent on an older project -- see
`references/version-compatibility.md` before promising this approach is
available.

- `AnimatorServicesContext` is the extension context. Get it via
  `context.Extension<AnimatorServicesContext>()` after it's active -- either
  via `[DependsOnContext(typeof(AnimatorServicesContext))]` on your `Pass<T>`,
  or `sequence.WithRequiredExtension(typeof(AnimatorServicesContext), ...)` in
  `Configure()`.
- `.AnimationIndex.RewriteObjectCurves(Func<Object, Object> mapping)` walks
  every object-reference curve (materials, textures, or other object
  references assigned via animation) across every animator controller NDMF has
  virtualized, and replaces each keyframe's value through your mapping
  function.
  - The mapping must never return `null` for a non-null input -- it throws
    `InvalidOperationException` if it does. Curve values that are already
    `null` are skipped automatically before your mapping is even called, so
    you don't need to guard for that case yourself.
  - This rewrites *every layer of every controller* NDMF knows about, not just
    one layer. That's usually what you want -- a renderer's live value and its
    animated overrides need to stay consistent with each other -- but if you
    need a narrower scope, `VirtualControllerContext.Controllers` is a
    dictionary keyed by layer type (e.g. `VRCAvatarDescriptor.AnimLayerType`
    for VRChat avatars), and `AnimationIndex` has a constructor that accepts
    just the subset of virtual nodes you want indexed.
- **Object-reference curves are only half the picture.** Numeric properties
  driven by animation -- blend shape weights, a `float`/`bool`/enum shader or
  component field, a collider radius -- are *float curves*, not object
  curves, and `RewriteObjectCurves` does not touch them. `VirtualClip` (also
  under `nadena.dev.ndmf.animator`) exposes the float-curve equivalents:
  `GetFloatCurveBindings()` to enumerate, `GetFloatCurve`/`SetFloatCurve` to
  read/replace a specific binding. (From 1.15.0, `SetFloatCurve` and
  `SetObjectCurve` throw `ArgumentException` when the binding's `type` is
  unset, so build bindings with `EditorCurveBinding.FloatCurve(path, type,
  property)`.) If your pass fixes a value that could be
  animated by a float curve (e.g. forcing a blend shape weight), you generally
  need to neutralize or remove the competing float curve too -- otherwise the
  animator will keep driving the old value at runtime and silently override
  whatever your pass set. Enumerate clips via `AnimatorServicesContext`'s
  indexed data (or walk `VirtualControllerContext`'s controllers directly) to
  find the bindings whose path/property match what you changed.
- **Moving, renaming, reparenting, or removing a GameObject** while
  `AnimatorServicesContext` is active is a separate concern from the curve
  values above -- it's about whether an animation path (e.g.
  `Body/Hips/Chest`) still resolves to the right object at all, not what
  value a curve carries. `AnimatorServicesContext.ObjectPathRemapper` tracks
  this automatically, but only if you follow its rules:
  - **Moving or renaming an object is free** -- paths are tracked by
    `Transform` reference internally, not by string, so no extra call is
    needed for this case alone.
  - **Removing an object outright with a replacement taking its place**
    requires calling `ObjectPathRemapper.ReplaceObject(old, new)` before the
    removal, so existing animation paths get redirected to the replacement
    instead of going dangling.
  - **Adding a new object** you want existing or new animations to target:
    call `GetVirtualPathForObject(transform)` (or `RecordObjectTree(subtree)`
    for a whole hierarchy, e.g. an instantiated prefab) to register its path
    before authoring curves against it.
  - Access it via
    `context.Extension<AnimatorServicesContext>().ObjectPathRemapper` (same
    activation requirement as `AnimationIndex`).
- **Removing an object with no replacement** leaves its paths resolving to
  `null` in `ObjectPathRemapper.GetVirtualToRealPathMap()` -- i.e. any
  animation curve still targeting it is now a dangling reference. Don't
  assume this is automatically harmless -- verify in the actual Editor (see
  `testing.md`) whether a dangling curve binding is silently ignored or
  produces a console warning, and prune the binding explicitly
  (`GetObjectCurveBindings()`/`GetFloatCurveBindings()` plus removing the
  matching entries) if it isn't clean.
- **In unit tests**, activating this context requires
  `context.ActivateExtensionContextRecursive<AnimatorServicesContext>()`, not
  the plain `ActivateExtensionContext<T>()` -- the non-recursive version does
  not also activate `AnimatorServicesContext`'s own declared dependency
  (`VirtualControllerContext`) and throws "Extension ... not active" instead.
  Deactivate with `context.DeactivateAllExtensionContexts()`, which commits the
  virtualized controllers back onto the avatar in dependency order -- only
  read the results back after this call returns. (In a real build driven by
  `[DependsOnContext]` or `AvatarProcessor`, NDMF handles this activation
  order correctly on its own; this distinction only matters when you're
  constructing a `BuildContext` by hand in a test. See `testing.md`.)

## VRChat build pipeline integration & VRCFury ordering

This section applies only when building for VRChat (VRCSDK installed). It is
essential for diagnosing **ordering bugs** or behavior that differs between a
plain NDMF build and a VRChat SDK upload or Play mode entry.

### The "VRCFury sandwich" -- split callback order

NDMF registers **two** `IVRCSDKPreprocessAvatarCallback` hooks, not one:

| Hook class | `callbackOrder` | Phases covered |
|---|---|---|
| `BuildFrameworkPreprocessHook` | `-11000` | `First` → `Transforming` |
| `BuildFrameworkOptimizeHook` | `-1025` | `Optimizing` → `Last` |

If VRCFury is installed, NDMF source documents its callback at approximately
`-10000` (`// Must run before -10000 (VRCFury)`), which falls between these
two NDMF hooks. In that setup, the practical execution order for VRChat build
and Play mode entry is:

> **NDMF** (Resolving / Generating / Transforming) → **VRCFury** → **NDMF** (Optimizing / Last)

Consequences:
- Passes in `Transforming` or earlier run *before* VRCFury. They cannot see
  VRCFury's output.
- Passes in `Optimizing` or `Last` run *after* VRCFury. This is intentional for
  optimizer passes that should see the fully merged avatar.

Source: `Editor/VRChat/BuildFrameworkPreprocessHook.cs`.

### ApplyOnPlay goes through the full VRCSDK preprocessor chain

When Play mode is entered for a VRChat avatar, NDMF's `ApplyOnPlay` path calls
`VRCBuildPipelineCallbacks.OnPreprocessAvatar(avatarGameObject)` -- **not**
`AvatarProcessor.ProcessAvatar` directly. This triggers the full VRCSDK
`IVRCSDKPreprocessAvatarCallback` chain, so all the phase ordering above applies
in Play mode too (VRCFury and other VRCSDK preprocessors participate).

NDMF also replicates the VRCSDK clone/original naming convention at this point:
it temporarily renames the avatar to `"(Clone)"` and creates a short-lived fake
original `GameObject` because some non-NDMF hooks (e.g. VRCFury) depend on that
name pattern to distinguish the build copy from the source avatar.

For non-VRChat avatars, or when VRCSDK is absent, `ApplyOnPlay` bypasses all of
this and calls NDMF directly.

Source: `Editor/ApplyOnPlay.cs`, around the `VRCAvatarDescriptor` branch.

### VRCFury hack detection in `AvatarProcessor.ProcessAvatar`

VRCFury occasionally calls `AvatarProcessor.ProcessAvatar` directly during its
own processing. NDMF detects this via a stack-trace pattern check and adjusts:

- **Inside VRCSDK build hooks** (normal build or Play via VRC pipeline): the
  duplicate call is silently ignored -- NDMF will run via its own registered
  hooks in the correct order anyway.
- **In Play mode outside the VRCSDK hook chain** (legacy/manual path): `lastPhase`
  is clamped to `BuildPhase.Transforming`, skipping `Optimizing` and `Last` to
  avoid running optimizer passes before VRCFury has finished.

**Practical implication for custom build scripts and tests:** calling
`AvatarProcessor.ProcessAvatar` directly may produce different phase behavior
depending on what else is on the call stack. When debugging order-sensitive
behavior, verify against the installed source in
`Packages/nadena.dev.ndmf/Editor/AvatarProcessor.cs` and
`Editor/VRChat/BuildFrameworkPreprocessHook.cs` -- the installed code is
authoritative over any summary in this reference file.

Source: `Editor/AvatarProcessor.cs`, `IsVRCFuryHack`/`InHookExecution` block.

## Package & component conventions

- Give a runtime settings component `INDMFEditorOnly` (from the
  `nadena.dev.ndmf` namespace) instead of hand-rolling `#if` guards around
  VRChat's `IEditorOnly`. **Version gate: 1.7.0+** -- see
  `references/version-compatibility.md` for the pre-1.7.0 fallback. It derives
  from `IEditorOnly` only when
  `NDMF_VRCSDK3_AVATARS` is defined, and is a harmless no-op interface
  otherwise -- so the same component source compiles and behaves correctly
  whether or not VRCSDK is installed in the project.
- A typical three-assembly layout:
  - `Runtime` -- component definitions only, references `nadena.dev.ndmf.runtime`.
  - `Editor` -- the plugin and passes, references `Runtime` plus
    `nadena.dev.ndmf` and `nadena.dev.ndmf.runtime`, `includePlatforms: ["Editor"]`.
  - `Tests/Editor` -- see `testing.md`.
- If a pass reads settings from a component and the effect should be "baked
  in" to the build output, destroy the component at the end of the pass
  (`Object.DestroyImmediate(component)`) rather than leaving it behind. The
  whole point of NDMF is that the *avatar project* stays untouched while the
  *build output* reflects the transform -- a leftover marker component in the
  built avatar usually isn't what the user wants, unless it has a genuine
  runtime purpose. A leftover is also what Avatar Optimizer flags as an
  unknown component (SKILL.md Step 3, item 5). If the component really does
  need to survive, register it with AAO instead; see
  `references/aao-compatibility.md`.
