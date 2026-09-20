---
name: ndmf-plugin-builder
description: "Build, port, or debug NDMF (Non-Destructive Modular Framework, nadena.dev.ndmf) plugins and passes -- Unity Editor code that transforms a VRChat avatar at build time (duplicating materials, rewriting AnimatorController/animation-clip references, adding or removing components) without touching the source scene, prefabs, or assets. Also covers NDMF Preview (IRenderFilter, ComputeContext) to show a transform live in the Scene view without building, pass phases and ordering (e.g. relative to Modular Avatar or VRCFury), and testing passes. Use whenever the user writes or debugs an NDMF plugin or pass; mentions Plugin<T>, Pass<T>, BuildContext, or nadena.dev.ndmf; or wants an avatar-modifying editor tool to become non-destructive / apply only at build time -- even if they never say 'NDMF'."
---

# NDMF Plugin Builder

Guides implementing an NDMF (`nadena.dev.ndmf`) plugin: a Unity Editor pass that
transforms a VRChat/Unity avatar non-destructively at build time, leaving the
source scene, prefabs, and assets untouched.

## Why this shape of problem bites people

An NDMF pass runs against a *build-time copy* of the avatar. Anything you want
to end up in the built avatar -- a swapped material, a rewritten animation, a
removed helper component -- has to be produced by your code, not by hand-editing
the source project. This has one recurring practical consequence worth
internalizing before writing anything: **every live reference to the thing
you're changing usually has an animated counterpart you need to change too**
(a renderer's current material *and* any animation clip that swaps it; a
component's current field value *and* any animation curve driving that field).
Forgetting the animated side is the single most common way an NDMF pass looks
correct in the Editor and breaks the moment an animation plays. Keep this in
mind through every step below.

## Step 1 -- Read the official best practices first

Before designing anything, fetch and read `https://ndmf.nadena.dev/best-practices.html`
with WebFetch. It's short, it's authoritative, and it can change independently
of this skill -- don't rely on a summary baked into this file going stale.
Re-fetch it if it's been a while since you last read it in this session.

## Step 2 -- Confirm the shape of the plugin

Before writing code, make sure you know:

- **What triggers the transform** -- a settings `MonoBehaviour` the user adds
  to the avatar, or an unconditional pass that runs on every avatar? Most
  non-trivial plugins want a settings component (see
  `references/common-patterns.md`) so users can opt in/out and configure it
  per-avatar.
- **Which build phase** the transform belongs in (Resolving / Generating /
  Transforming / Optimizing), and **whether it needs to run before or after
  another known plugin** -- most commonly Modular Avatar, qualified name
  `"nadena.dev.modular-avatar"`. Get the phase semantics from the best-practices
  page you just fetched, and the exact ordering API from
  `references/api-reference.md` -- don't guess; ordering bugs are subtle and
  only surface when the *other* plugin happens to be installed.

  **VRChat-specific:** For VRChat avatars NDMF splits its VRCSDK preprocessor
  callbacks; when VRCFury is installed, its hook (roughly -10000) runs
  *between* the Transforming and Optimizing phases (NDMF callback orders -11000
  and -1025). Entering
  Play mode for a VRChat avatar also goes through the full VRCSDK preprocessor
  chain rather than calling NDMF directly, so this sandwich ordering applies in
  Play mode too. See the "VRChat build pipeline integration & VRCFury ordering"
  section of `references/api-reference.md` before debugging any pass whose
  behavior differs between a normal NDMF build and a VRChat build or Play mode
  entry. When in doubt, check the installed source at
  `Packages/nadena.dev.ndmf`; it is always authoritative over this skill's
  summaries.
- **What exactly gets duplicated or rewritten, and whether an animation clip
  could reference it.** If in doubt, assume yes and read the animation section
  of `references/api-reference.md`.
- **What NDMF version the project has installed** -- check
  `Packages/nadena.dev.ndmf/package.json`'s `version` field or the Package
  Manager window. Most of the API this skill teaches (`ErrorReport`,
  `AnimatorServicesContext`, NDMF Preview, `IAssetSaver`, and more) was added
  incrementally between 1.3.0 and 1.14.1, not present from day one. If there's
  any doubt the project might be on an older release, load
  `references/version-compatibility.md` before relying on API from
  `references/api-reference.md` or `references/preview-api.md`.

Load `references/api-reference.md` now for the confirmed API surface
(`Plugin<T>`/`Pass<T>`, `BuildContext` members, the animator-services layer,
package/component conventions). Treat it as the primary source for exact
method names rather than re-deriving them from memory -- NDMF's extension-context
activation in particular has more than one method with subtly different
behavior, and guessing wrong compiles fine but breaks quietly.

## Step 3 -- Implement

1. Package layout and component conventions: see the "Package & component
   conventions" section of `references/api-reference.md`.
2. Plugin + Pass skeleton: see the "Plugin & Pass registration" section of
   `references/api-reference.md`.
3. For the specific transform you're building, check
   `references/common-patterns.md` -- it covers the two patterns that come up
   in almost every plugin (non-destructively duplicating a shared asset like a
   material, and rewriting animation references to match), including two
   concrete Unity API gotchas discovered the hard way: they compile fine and
   produce silently wrong output.
4. Report problems through `ErrorReport`, not exceptions -- a thrown exception
   aborts the *entire* avatar build for a problem the user could otherwise be
   warned about and continue past.

## Step 4 -- Add NDMF Preview support (only if asked)

If the user also wants to *see* the transform's effect in the Scene view
without running a full build -- they say "NDMF Preview," "preview," "live
preview," "see it without building," or mention
`IRenderFilter`/`RenderGroup`/`ComputeContext` -- that's NDMF Preview
(`nadena.dev.ndmf.preview`), a separate API layered on top of the pass you
just built, not a variant of it. Load `references/preview-api.md` before
designing it: it covers the registration API, and in particular several
quiet-by-design behaviors that compile fine, pass naive unit tests, and
produce no error -- the only symptom in every case is preview silently doing
nothing. Materials must be re-applied every frame in
`IRenderFilterNode.OnFrame`, not just set once; `GetTargetGroups` must
exclude renderer types NDMF's preview proxy can't handle (e.g.
`ParticleSystemRenderer`), or the *entire* filter's preview goes dark for
every avatar in the scene; and anything `GetTargetGroups` reads through a
plain Unity API call instead of a `context.*` call -- including your own
settings component, found via a raw lookup after `context.GetAvatarRoots()`
-- is invisible to the reactivity system and never triggers a recompute when
it changes (preview renders once, then goes stale -- or never appears at
all, if the component was added after the first compute). All three look the
same from the Scene view, so read the whole file before writing the filter,
not after debugging why nothing shows up.

Skip this step entirely if the user only asked for the build-time transform.

## Step 5 -- Test

Load `references/testing.md` for how to write EditMode tests against a
hand-built minimal avatar (NDMF's distributed package ships no test helpers of
its own), plus two non-obvious blockers that otherwise waste time: unsaved
scene changes blocking a test run before it even starts, and a domain reload
needing a moment to settle before tests will run right after a compile. Separately,
driving a pass inside a test needs `ActivateExtensionContextRecursive<T>`,
not the non-recursive `ActivateExtensionContext<T>` -- that one is covered in
the "In unit tests" bullet of `references/api-reference.md`'s animation
section, not in `testing.md`'s blockers. If you added NDMF Preview support in
Step 4, `references/preview-api.md` has its own testing section -- notably a
`ComputeContext` caching gotcha that produces mysterious "object not found"
failures unrelated to your filter's actual logic.

Compile and run tests through whatever this project's Unity tooling is (check
for project-specific skills or a CLAUDE.md pointer first; don't assume a
generic `dotnet test` or CLI path works for a Unity package).

## Step 6 -- Verify for real, then review

The last section of `testing.md` explains why a real Editor smoke test and a
code-review pass both matter specifically for this kind of code -- do both
before calling the plugin done, not only when something feels off. Bugs that
compile fine and pass a naive test are the norm for Unity API work like this,
not the exception: this skill's own reference material documents two bugs
that were caught only by hands-on inspection and review, not by the initial
implementation or first test pass.

If a smoke test shows wrong output but it's unclear *where in the pipeline*
it goes wrong -- e.g. whether it's your pass, an ordering issue with another
plugin, or an extension-context transition -- NDMF 1.14.1+ ships a single-step
build debugger that steps through the build one pass at a time on an isolated
clone. See `references/debugging.md` for how to use it and a gotcha around
its Step Back button before relying on it.
