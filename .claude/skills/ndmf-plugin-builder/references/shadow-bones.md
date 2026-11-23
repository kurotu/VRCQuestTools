# Previewing bone changes with shadow bones (NDMF 1.15.0+)

Load this when an NDMF Preview filter has to show a change to a **bone**
rather than to a renderer: a bone scaled, moved, or rotated, or a renderer
bound to a different bone. It builds on `preview-api.md`; read that first.
The build-time pass doesn't need any of this: it edits the transforms of the
avatar copy directly.

Contents:

- **Version gate**
- **What the API does**
- **The usual shape of a filter** -- including shadowing the whole subtree
- **Recipes** -- pose a bone with its children; change only one bone's own
  vertices; snap an object to another transform
- **Gotchas** -- absolute poses, re-requesting on every `Refresh`, where each
  call is allowed, unshadowed descendants
- **Testing** -- `ShadowBoneTestFixture`

## Version gate

`PreviewContext`, `IShadowBoneManagerHandle`, and `ShadowBoneTestFixture`
(all in `nadena.dev.ndmf.preview`) are new in 1.15.0. Earlier releases had an
`internal` shadow-bone class that NDMF used only to parent `MeshRenderer`
proxies; plugins couldn't call it. A preview filter can't touch the real
bones (preview must never modify the scene), so on a project pinned below
1.15.0 there is no supported way to preview a bone change. Say so, and either
skip that part of the preview or ask the user to update NDMF. Check the
installed version first (`version-compatibility.md`).

## What the API does

```csharp
using nadena.dev.ndmf.preview;

IShadowBoneManagerHandle bones = PreviewContext.Instance.ShadowBoneManager;

// Instantiate / Refresh:
Transform shadow = bones.GetBone(originalBone);

// Instantiate / Refresh only:
bones.ReplaceBone(originalRenderer, originalBone, someOtherTransform);
```

- **`GetBone(bone)`** returns a *shadow* of `bone`: a hidden `DontSave`
  transform in NDMF's preview scene (`___NDMF Preview___`). NDMF also creates
  shadows for its **ancestors**, so the shadow's world pose matches the
  original's, but **not for its descendants**. A child bone you didn't request
  stays the original transform under the original parent, so the vertices
  skinned to it don't follow when you pose the shadow (gotcha 6).
  At the start of every preview frame NDMF copies each original bone's local
  position, rotation, and scale onto its shadow. Requesting a shadow also makes
  the proxies point at it instead of the original: a `SkinnedMeshRenderer`
  proxy's `bones` and `rootBone`, any proxy's `probeAnchor`, and the parent of
  a `MeshRenderer` proxy. This applies to every proxy in the pipeline, not
  only your own group. The original renderer and the original bone are never
  touched. So to preview a bone change, request its shadow and pose the shadow.
- **`ReplaceBone(originalRenderer, original, replacement)`** makes one
  renderer's proxy use `replacement` wherever it referenced `original`: in its
  bones, root bone, and probe anchor, and as the parent of a proxy whose
  renderer sits on `original`. The match is exact: it doesn't follow
  descendants of `original`, and it doesn't chain through earlier
  replacements. The replacement is kept and replayed on each frame for you.
  Use it to bind a renderer to a different bone, instead of writing
  `proxy.bones` yourself, so the change combines with other filters' shadow
  bones.

