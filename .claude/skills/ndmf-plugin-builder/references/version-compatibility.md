# NDMF API Version Compatibility (1.3.0 -> 1.15.1)

This skill's other reference files describe the API surface as it exists in a
recent NDMF release, but a real project can be pinned to whatever version it
was last updated against. Most of the APIs this skill teaches were added
incrementally, not present from day one -- if the project's installed NDMF
predates an API's introduction, that API genuinely isn't there yet, and code
written against it won't compile or won't behave as documented.

**Last verified: 1.15.1 (2026-11-23).**

This file's version data reflects the upstream changelog
(`https://github.com/bdunderscore/ndmf/blob/main/CHANGELOG.md`, or the vendored
copy at `Packages/nadena.dev.ndmf/CHANGELOG.md` if one is present in the
project) up to and including the release above. It was compiled by reading
that changelog, not by testing every API against every version, so treat
every version number below the same way the rest of this skill treats API
names: as a strong lead, not an unconditional fact -- if the installed source
or changelog disagrees with this file, trust the installed source. For
anything released after the version above, this file is simply silent; if the
project's installed NDMF is newer, skim the changelog entries in between
before assuming an API's status hasn't changed.

Contents:

- **Checking the installed version** -- where to look
- **Feature availability** -- minimum version for each API this skill teaches,
  and what to do on an older project
- **Known breaking/removed APIs and behavior changes** -- cases where an API
  or a documented behavior changed in a patch release

## Checking the installed version

