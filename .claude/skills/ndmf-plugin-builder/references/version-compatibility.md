# NDMF API Version Compatibility (1.3.0 -> 1.14.1)

This skill's other reference files describe the API surface as it exists in a
recent NDMF release, but a real project can be pinned to whatever version it
was last updated against. Most of the APIs this skill teaches were added
incrementally, not present from day one -- if the project's installed NDMF
predates an API's introduction, that API genuinely isn't there yet, and code
written against it won't compile or won't behave as documented.

This file's version data reflects the upstream changelog
(`https://github.com/bdunderscore/ndmf/blob/main/CHANGELOG.md`, or the vendored
copy at `Packages/nadena.dev.ndmf/CHANGELOG.md` if one is present in the
project) as of **1.14.1 (2026-07-18)**. It was compiled by reading that
changelog, not by testing every API against every version, so treat every
version number below the same way the rest of this skill treats API names:
as a strong lead, not an unconditional fact -- if the installed source or
changelog disagrees with this file, trust the installed source and correct
this file. For anything released after 1.14.1, this file will simply be
silent on it; check the changelog directly.

Contents:

- **Freshness** -- how to tell if this file itself is stale, and what to do
  about it
- **Checking the installed version** -- where to look
- **Feature availability** -- minimum version for each API this skill teaches,
  and what to do on an older project
- **Known breaking/removed APIs** -- two cases where a just-added API didn't
  survive to the next patch

## Freshness

**Last verified: 1.14.1 (2026-07-18).** There's no automation keeping this in
sync with new NDMF releases -- whoever next touches this skill (via
`skill-creator`, per `AGENTS.md`) should spend one glance checking whether
that's still current before assuming the entries below cover everything:

1. Check the latest version at
   `https://github.com/bdunderscore/ndmf/blob/main/CHANGELOG.md` (or the
   vendored `Packages/nadena.dev.ndmf/CHANGELOG.md` if it's newer than what's
   installed here).
2. If it's still 1.14.1, this file needs nothing.
3. If not, skim the new entries for anything that changes what this skill
   teaches (a new API worth adding, an existing one deprecated/moved/removed --
   see "Known breaking/removed APIs" below for what that looks like), update
   the relevant entry or add a new one, and bump this line plus the file's
   title/banner version.

This is a manual trigger, not a scheduled job -- it only fires when someone is
already in here for another reason. That's a deliberate, low-overhead choice:
NDMF ships patch releases often enough that a standing automated check would
mostly report "nothing new," and the entries that do matter (a removed API, a
new one worth documenting) need human judgment to write well, not just a diff.

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
"asset import is batched" gotcha references this method by name as one of the
paths that opens a serialization scope across the whole pipeline; on an older
project this specific manual-test-bake entry point doesn't exist under this
name (there may be an older manual-bake path -- check the installed source
and NDMF's menu items rather than assuming).

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

## Known breaking/removed APIs

Two cases in NDMF's history are worth knowing about specifically because they
show that "this API exists in version X" isn't always a stable fact even one
patch release later -- a brand-new API can be pulled again quickly if it
turns out to cause compatibility problems:

- **`IExtensionContext.Owner`** was added in 1.6.0 and removed again in 1.6.1
  (released the same day) "due to compatibility issues," per the changelog.
  Anyone who wrote code against it in the few hours between those releases
  would have had it break immediately.
- **`IVirtualizedMotion`** moved from NDMF's main API namespace into
  `nadena.dev.ndmf.animator` in 1.7.2 -- an explicitly semver-breaking change
  per the changelog.

The practical takeaway: don't assume an API is safe to depend on just because
this file or another reference file names a version it "shipped in" -- if the
project's installed version is very close to that version (especially a
patch release or two above it), double-check the installed source directly,
the same way this skill already recommends doing whenever a reference file
and the installed package disagree.
