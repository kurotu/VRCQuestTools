# NDMF Preview (`nadena.dev.ndmf.preview`)

NDMF Preview is NDMF's live Scene-view preview system. Verified against
`nadena.dev.ndmf` 1.14.0, where NDMF Preview dates to 1.5.0 (it was introduced
whole -- there is no older, separate mechanism to fall back to). As with
`api-reference.md`, trust the installed source under `Packages/nadena.dev.ndmf`
over this file if they disagree. See `references/version-compatibility.md` for
the rest of the version matrix relevant to preview work (e.g. when the public
`PropCache`/`PropCacheDebug` helpers used in testing became available).

This is a separate concern from everything else in this skill: build-time
passes (`Plugin<T>`/`Pass<T>`) transform a *copy* of the avatar during an
actual build, while a preview filter renders a *live approximation* of that
same transform in the Scene view without ever running a build or touching the
real scene objects. Add this only when the user actually wants to *see* the
transform's effect before building -- most plugins don't need it, and it's
meaningfully more error-prone than the build-time path (see the gotchas
below).

Contents:

- **Registration** -- `.PreviewingWith(...)`, singleton filter instances
- **The two interfaces you implement** -- `IRenderFilter` /
  `IRenderFilterNode`
- **The gotcha that will cost you the most time** -- materials must be
  reapplied every frame in `OnFrame`; plus a checklist of cheaper causes to
  rule out first
- **The other gotcha** -- `GetTargetGroups` must exclude renderer types NDMF
  can't proxy, or the whole filter's preview silently dies
- **What to `Observe` and what NDMF already tracks for you** -- renderer/
  material/mesh state is automatic; anything else `GetTargetGroups` reads
  outside `context.*` is not, including a trap where `GetAvatarRoots()`
  itself won't tell you either
- **`ObjectRegistry`, `AssetSaver`, and `ErrorReport` in a preview filter** --
  which build-time helpers carry over and which don't
- **Testing preview filters** -- `ComputeContext` construction, the
  `PropCache` staleness gotcha, and what's worth asserting explicitly

## Registration

Chain `.PreviewingWith(filterInstance)` directly off the same `.Run(...)` call
that registers your pass:

```csharp
InPhase(BuildPhase.Transforming)
    .AfterPlugin("nadena.dev.modular-avatar")
    .Run(YourPass.Instance)
    .PreviewingWith(YourPreviewFilter.Instance);
```

Use a single static/singleton `IRenderFilter` instance -- NDMF's own doc
comment on `PreviewingWith` states each filter instance may be registered only
once; don't `new` one per call or register the same instance from two places.

## The two interfaces you implement

`IRenderFilter` decides *what* to preview and builds the first version of it:

- `GetTargetGroups(ComputeContext context)` returns the `RenderGroup`s
  (renderer sets) you want to process. Query through `context` --
  `context.GetAvatarRoots()`, `context.GetComponentsInChildren<T>(root,
  includeInactive)`, `context.Observe(obj)`, etc. -- rather than plain Unity
  APIs. This is what makes the whole thing reactive: NDMF tracks what your
  query touched and recomputes automatically when it changes, so editing a
  settings field or adding/removing a component updates the Scene view without
  any manual event wiring on your part.
- `Instantiate(RenderGroup group, IEnumerable<(Renderer original, Renderer
  proxy)> proxyPairs, ComputeContext context)` builds the first
  `IRenderFilterNode` for a group. Same rule as the build-time path: clone
  Mesh/Material/Texture, never mutate `original` in place. NDMF actively
  enforces this -- it snapshots each original renderer's `sharedMaterials`
  before calling `Instantiate` and compares afterward; if you changed the
  original, it logs a warning and force-restores it. Write to `proxy`, read
  from `original`.

`IRenderFilterNode` is the live instance for one group:

- `Refresh(proxyPairs, context, updatedAspects)` rebuilds when an upstream
  dependency changes (e.g. re-run the same logic as `Instantiate`). Returning
  `null` tells the pipeline to fall back to a fresh `Instantiate` instead.
- `OnFrame(Renderer original, Renderer proxy)` -- see the gotcha below; this is
  not optional for a filter that changes materials.