Read the `version` field of `Packages/nadena.dev.ndmf/package.json` in the
target project, or check Unity's Package Manager window (`Window > Package
Manager`, find "NDM Framework"). Do this before relying on any API from
`api-reference.md` or `preview-api.md` if there's any reason to suspect the
project hasn't been updated recently -- e.g. it depends on an old, pinned
VPM/VCC repository, or the user mentions build errors that look like a missing
type or member rather than a logic bug.

## Feature availability

**`ErrorReport` / `ErrorReport.ReportError`** -- 1.3.0+. This is the version
that introduced NDMF's whole error-reporting framework (the changelog entry is
literally "New error reporting framework"); there's no older recoverable-error
mechanism to fall back to. On a pre-1.3.0 project, a pass can only report
problems by throwing or logging directly to the Console, which aborts the
whole build for a thrown exception -- see the tradeoff this skill otherwise
recommends against in `api-reference.md` and `SKILL.md`'s Step 3.

**`ObjectRegistry.RegisterReplacedObject`** -- 1.3.0+, added in the same
release as `ErrorReport` (changelog: "API to record when one object is
replaced by another"). No fallback needed below 1.3.0 in practice -- this
skill assumes NDMF 1.3.0+ as a baseline throughout, since that's also where
`ErrorReport` starts existing.

**Parameter introspection** (`IParameterProvider`, `[ParameterProviderFor]`,
`ProvidedParameter`, `ParameterInfo`) -- 1.4.0+ (changelog #184: "Added a new
API to allow NDMF plugins to declare and introspect expressions parameter
usage"). Matching a provider through base classes and interfaces is also
1.4.0 (#198). `ProvidedParameter.DefaultValue` came in 1.5.0 (#312), and
`ParameterInfo.ForPreview(ComputeContext)` presumably did too: #312 added
`ComputeContext` support to `ParameterInfo`, and `ComputeContext` itself is
1.5.0. On a pre-1.4.0 project, skip the provider; the pass still has to write
the Expression Parameters asset itself either way (see
`expression-parameters.md`).

**NDMF Preview framework** (`IRenderFilter`, `IRenderFilterNode`,
`RenderGroup`, `ComputeContext`, `.PreviewingWith(...)`) -- 1.5.0+ (changelog:
"Added a framework that can be used to override the rendering of an object
without modifying the object itself"). It was introduced whole in 1.5.0; there
is no older, separate preview mechanism to fall back to. On a project pinned
below 1.5.0, skip Step 4 of `SKILL.md` entirely -- there's no live-preview
option available, only the build-time path.

**`IAssetSaver`/`context.AssetSaver.SaveAsset(...)`, `SerializationScope`/
`context.OpenSerializationScope()`, `[DependsOnContext]`** -- 1.6.0+. Before
1.6.0, explicit asset persistence went through a different, less structured
API (direct `AssetContainer` access); `api-reference.md`'s guidance to prefer
`SaveAsset` over that older pattern only applies once 1.6.0 is available. On
an older project, check the installed source under
`Packages/nadena.dev.ndmf/Editor` for whatever asset-persistence API that
version actually shipped, rather than assuming `IAssetSaver` exists.

**`AnimatorServicesContext` and the animation-safe editing layer**
(`AnimationIndex.RewriteObjectCurves`, `VirtualClip`, `ObjectPathRemapper`) --
1.7.0+ (changelog #467: "Added `AnimatorServicesContext` and lots of
supporting APIs for working with animator controllers"). This is the biggest
single version gate in this skill: the entire "Animation-safe editing" section
of `api-reference.md` depends on `AnimatorServicesContext`, and there is no
clean equivalent before 1.7.0. A pass targeting an older NDMF that still needs
to keep animation clips consistent with a live value it changes has to walk
and rewrite `AnimatorController`/`AnimationClip` data by hand with plain Unity
animation APIs -- a materially different and more error-prone approach that
this skill does not document. Confirm the version before promising a user this
layer is available. Individual members of this layer kept accreting after
1.7.0 rather than shipping all at once -- e.g. `ObjectPathRemapper
.GetAllPathsForObject` specifically dates to 1.11.0 -- so on a project between
1.7.0 and the current release, check the installed source's member list
rather than assuming every method `api-reference.md` documents shipped
simultaneously with the class. (`context.ActivateExtensionContextRecursive<T>()`,
used for unit-test activation of this context, isn't itemized in the
changelog at all; it's confirmed present in installed 1.14.0 source but its
actual introduction version is unverified -- treat 1.7.0 as a floor for it,
not a confirmed exact version.)

**`INDMFEditorOnly`** (the interface `api-reference.md`'s "Package & component
conventions" section and `common-patterns.md`'s settings-component pattern
both recommend for a settings `MonoBehaviour`) -- 1.7.0+. The changelog lists
this under the name `INDMFEditorOnlyComponent`; the shipped name is
`INDMFEditorOnly` (confirmed both in installed 1.14.0 source and in a
prerelease changelog note about which assembly it needed to live in), so
don't be thrown by the name mismatch if searching the changelog directly. On
a project pinned below 1.7.0, fall back to the hand-rolled `#if
NDMF_VRCSDK3_AVATARS` / VRChat `IEditorOnly` guard that `api-reference.md`
otherwise recommends against.

**`AvatarProcessor.ManualProcessAvatar()`** -- 1.10.0+. `api-reference.md`'s
"the whole build is meant to run inside `AssetDatabase.StartAssetEditing`"
gotcha names this method as the entry point that wraps the whole pipeline in
`StartAssetEditing` (the intended behavior), and treats the manual bake as
the reference run. On an older project this entry point doesn't exist under
this name. There may be an older manual-bake path, and it may not open that
scope, so check the installed source and NDMF's menu items rather than
assuming.

**Publicly exposed `PropCache` / `nadena.dev.ndmf.preview.PropCacheDebug.
InvalidateAllCaches()`** -- flagged here as 1.10.0+, per the changelog entry
"Exposed `PropCache` class for `ComputeContext`-aware memoization." This is a
softer claim than the others above: the changelog entry describes the
*public* class being exposed, but the `PropCacheDebug.InvalidateAllCaches()`
helper `preview-api.md`'s testing section relies on may have existed
internally before that, or may have shipped at a different point than the
public `PropCache` type itself. Don't treat 1.10.0 as gospel for this one
specifically -- verify against the installed source
(`Packages/nadena.dev.ndmf/Editor/PreviewSystem` or wherever `PropCacheDebug`
lives in the installed version) before telling a user their older project
can't use it.

**Single-step build debugger** -- 1.14.1+. Already fully documented in
`references/debugging.md`, including its own version-gate note; this entry
exists only so this file's feature list is complete. Don't duplicate that
file's content here -- load it directly when the debugger itself is relevant.

**Shadow bones for preview** (`PreviewContext`, `IShadowBoneManagerHandle`
with `GetBone`/`ReplaceBone`, `ShadowBoneTestFixture`) -- 1.15.0+
(changelog #839 "Added ShadowBoneManager"; the public entry point is
`PreviewContext.Instance.ShadowBoneManager`, while the `ShadowBoneManager`
class itself stays `internal`). Releases before 1.15.0 had an internal
shadow-bone class that plugins couldn't use, so there's no fallback for
previewing a bone change below 1.15.0. Details are in `shadow-bones.md`.

**`IParameterProvider.ObserveDependencies(ComputeContext)`** -- 1.15.0+,
a default (no-op) interface member. Declaring it on a provider also compiles
against older NDMF (it's just never called there), down to 1.5.0 where
`ComputeContext` appears. See `expression-parameters.md`.

**`ProvidedParameter.NumericBitWidth` / `Signedness` / `LerpDuration`,
`ParameterSignedness`, and the matching `ParameterInfo.ConflictType` values**
-- 1.15.0+ (changelog #861 "Support advanced features of ExpressionsParameter
on IParameterProvider"). NDMF's built-in avatar-descriptor provider fills
them from the existing Expression Parameters asset only when
`com.vrchat.avatars` is 3.10.6 or later (asmdef define
`NDMF_VRCSDK3_AVATARS_PARAMETER_ADVANCED_FEATURES`). On an older SDK they
stay unset for those entries. The SDK side (`Parameter.bitWidth`,
`flippedRepresentation`, `useCustomLerpDuration`, `customLerpDuration`) is
itself new in VRChat SDK 3.10.6. That release also makes hand-written
field-by-field copies of a `Parameter` lossy. See `expression-parameters.md`,
"Writing the asset yourself", which was checked against SDK 3.10.6.

## Known breaking/removed APIs and behavior changes

These cases in NDMF's history are worth knowing about specifically because
they show that "this API exists in version X" isn't always a stable fact even
one patch release later -- a brand-new API can be pulled again quickly if it
turns out to cause compatibility problems, and a patch release can change a
signature or a behavior without the changelog calling it out:

- **`IExtensionContext.Owner`** was added in 1.6.0 and removed again in 1.6.1
  (released the same day) "due to compatibility issues," per the changelog.
  Anyone who wrote code against it in the few hours between those releases
  would have had it break immediately.
- **`IVirtualizedMotion`** moved from NDMF's main API namespace into
  `nadena.dev.ndmf.animator` in 1.7.2 -- an explicitly semver-breaking change
  per the changelog.
- **`ChangeNotifier.NotifyObjectUpdate(int instanceId)`** became
  `NotifyObjectUpdate(EntityId instanceId)` in 1.14.7, as part of the Unity 6
  support work. The changelog doesn't mention it. On Unity versions before
  6000.2, `EntityId` is a struct NDMF defines itself
  (`nadena.dev.ndmf.EntityId`) with an implicit conversion *to* `int` but
  none *from* it, so existing code passing an `int` stops compiling. Pass the
  `UnityEngine.Object` to the `NotifyObjectUpdate(Object)` overload instead,
  which exists on both sides of the change.
- **NDMF Preview's per-frame proxy reset got broader in 1.14.2.** The API
  didn't change, but `ProxyObjectController.OnPreFrame()` switched from
  copying a fixed list of renderer fields to copying the whole serialized
  renderer component, so a proxy renderer property a filter set only once in
  `Instantiate`/`Refresh` (one that was outside the old list) now gets reverted
  every frame. See `preview-api.md`'s `OnFrame` gotcha for details and the
  fix.
- **NDMF Preview's component queries got narrower in 1.15.0** (#847).
  `ComputeContext.GetComponent*`/`GetComponentsInChildren` now invalidate
  only when the query's own result changes, not on any component
  add/remove. A filter that only updated because some *other* component
  change happened to invalidate it goes stale after upgrading. See
  `preview-api.md`, "Not tracked unless you ask".
- **Preview filter exceptions are caught from 1.15.0** (#849). An exception
  from `GetTargetGroups`, `Instantiate`, or `Refresh` (or a `null` from
  `Instantiate`) is logged once as `Preview filter <type> failed in ...` and
  the filter or group is skipped. Before 1.15.0 it made the pipeline rebuild
  continuously, repeating the error. See `preview-api.md`'s checklist.
- **`VirtualClip.SetFloatCurve` / `SetObjectCurve` validate the binding in
  1.15.0.** They throw `ArgumentException` when `binding.type` is `null` or
  isn't a `UnityEngine.Object` type. The changelog doesn't mention it. Before
  1.15.0 these calls didn't reject an `EditorCurveBinding` without a `type`;
  now they throw. Always set `type` (e.g. with
  `EditorCurveBinding.FloatCurve(path, typeof(SkinnedMeshRenderer),
  "blendShape.X")`).

The practical takeaway: don't assume an API is safe to depend on just because
this file or another reference file names a version it "shipped in" -- if the
project's installed version is very close to that version (especially a
patch release or two above it), double-check the installed source directly,
the same way this skill already recommends doing whenever a reference file
and the installed package disagree.