`PreviewContext.Instance` exists only while NDMF is running a preview
pipeline. It's active during `Instantiate`, `Refresh`, and `OnFrame`.
Anywhere else it throws `InvalidOperationException` ("PreviewContext is only
available while executing a preview pipeline."), including in the build-time
pass and in a unit test that calls your filter directly. Keep it out of any
conversion code the pass and the filter share.

## The usual shape of a filter

Request shadows in `Instantiate` and again in `Refresh`, using one shared
method, and have each call return a **new** node that holds the shadows it
just requested. Write the pose in `OnFrameGroup` (called once per group per
frame; `OnFrame(original, proxy)` is called once per renderer). Both have
default no-op bodies on `IRenderFilterNode`.

```csharp
internal class BoneScaleNode : IRenderFilterNode
{
    private readonly List<(Transform original, Transform shadow, Vector3 scale)> _targets;

    // Constructed from both Instantiate and Refresh, so every shadow is
    // requested again each time (see the second gotcha below).
    internal BoneScaleNode(BoneScaleSettings s, IEnumerable<(Renderer, Renderer)> proxyPairs)
    {
        var bones = PreviewContext.Instance.ShadowBoneManager;
        var posed = s.targets.Where(t => t.bone != null).ToList();
        ShadowSubtrees(bones, proxyPairs, posed.Select(t => t.bone).ToList());
        _targets = posed.Select(t => (t.bone, bones.GetBone(t.bone), t.scale)).ToList();
    }

    // Shadow every transform a renderer in the group references at or below a
    // posed bone. Otherwise the vertices skinned to those children keep following
    // the original (gotcha 6). The group's renderers are already monitored by
    // NDMF, so plain reads of bones/rootBone/probeAnchor are fine here.
    internal static void ShadowSubtrees(IShadowBoneManagerHandle bones,
        IEnumerable<(Renderer, Renderer)> proxyPairs, IReadOnlyCollection<Transform> posed)
    {
        bool Affected(Transform t) => t != null && posed.Any(p => t == p || t.IsChildOf(p));

        foreach (var (original, _) in proxyPairs)
        {
            if (original == null) continue;
            var refs = new List<Transform> { original.transform, original.probeAnchor };
            if (original is SkinnedMeshRenderer smr)
            {
                refs.AddRange(smr.bones);
                refs.Add(smr.rootBone);
            }
            foreach (var t in refs.Where(Affected).Distinct()) bones.GetBone(t);
        }
    }

    public void OnFrameGroup()
    {
        foreach (var (original, shadow, scale) in _targets)
        {
            if (original == null || shadow == null) continue;
            // Absolute: computed from the original's current pose, never `shadow.localScale *= ...`.
            shadow.localScale = Vector3.Scale(original.localScale, scale);
        }
    }

    // WhatChanged, Refresh, Dispose, ... as usual (see preview-api.md).
}
```

`GetTargetGroups` still decides which renderers are in the group. Include the
renderers that are skinned to (or sit under) the bones you change, and observe
the settings component through `context.*` as `preview-api.md` explains.

## Recipes

These are the three shapes a bone-changing preview usually takes.

### 1. Pose a bone together with everything under it

Use this when the build changes the bone's own transform (scale, offset,
rotation), so its children move with it. Pose the bone's shadow, and shadow
the whole referenced subtree as `BoneScaleNode` above does. Scaling the shadow
then scales the child bones' vertices too, the same as the build.

### 2. Change only the vertices weighted to one bone, not its children

Scaling a shadow also scales everything under it. To affect only the vertices
weighted to the bone itself, give those vertices a transform of their own:

```csharp
// In the node's constructor (called from Instantiate and Refresh), after ShadowSubtrees(...):
var node = new GameObject("[YourPlugin] scale node").transform;
node.SetParent(bones.GetBone(bone), false);   // child of the bone's shadow, identity local pose
_nodes.Add((node, scale));
foreach (var (original, _) in proxyPairs)
{
    if (original is SkinnedMeshRenderer smr && (smr.bones.Contains(bone) || smr.rootBone == bone))
        bones.ReplaceBone(original, bone, node);  // only this renderer's references to `bone`
}

public void OnFrameGroup()
{
    foreach (var (node, scale) in _nodes)
        if (node != null) node.localScale = scale;
}

public void Dispose()
{
    // Your node, not NDMF's: destroy it. It can already be gone with its parent shadow.
    foreach (var (node, _) in _nodes)
        if (node != null) Object.DestroyImmediate(node.gameObject);
}
```

The child bones' shadows stay under the bone's shadow, not under your node,
so they keep their size. NDMF doesn't sync your node's pose, but write it in
`OnFrameGroup` anyway, from settings, as an absolute value. A later `GetBone`
of the same bone doesn't undo the `ReplaceBone` (upstream test
`RepeatedShadowLookupDoesNotOverwriteExplicitReplacement`). Every `Refresh`
builds a new node, so it creates its own scale nodes and the old node's
`Dispose` removes the old ones.

### 3. Snap an object to another transform's world pose

Use this for constraint-like effects ("place this accessory at that bone").
Request shadows of both the moved object (with its subtree, as in recipe 1)
and the target. In `OnFrameGroup`, set the moved shadow's world pose from the
target's shadow:

```csharp
moved.position = target.position;   // both are shadows
moved.rotation = target.rotation;
```

- Read from the target's *shadow*, not the original, so you pick up what this
  frame's earlier poses did to it.
- World-space writes work because the shadow's ancestors are shadowed and
  synced too. After a sync, a shadow's world pose equals the original's.
- If one moved object can be the target of another, apply the moves in
  hierarchy order, parents first.
- `lossyScale` can't be assigned. Convert the target's world scale into a
  `localScale` relative to the moved shadow's parent.
- A `MeshRenderer` on the moved object follows by itself, because NDMF
  parents a `MeshRenderer` proxy to its own transform's shadow.
- If something is computed once in `Instantiate` from a world pose rather
  than every frame, observe that transform with
  `context.ObserveTransformPosition(t)` (available since 1.5.0) so the node is
  rebuilt when it moves.

## Gotchas