- `Dispose()` must destroy every Mesh/Material/Texture instance you created,
  the same lifetime discipline as `AssetSaver`-tracked objects in a build.

## The gotcha that will cost you the most time: materials must be reapplied every frame in `OnFrame`

This is the single most expensive mistake to make here, because it produces
**no error, no warning, a clean compile, and passing unit tests** -- the only
symptom is "I enabled preview and nothing changed," which looks exactly like a
dozen more mundane causes (preview disabled, wrong avatar, filter not
registered).

NDMF's proxy pipeline resets state every single rendered frame, before your
filter chain runs: `ProxyObjectController.OnPreFrame()`
(`Packages/nadena.dev.ndmf/Editor/PreviewSystem/Rendering/ProxyObjectController.cs`)
unconditionally does `Renderer.sharedMaterials = _originalRenderer.sharedMaterials;`
to re-mirror the proxy against the original's *current* state, and only after
that does `ProxyPipeline.OnFrame` call each node's `OnFrame(original, proxy)`.
This runs on every camera render (`ProxySession.OnPreCull`), not once at
setup. So:

```csharp
// WRONG: this only sticks for the single frame Instantiate/Refresh ran in.
// The very next rendered frame, OnPreFrame() resets proxy.sharedMaterials
// back to the original, and since OnFrame is never overridden, nothing
// reapplies the conversion -- the proxy silently reverts to the original.
public Task<IRenderFilterNode> Refresh(...)
{
    proxy.sharedMaterials = convertedMaterials;
    return Task.FromResult<IRenderFilterNode>(this);
}
```

The `IRenderFilterNode.OnFrame` doc comment says "generally, you should not
modify the mesh or materials in this method" -- read that as "don't do
*expensive* work here, like creating new Material instances," not "never
touch the materials array." Re-assigning an already-computed Material
*reference* every frame is cheap and is exactly what the framework expects for
any filter whose effect is a material swap. Store what each original renderer
should map to (e.g. `Dictionary<Renderer, Material[]>` computed once in
`Instantiate`/`Refresh`), then reapply it in `OnFrame`:

```csharp
private readonly Dictionary<Renderer, Material[]> _convertedMaterials;

public void OnFrame(Renderer original, Renderer proxy)
{
    if (_convertedMaterials.TryGetValue(original, out var materials))
    {
        proxy.sharedMaterials = materials;
    }
}
```

If preview "does nothing" and you've already ruled out the mundane causes (see
the checklist below), suspect this first -- it's the quiet-by-design behavior
most likely to be the actual cause.

### Checklist before you go deeper

Cheaper things to rule out first, since they produce the identical symptom:

1. NDMF Preview globally enabled? (`Tools > NDM Framework`, the preview toggle
   menu item.)
2. Your plugin's own preview enabled in `Tools > NDM Framework > Configure
   Previews`? (Appears automatically once any pass calls `.PreviewingWith`;
   users can disable it per-plugin.)
3. Does `GetTargetGroups` actually find your settings component and avatar
   root? (Log or step through it -- see the testing section below for how to
   call it directly.) Note that finding them once isn't the whole story: if
   preview appears but stops reflecting later edits, that's the reactivity
   section below ("What to `Observe`..."), not this checklist.
4. Does your conversion logic actually succeed for the materials/values in
   question (e.g. a target shader that's missing from the project)? Test the
   core conversion logic in isolation first, the same way you would for the
   build-time path.

Only after all four check out does the per-frame `OnFrame` reapplication
become the prime suspect.

## The other gotcha: `GetTargetGroups` must exclude renderer types NDMF can't proxy

This is a second way for a preview filter to go completely silent, and it's
easy to mistake for the `OnFrame` gotcha above -- same symptom ("I enabled
preview and nothing changed"), different cause, different fix.

NDMF's proxy system only knows how to build a preview proxy for
`MeshRenderer` and `SkinnedMeshRenderer`. `ProxyObjectController
.CreateReplacementObject` (`Packages/nadena.dev.ndmf/Editor/PreviewSystem
/Rendering/ProxyObjectController.cs`) logs `"Unsupported renderer type: " +
...` and returns `null` for anything else -- `ParticleSystemRenderer`,
`TrailRenderer`, `LineRenderer`, etc. `TargetSet.ComputeGroupsForFilter`
(`Packages/nadena.dev.ndmf/Editor/PreviewSystem/Rendering/TargetSet.cs`)
guards against this *before* proxy creation even runs, by scanning every
`RenderGroup` your `GetTargetGroups` returned:

```csharp
var unsupportedRenderer = groups.SelectMany(g => g.Renderers)
    .FirstOrDefault(x => x is not MeshRenderer and not SkinnedMeshRenderer);
