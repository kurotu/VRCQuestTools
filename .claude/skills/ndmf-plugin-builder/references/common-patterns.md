# Common NDMF Plugin Patterns

## Non-destructively duplicating a shared asset (material, texture, ...)

The shape of almost every "convert X" NDMF pass is:

1. Find every distinct instance of the thing you want to convert, across
   **both** live component references (renderer materials, etc.) and animation
   curves -- build a single de-duplicating cache
   (`Dictionary<Material, Material>` or similar) keyed by the original object,
   so the same original always maps to the same converted clone. Without this
   cache, a renderer and an animation clip that both reference the same
   original material would silently diverge into two different clones after
   conversion -- easy to miss in a small test scene, and a real correctness
   bug in an avatar where many outfit toggles share one material.
2. For each original, clone it (e.g. `new Material(original)`), apply your
   changes to the *clone*, then replace every reference (renderer materials,
   animation curves -- see below) with the clone. Never mutate `original` in
   place -- part of the point of choosing NDMF over an in-place editor script
   is that the source asset stays exactly what the user sees in the Project
   window after a build.
3. Call `ObjectRegistry.RegisterReplacedObject(original, clone)` for every
   clone you create -- see `api-reference.md`. You do *not* need
   `context.AssetSaver.SaveAsset(clone)` here: because step 2 already wired
   the clone into a live reference or animation curve, NDMF's end-of-build
   serialization finds it via that reference and persists it automatically.
   The narrow cases where `SaveAsset` *is* needed -- and why it's not a
   safety net for a clone you forgot to wire in -- are covered under
   `AssetSaver` in `api-reference.md`.

### Gotcha: a getter that "resolves" a value doesn't tell you what was explicitly set

It's tempting to read a property before changing something and write it back
afterward to "preserve" it, e.g.
`var q = original.renderQueue; clone.shader = newShader; clone.renderQueue = q;`.
Don't do this reflexively -- verify first what the getter actually returns.
`Material.renderQueue`'s getter, for example, always returns the *resolved*
integer queue; there is no way to read back "unset" even when the material has
no explicit override. Round-tripping through it like this bakes in whatever
number the *old* shader happened to resolve to, silently overriding whatever
queue the *new* shader would otherwise pick by default -- and it will look
correct in casual testing because the number is still "a valid queue", just
the wrong one. The copy constructor (`new Material(original)`) already copies
the real underlying override state (including "no override") correctly on its
own -- trust it, and don't touch a property like this unless you have a
specific, verified reason to.

This generalizes beyond render queues: before writing code that reads a Unity
property back in order to restore it later, check whether the getter can
actually represent every state the setter can produce. If it collapses several
underlying states into one resolved value, round-tripping through it is lossy,
and the bug will compile fine and often look fine in a quick check.

### Gotcha: Material Variants silently ignore changes on their clones too

If `original` is a Material Variant (`original.isVariant == true`, non-null
`original.parent`), `new Material(original)` produces *another* variant --
`clone.isVariant` stays `true` and `clone.parent` stays non-null. A subsequent
`clone.shader = newShader` (or other change the variant system locks) is then
**silently ignored**: no exception, no console warning, the clone just keeps
rendering with the parent's shader. This is easy to miss because everything
compiles and runs without complaint -- you only notice when the "converted"
material still looks like the original.

Fix: break the link right after cloning, before making the change the variant
system would otherwise reject:

```csharp
var clone = new Material(original);
if (clone.parent != null) clone.parent = null; // bakes current values, becomes standalone
clone.shader = newShader; // now actually takes effect
```

Setting `parent = null` bakes the variant's currently-resolved values
(inherited plus any overrides) into the clone as a standalone material --
nothing is lost, but the clone is no longer subject to its former parent's
restrictions. Confirm this behavior against whatever Unity version the project
actually targets by testing it in the real Editor (see `testing.md`) rather
than assuming -- it's the kind of "quiet by design" API behavior that is worth
a quick empirical check rather than trusting from memory, and could change
across Unity versions.

## Rewriting animation references to match

Any renderer or component you modify might also be the target of an
animator-driven override (a toggle, an outfit swap, a material-swap
animation). If you only update the live value and skip the animation layer,
the avatar looks correct in the Editor and in Play Mode right up until the
*first* time that animation actually plays -- then it snaps back to the
unconverted value. Always pair a "modify the live reference" step with an
`AnimationIndex.RewriteObjectCurves` pass (or the equivalent for whatever kind
of curve you're touching) using the exact same mapping you used for live
references -- see `api-reference.md`'s animation section. Reuse the same
per-original cache from the duplication pattern above so a renderer and an
animation clip that reference the same original resolve to the *same* clone.

## Settings component design

- One `MonoBehaviour : INDMFEditorOnly` per concern, discovered via
  `avatarRoot.GetComponentsInChildren<YourSettings>(true)`. Decide up front
  what happens if more than one exists in the hierarchy (reporting a NonFatal
  `ErrorReport` and using the one closest to the root is a reasonable default)
  and what happens if none exists (usually: skip the whole pass rather than
  silently applying default behavior nobody asked for).
- Give it a `[HelpURL("...")]` pointing at user documentation, and
  `[DisallowMultipleComponent]` if only one instance per GameObject makes
  sense for the feature.
- If the component's job is purely to configure a build-time transform,
  destroy it at the end of the pass -- see "Package & component conventions"
  in `api-reference.md` for the reasoning and the exception.
