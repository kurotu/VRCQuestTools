# Avatar Optimizer (AAO) compatibility beyond removing your components

Load this only when SKILL.md's default isn't enough. The default is to destroy
build-time components before AAO runs, so AAO never sees them as unknown
(Step 3, item 5). You need this file in two cases:

- **Your transform has to run after AAO.** See "Running your transform after
  AAO". This usually does *not* require keeping the component.
- **A component has to still exist when AAO runs.** It does something at
  runtime, or AAO must be told to keep objects it references alive. See
  "Background" and the sections after it. On VRChat, custom scripts don't run
  on uploaded avatars at all, so the runtime case mostly means non-VRChat
  targets (AAO also optimizes VRM avatars, for example).

Authoritative source:
`https://vpm.anatawa12.com/avatar-optimizer/en/docs/developers/make-your-components-compatible-with-aao/`
AAO's guidance can change independently of this file, so fetch the page with
WebFetch before implementing. What follows is the stable skeleton, checked
against the AAO 1.9.20 docs and the public API source. The real reference for
every collector method is the xmldoc in `API-Editor/ComponentInformation.cs`.

Contents:

- **Running your transform after AAO** -- split the pass and keep removing
  the component
- **Background** -- what AAO assumes about a component it doesn't know
- **Register with `ComponentInformation`** -- asmdef, version defines, the
  class, collector cheat-sheet
- **Asset Description** -- and why it's rarely right for NDMF tools
- **Verifying**

## Running your transform after AAO

Some transforms need AAO's output, for example to work on meshes AAO has
already merged. Needing to run *after* AAO doesn't mean the settings component
has to *survive* AAO. Split the work into two sequences and pass the settings
across in your own `GetState<T>` class (see `BuildContext` in
`api-reference.md`):

```csharp
protected override void Configure()
{
    // 1. Before AAO: read settings into state, then remove every component.
    //    Any earlier phase works too; in Optimizing, BeforePlugin is required.
    InPhase(BuildPhase.Optimizing)
        .BeforePlugin("com.anatawa12.avatar-optimizer")
        .Run("Collect MyFeature settings", ctx =>
        {
            var all = ctx.AvatarRootObject.GetComponentsInChildren<MySettings>(true);
            var state = ctx.GetState<MyFeatureState>();
            state.Enabled = all.Length > 0 && all[0].enableFeature;
            state.Strength = all.Length > 0 ? all[0].strength : 0f;
            foreach (var s in all) Object.DestroyImmediate(s);
        });

    // 2. After AAO: do the work from state only.
    InPhase(BuildPhase.Optimizing)
        .AfterPlugin("com.anatawa12.avatar-optimizer")
        .Run(MyFeaturePass.Instance);
}

internal class MyFeatureState
{
    public bool Enabled;
    public float Strength;
}
```

`AfterPlugin(name)` orders the whole sequence after the *last* pass that
plugin has in that phase, so step 2 sees AAO's finished output. Both
constraints use the string overload, so they're harmless when AAO isn't
installed: the two steps then simply run in order.

Carry **plain values** across the AAO boundary, not references to renderers,
meshes, bones, or GameObjects. Between the two steps AAO may merge, replace,
or delete those objects, so a reference you stored can point at a destroyed
object, or at a mesh that is no longer what's rendered. In step 2, find the
targets again on the avatar as it is now (by component type, by name, or by
something you can still identify after a merge).

That last point is also where splitting stops being enough. Suppose step 2
needs an object AAO would otherwise remove as unused, such as a bone or an
empty GameObject your transform will attach things to. Moving the reference
into state doesn't keep that object alive. AAO decides what to keep only from
what's on the avatar, so it needs a component there that declares the
dependency. In that case, keep a component through AAO and register it as
described below. The registration has to do two things: keep the component
itself, because AAO garbage-collects registered components too unless
something keeps them (see the `MarkEntrypoint` notes in the cheat-sheet), and
`AddDependency` from the component to every object step 2 needs. Then destroy
the component yourself in step 2.

## Background

AAO's Trace and Optimize runs in the Optimizing phase and garbage-collects
unused components. To do that it has to understand every component still on
the avatar. For a type it has no information about, it assumes two things:
the component is a runtime component, so it's kept whenever it can be active
and enabled; and the component depends on *everything* it references, so those
objects are kept alive too. It then logs an "unknown component" warning. A
component that legitimately survives the build still triggers all of this
unless you register it. Registering it is how you replace those blanket
assumptions with what the component actually does.

## Register with `ComponentInformation`