if (unsupportedRenderer != null)
{
    Debug.LogError("[" + filter + "] Unsupported renderer " + unsupportedRenderer +
                   " in groups: " + string.Join(", ", groups));
    return new CachedGroups(ImmutableList<RenderGroup>.Empty);
}
```

If even one renderer across one group is an unsupported type, **every group
your filter returned is discarded, for every avatar in the scene** -- not
just the group containing the offending renderer. A single
`ParticleSystemRenderer` anywhere under one avatar silently kills preview for
every other avatar this filter would otherwise have handled too. No
exception is thrown and the filter isn't disabled; the only trace is a
`Debug.LogError` that repeats on every recompute and is easy to lose among
other Console output. This is per-filter-instance
(`PropCache<IRenderFilter, CachedGroups>`), so it doesn't cascade to other
plugins' preview filters -- only this filter's own preview stage goes dark.

**Fix: filter renderers down to the proxy-supported types before building
`RenderGroup`s in `GetTargetGroups`**, not after:

```csharp
var renderers = context.GetComponentsInChildren<Renderer>(avatarRoot, true)
    // NDMF's TargetSet discards this filter's ENTIRE group set if even one
    // returned renderer is a type it can't proxy (e.g. ParticleSystemRenderer).
    // Exclude unsupported types here rather than letting that happen.
    .Where(r => r is SkinnedMeshRenderer or MeshRenderer)
    .ToArray();
if (renderers.Length == 0) continue;