1. **Write absolute poses, and write them every frame.** NDMF copies the
   original's pose onto the shadow once per *editor frame*. `OnFrame` and
   `OnFrameGroup` run once per *camera render*, so the Scene view, the Game
   view, and each extra Scene view all call them within one editor frame. A
   relative edit (`shadow.localScale *= 2`, `shadow.Rotate(...)`) is applied
   again on each render without a resync in between, and it compounds. Derive
   the value from the original bone each time. A pose you set in
   `Instantiate`/`Refresh` is overwritten at the next sync, the same way
   materials are reset (`preview-api.md`'s main gotcha). Pose the shadow in
   `OnFrame`/`OnFrameGroup`.
2. **Request the shadows again in every `Refresh`.** Each pipeline rebuild
   resets the shadow-bone registrations. A shadow that nobody requested during
   the rebuild is destroyed when the rebuild finishes. NDMF always calls
   `Refresh` on a node that used the shadow-bone manager, even when nothing it
   observed changed. So `Refresh` has to call `GetBone`/`ReplaceBone` again
   and return a new node holding the new results (or `null` to fall back to
   `Instantiate`). Don't return `this` with its shadow fields overwritten:
   the previous pipeline can still be rendering with that same node object
   while the new one builds, and it would start posing the new pipeline's
   shadows. A shadow kept from an earlier pipeline can also be a destroyed
   object. Don't keep the `IShadowBoneManagerHandle` across
   pipelines either: a handle from an earlier pipeline throws ("does not
   belong to the active PreviewContext lease"). Fetch it from
   `PreviewContext.Instance` each time. (Sources: NDMF's `NodeController
   .Refresh`, `ShadowBoneManager.ResetPipeline`/`SealPipeline`, and the
   upstream test `ResetReusesRequestedShadowsAndSealDisposesUnrequestedOnes`.)
3. **`ReplaceBone` is only allowed in `Instantiate`/`Refresh`, and only for a
   renderer in that call's group.** Anywhere else it throws
   `InvalidOperationException`. An original renderer that isn't in the
   current group's proxy pairs throws `ArgumentException`. If `Instantiate` or
   `Refresh` throws, the replacements it made are rolled back.
4. **`GetBone` only redirects references that came from the original
   hierarchy.** A bone reference your filter writes into a proxy itself
   isn't redirected to a shadow. Use `ReplaceBone` to change references.
5. **Don't destroy or reparent shadows yourself.** NDMF owns them. If one
   disappears unexpectedly, NDMF logs "Proxy bone was destroyed improperly!
   Resetting preview system..." and rebuilds the whole preview.
6. **Shadow the descendants you need, explicitly.** Posing a shadow moves
   only the shadows under it. A child bone nobody requested is still the
   original transform, so in a skinned mesh the part weighted to the bone
   moves and the part weighted to its children stays put. It can look right
   by accident: NDMF shadows every `MeshRenderer`'s own transform and its
   parents, so an accessory mesh under the child bone makes that child a
   shadow too, and the bug only shows on avatars without one. Request the
   referenced subtree yourself (`ShadowSubtrees` above).

These were confirmed by running them against 1.15.0:

- gotchas 1, 3, and 6
- the redirects in "What the API does"
- the per-transform results of recipes 2 and 3: the proxy references, the
  child bones keeping their scale, and the world poses
- both test samples

The following come only from reading 1.15.0's source and upstream tests:

- the once-per-editor-frame timing in gotcha 1
- all of gotcha 2
- the replay of replacements in the real pipeline
- what finally renders

If the installed version behaves differently, trust it over this file.

## Testing

Because `PreviewContext.Instance` throws outside a pipeline, a test that
calls `Instantiate` directly has to set one up. `ShadowBoneTestFixture` gives
a real shadow-bone manager for that:

```csharp
// original and proxy are typed Renderer: a (SkinnedMeshRenderer, SkinnedMeshRenderer)[]
// won't convert to IEnumerable<(Renderer, Renderer)>.
using var fixture = new ShadowBoneTestFixture();
using (new PreviewContext { ShadowBoneManager = fixture.Handle }.Activate())
{
    var pairs = new Dictionary<Renderer, Renderer> { { original, proxy } };

    // Runs the call the way the pipeline would (inside a stage), and keeps its
    // GetBone/ReplaceBone registrations if it returns non-null.
    var node = fixture.ExecuteInStageSync(pairs, () =>
        filter.Instantiate(group, new[] { (original, proxy) }, new ComputeContext("test")).Result);

    fixture.SyncPoses();   // what NDMF does at the start of a frame
    node.OnFrameGroup();
    Assert.That(((SkinnedMeshRenderer)proxy).bones[0].localScale, Is.EqualTo(expected));

    // Gotcha 1 as a regression test: two renders in one editor frame.
    node.OnFrameGroup();
    Assert.That(((SkinnedMeshRenderer)proxy).bones[0].localScale, Is.EqualTo(expected));

    // Gotcha 6 as a regression test: the child bone (index 1 here) is a shadow
    // under the posed bone's shadow, so it follows.
    var smrProxy = (SkinnedMeshRenderer)proxy;
    Assert.That(smrProxy.bones[1], Is.Not.SameAs(((SkinnedMeshRenderer)original).bones[1]));
    Assert.That(smrProxy.bones[1].parent, Is.SameAs(smrProxy.bones[0]));
}
```

Build the test avatar for gotcha 6 from skinned renderers only. A
`MeshRenderer` under the child bone would shadow it anyway and hide the bug.
Also assert that the original bone's transform and the original renderer's
`bones` are unchanged.