For a component that has to exist when AAO runs, tell AAO what it does.
**Version gate: AAO 1.6.0+** (the public API didn't exist before), and AAO
will break it in 2.0.0.

### Assembly setup

The API lives in `com.anatawa12.avatar-optimizer.api.editor`, an Editor-only
assembly with `autoReferenced: false`, so you need an asmdef and an explicit
reference; the registration class therefore goes in your **Editor** assembly.
To avoid a hard dependency on AAO, guard it with a version define (resource
`com.anatawa12.avatar-optimizer`, range `[1.6,2.0)` -- or stricter, e.g.
`[1.7,2.0)`, if you use a later API). Listing the reference by name is fine
when AAO isn't installed -- Unity skips unresolved name references -- and the
`#if` keeps the code that needs it from compiling:

```json
{
  "name": "com.example.my-plugin.editor",
  "references": [
    "com.example.my-plugin.runtime",
    "nadena.dev.ndmf",
    "nadena.dev.ndmf.runtime",
    "com.anatawa12.avatar-optimizer.api.editor"
  ],
  "includePlatforms": ["Editor"],
  "versionDefines": [
    {
      "name": "com.anatawa12.avatar-optimizer",
      "expression": "[1.6,2.0)",
      "define": "AVATAR_OPTIMIZER"
    }
  ]
}
```

### The registration class

```csharp
#if AVATAR_OPTIMIZER && UNITY_EDITOR
using Anatawa12.AvatarOptimizer.API;

[ComponentInformation(typeof(MyRuntimeComponent))]
internal class MyRuntimeComponentInformation : ComponentInformation<MyRuntimeComponent>
{
    protected override void CollectDependency(MyRuntimeComponent component,
        ComponentDependencyCollector collector)
    {
        // what keeps the component alive, and what it keeps alive
    }

    protected override void CollectMutations(MyRuntimeComponent component,
        ComponentMutationsCollector collector)
    {
        // which animatable properties of other components it changes at runtime
    }
}
#endif
```

Only `CollectDependency` is abstract; `CollectMutations` and
`ApplySpecialMapping` are optional overrides. The class needs a parameterless
constructor and must be the *only* `ComponentInformation` for that type --
so register only components you own, never someone else's.

### Collector cheat-sheet

Read the xmldoc in `ComponentInformation.cs` before choosing; the semantics
are about AAO's mark-and-sweep GC and are easy to get subtly wrong.

`ComponentDependencyCollector` (in `CollectDependency`):

- `MarkEntrypoint()` -- the component has effects outside the avatar (like a
  renderer, or a contact other avatars can touch), so it's a GC root. **Even an
  entrypoint is removed when it's disabled or on an inactive GameObject.** If
  it must survive regardless of enabled state, the xmldoc says to make it a
  dependency of the avatar root instead of marking it an entrypoint.
- If it only affects specific things inside the avatar, *don't* mark it an
  entrypoint; declare dependencies so AAO can remove it when what it affects
  is gone (AAO models PhysBones and constraints this way).
- `AddDependency(dependency)` / `AddDependency(dependant, dependency)` --
  returns `ComponentDependencyInfo`; chain `.EvenIfDependantDisabled()` or
  `.OnlyIfTargetCanBeEnable()` to adjust the condition. Don't hold on to the
  returned object across another `AddDependency` call -- it may be shared.
- `AddPathDependency(Transform dependency, Transform root)` -- for objects
  your component references by relative path string. **AAO 1.7.0+.** The
  path can change during optimization; better still, resolve path strings into
  object references in your own Resolving pass so neither AAO nor other NDMF
  plugins have to understand them.
- `MarkHeavyBehaviour()` -- runtime cost that AAO may save by toggling
  `enabled` off when no entrypoint needs it. `MarkBehaviour()` -- enabled
  state is meaningful, so AAO must never toggle it (it may still remove the
  component).

`ComponentMutationsCollector` (in `CollectMutations`):

- `ModifyProperties(targetComponent, params string[] properties)` -- every
  animatable property (e.g. `"blendShape.Smile"`, `"m_LocalRotation.x"`) your
  component writes at runtime. Without this AAO may treat the property as
  constant and optimize it away, or fail to track it when it renames/merges.

`ApplySpecialMapping(component, MappingSource)`:

- AAO remaps **object references** in your component automatically when it
  merges or replaces objects. It does *not* know about strings (blend shape
  names, property names, relative paths) -- remap those yourself via
  `mappingSource.GetMappedComponent(c).TryMapProperty(...)` /
  `GetMappedGameObject(...)`. `TryMapProperty` only tracks properties you
  declared through `ModifyProperties`.

If your plugin instead wants to *add* AAO's own components to the avatar, that
is a different API (`com.anatawa12.avatar-optimizer.runtime`, AAO 1.7.0+) --
see the "Component Scripting API" page next to the one linked above.

## Asset Description

Asset Description (AAO 1.7.0+) is an asset (`Create > Avatar Optimizer >
Asset Description`) listing component scripts AAO should simply ignore. It's
meant for in-place, edit-mode-only tools that hold data and process nothing at
build or runtime. AAO's own docs advise non-destructive tools to keep removing
their components in a pass instead: if a component that *does* get processed
at build time is listed and execution order ever changes, AAO may remove it
before your pass sees it. For an NDMF plugin, destroy the component in your
pass instead (SKILL.md Step 3, item 5).

## Verifying

Compatibility can only be confirmed in a project that has AAO installed. If
the user's project doesn't, say so rather than claiming it was verified;
otherwise (AAO 1.6.0+), add
`AAO Trace And Optimize` to the test avatar's root alongside your
component(s), build (or enter Play mode), and check the NDMF console for any
warning naming your component type. Also check that a disabled instance of
your component, and the objects it depends on, behave after the build the way
your `CollectDependency` intends.