groups.Add(RenderGroup.For(renderers).WithData(settings, ReferenceEquals));
```

As cheap insurance, also guard `proxy == null` wherever `IRenderFilterNode`'s
`Instantiate`/`Refresh` consumes a `proxyPairs` pair, even though the guard
above means that path isn't currently reachable in this NDMF version. It
costs essentially nothing, and NDMF's own `NodeController.OnFrame` already
guards the same way (`if (original != null && proxy.Renderer != null)`) --
a possibly-null proxy is already part of `IRenderFilter`'s contract, and a
future NDMF version could drop the "discard the whole group set" behavior in
favor of excluding just the offending renderer, at which point this guard
starts actually firing.

This is a preview-only limitation. The build-time path (`Plugin<T>`/`Pass<T>`)
never goes through the proxy system, so a `ParticleSystemRenderer` etc. is
converted correctly in the real build output even though you can't see that
conversion live in preview.

## What to `Observe` and what NDMF already tracks for you

Once a filter does more than just "which renderers exist in my target group,"
two separate questions come up: what must the filter explicitly wire up
through `context`, and what does NDMF already track without being asked?
Getting either wrong produces the same silent-no-recompute symptom as the two
gotchas above, for reasons specific to reactivity rather than to `OnFrame` or
renderer types.

### Already tracked for you: renderer/material/mesh state of renderers already in a group

Once a renderer is part of a `RenderGroup` your filter returned, NDMF's proxy
pipeline already watches it on your behalf. `ProxyObjectController
.SetupRendererMonitoring` (`Packages/nadena.dev.ndmf/Editor/PreviewSystem
/Rendering/ProxyObjectController.cs`) observes the renderer component itself,
every entry of `sharedMaterials` plus each material's textures, and (for
`SkinnedMeshRenderer`/`MeshRenderer`) its mesh. Any change there invalidates
the whole preview pipeline and is reported to your node's `Refresh(proxyPairs,
context, updatedAspects)` as the matching `RenderAspects` flag (`Mesh` /
`Material` / `Texture` / `Shapes`) -- see
`Packages/nadena.dev.ndmf/Editor/PreviewSystem/Rendering/ProxyPipeline.cs`,
where each proxy's `InvalidateMonitor.Invalidates(_ctx)` feeds the
pipeline-wide
invalidation and `ChangeFlags` becomes the `updatedAspects` your node receives.
Calling `context.Observe(original)` yourself in `Instantiate`/`Refresh` on top
of this doesn't unlock anything you don't already get for free -- it's
redundant, not wrong.

### Not tracked unless you ask: anything `GetTargetGroups` reads outside `context.*`

Reactivity only exists for what you fetch *through* `context` --
`context.GetAvatarRoots()`, `context.GetComponent<T>(obj)`,
`context.GetComponentsInChildren<T>(root, includeInactive)`,
`context.Observe(obj)` -- because these are the calls that register a watcher
as a side effect (`Packages/nadena.dev.ndmf/Editor/PreviewSystem/ComputeContext
/SingleObjectQueries.cs`; the scene-wide queries such as `GetAvatarRoots` live
next to it in `GlobalQueries.cs`). A plain `GetComponentInChildren<T>()` or
`someObj.GetComponent<T>()` call anywhere inside `GetTargetGroups` is invisible
to the pipeline: NDMF has no way to know your result depended on it, so
adding, removing, or editing that object later never triggers a recompute --
preview goes stale silently, the same "nothing updates" symptom as the two
gotchas above, for a third, unrelated reason.

### The trap that combines both: `GetAvatarRoots()` doesn't propagate every change under a root

`context.GetAvatarRoots()` is backed by a `PropCache` compared with
`Enumerable.SequenceEqual` on the resulting list of avatar root `GameObject`s
(`GlobalQueries.cs`). Per `PropCache.InvalidateEntry`
(`Packages/nadena.dev.ndmf/Editor/PreviewSystem/ComputeContext/PropCache.cs`),
when a recompute produces a value that compares equal to the cached one, the
cache's observer context is deliberately *not* invalidated -- by design,
callers downstream of `GetAvatarRoots()` are told nothing changed. Adding a
settings `MonoBehaviour` to an avatar that was already an avatar root doesn't
change the *list of avatar roots*, so it never propagates through
`GetAvatarRoots()` at all -- this isn't a bug, it's the whole point of the
equality comparer, but it means depending on `GetAvatarRoots()` alone is not
enough to react to component-level edits on an existing avatar.

If `GetTargetGroups` calls `context.GetAvatarRoots()` and then locates your
settings component with a plain Unity API call (the previous point), that
component's changes are invisible for two independent reasons stacked
together. Locate your own component through a context-aware call instead --
`context.GetComponentsInChildren<YourSettings>(root, true)` or
`context.GetComponent<YourSettings>(root)` -- which registers its own
membership watch regardless of whether the avatar-root list itself changed.

### `Observe(obj)` vs `Observe(obj, extract)`: event-driven vs. also polled

Both overloads of `context.Observe` (`SingleObjectQueries.cs`) react to
Unity's Undo/hierarchy-dirty event stream through
`ObjectWatcher.MonitorObjectProps`
(`Packages/nadena.dev.ndmf/Editor/ChangeStream/ObjectWatcher.cs`). Only the
`extract` overload additionally registers with `PropertyMonitor`
(`Packages/nadena.dev.ndmf/Editor/ChangeStream/PropertyMonitor.cs`), which
re-checks the object on a per-frame loop while the Editor is focused (or
animation mode is active), independent of whether an Undo-recorded event
fired at all. This polling applies to components, materials, and other
non-`GameObject` objects; `MonitorObjectProps` routes a `GameObject` through
its hierarchy-event branch without ever consulting the prop monitor, so an
`extract` observation *of a `GameObject` itself* stays event-driven despite
the overload used. In practice: a field edited
through the Inspector is already caught by plain `Observe(obj)`, since
Inspector edits go through Unity's Undo system -- don't reach for the
`extract` overload "just in case," since it adds an ongoing per-frame
recheck. Use `Observe(obj, extract)` when the value you actually care about
can change without an Undo event, e.g. another script writing directly to a
field outside the normal property-drawer path.

## `ObjectRegistry`, `AssetSaver`, and `ErrorReport` in a preview filter

These three build-time-path helpers (`api-reference.md`) behave differently
under NDMF Preview, and the difference isn't obvious from their signatures
alone:

- **`ObjectRegistry.RegisterReplacedObject(original, replacement)` works
  unchanged, and you should still call it.** It isn't actually
  `BuildContext`-scoped -- it reads/writes a static
  `AsyncLocal<IObjectRegistry> ActiveRegistry`. NDMF's own preview pipeline
  (`NodeController.Create`/`Refresh`) wraps every call into your
  `Instantiate`/`Refresh` in an `ObjectRegistryScope`, specifically so filters
  can register this mapping the same way a build-time pass does. Outside any
  scope (e.g. a direct unit-test call) it's already a safe no-op. There's no
  reason to special-case this call between the two code paths -- put it in
  whatever shared conversion logic both paths call.
- **`context.AssetSaver.SaveAsset(...)` is genuinely build-only and has no
  preview equivalent.** It persists a new asset into the build's output
  container; a preview proxy's materials are transient in-memory objects that
  get `Dispose()`d when the node is torn down, so there's nothing to save.
  Don't call it from preview code (you won't have a `BuildContext` to call it
  on anyway, if your shared conversion core is designed to not need one).
- **Don't call `ErrorReport.ReportError(...)` from a preview filter.** It
  unconditionally logs to `Debug.LogWarning`/`Debug.LogException` every time
  it's invoked, regardless of whether a build's error report is active (the
  active-report check only gates whether it's *additionally* recorded into a
  build's error list). A preview filter recomputes on every relevant edit, so
  routing errors through it would spam the Console on every keystroke. Prefer
  a shared "try convert, return success/failure plus enough detail to explain
  why" core function; call `ErrorReport` only from the build-time caller, and
  have the preview caller silently fall back to the original value.

The practical shape this suggests: extract the actual per-object conversion
logic (shader lookup, property copying, whatever your transform does) into a
small `BuildContext`-free function that both the build `Pass` and the preview
filter call, and let each caller layer its own error-reporting/persistence
behavior on top. See `common-patterns.md` for the same
duplicate-and-transform shape applied to the build-time path -- a preview
filter is usually calling the same core logic against proxy renderers instead
of the real ones.

## Testing preview filters

`ComputeContext` has a public constructor (`new ComputeContext("description")`)
-- unlike `BuildContext`, you don't need `InternalsVisibleTo` tricks to build
one from a test assembly. `Instantiate`/`Refresh` return `Task<IRenderFilterNode>`
and can be implemented as synchronously-completed (`Task.FromResult(...)`),
so calling `.Result` in a test works fine without any real async machinery.

**Gotcha: `ComputeContext.GetAvatarRoots()` (and other global scene queries)
are cached and won't see an object your test just created.** They're backed by
a `PropCache` that only recomputes when NDMF's `ObjectWatcher` observes an
actual editor hierarchy-change event -- which is asynchronous and never fires
within a single synchronous `[Test]` method body. So a test that does
`new GameObject(...)`, adds your settings component, and immediately calls
`GetTargetGroups()` can fail with "component/avatar not found" even though the
object genuinely exists -- not because your filter is wrong, but because the
cache predates the object. Force a recompute with the public
`nadena.dev.ndmf.preview.PropCacheDebug.InvalidateAllCaches()` right before the
call that depends on current scene state:

```csharp
_builder.AddSettings();
PropCacheDebug.InvalidateAllCaches(); // otherwise GetAvatarRoots() may return a stale, pre-test snapshot
var groups = YourPreviewFilter.Instance.GetTargetGroups(new ComputeContext("test"));
```

Worth testing explicitly, beyond the obvious "does `Instantiate` produce the
right material":

- `Instantiate` mutates only the proxy, never the original (assert on the
  original renderer/material after calling it).
- Materials that should be shared (same original referenced by multiple
  renderers) resolve to the same converted instance -- `ReferenceEquals` --
  mirroring whatever dedup guarantee your build-time path makes.
- **The `OnFrame` reapplication specifically**, to lock in the gotcha above as
  a regression test: call `Instantiate`, then manually reset the proxy's
  materials back to the original's (simulating what `OnPreFrame` does every
  frame), call `node.OnFrame(original, proxy)`, and assert the converted
  materials are back. A test that only checks the state right after
  `Instantiate` will pass even with the `OnFrame` bug, since the bug only
  manifests on the *next* simulated frame.
- If you added `ObjectRegistry.RegisterReplacedObject` to the shared core,
  confirm it actually registers under preview too: wrap the `Instantiate` call
  in `using (new ObjectRegistryScope(new ObjectRegistry(null)))` and assert
  `ObjectRegistry.GetReference(convertedObject).Object == original`.
- **That `GetTargetGroups` locates your settings component through a
  context-aware call**, not a plain `GetComponent*`, to lock in the "not
  tracked unless you ask" gotcha above. This is awkward to assert directly
  (the effect is an omission -- no invalidation firing -- not a return value),
  so the practical version is a code-review check on `GetTargetGroups`
  itself: every lookup that decides which renderers end up in a group should
  go through `context.GetComponent<T>`/`context.GetComponentsInChildren<T>`,
  not `GameObject.GetComponent*`. A plain call will still pass every
  functional test run in the same synchronous method, since the component
  already exists by the time you query for it -- the bug only shows up as a
  live Scene view that doesn't update after adding the component to an
  already-open scene, which unit tests don't exercise.
- **That `GetTargetGroups` excludes renderer types NDMF can't proxy**, to lock
  in the other gotcha above as a regression test. Add a renderer type NDMF
  doesn't support (e.g. `ParticleSystemRenderer`) alongside a supported one on
  the same test avatar, and assert the returned group contains the supported
  renderer but not the unsupported one. (In this sketch, `_builder` is your
  own test-avatar builder from `testing.md`, and `FindOwnGroup` stands for a
  small helper that calls `GetTargetGroups` -- after
  `PropCacheDebug.InvalidateAllCaches()`, see above -- and picks out the group
  this filter produced for the test avatar.)

  ```csharp
  [Test]
  public void GetTargetGroups_ExcludesUnsupportedRendererTypes()
  {
      _builder.SetBodyMaterials(_builder.CreateMaterial(_shader));
      var particleRenderer = _builder.AddParticleRenderer("Fx", _builder.CreateMaterial(_shader));
      _builder.AddSettings();

      var group = FindOwnGroup(new ComputeContext("test"));

      Assert.That(group, Is.Not.Null);
      Assert.That(group.Renderers, Does.Contain(_builder.BodyRenderer));
      Assert.That(group.Renderers.Contains(particleRenderer), Is.False);
  }
  ```

  Adding a `ParticleSystem` via `AddComponent<ParticleSystem>()` makes Unity
  attach a `ParticleSystemRenderer` automatically, which is a convenient way
  to grow an unsupported-type renderer on a test avatar:

  ```csharp
  public ParticleSystemRenderer AddParticleRenderer(string name, Material material)
  {
      var go = new GameObject(name);
      go.transform.SetParent(Root.transform, false);
      go.AddComponent<ParticleSystem>();
      var renderer = go.GetComponent<ParticleSystemRenderer>();
      renderer.sharedMaterial = material;
      return renderer;
  }
  ```

  Watch for a NUnit overload-resolution trap here: `Does.Contain` compiles
  against a `ParticleSystemRenderer` argument, but `Does.Not.Contain` with the
  same argument can fail to compile (it resolves to a `string`-conversion
  overload instead). The asymmetry -- one form compiles, the negated form
  doesn't -- is easy to lose time to. Sidestep it entirely by asserting on
  `.Contains(...)` directly: `Assert.That(group.Renderers.Contains(x), Is.False)`.

None of this replaces opening a real scene and watching NDMF Preview update
the Scene view live -- see `testing.md`'s "verify by hand" section, which
applies here at least as much as it does to the build-time path. A filter can
pass every unit test above and still do nothing visible if, for instance, the
shader it targets isn't actually imported into the test/real project
(`Shader.Find` returning null is a silent no-op by design, not a test-visible
failure).
