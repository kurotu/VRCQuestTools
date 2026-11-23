---
name: ndmf-plugin-builder
description: "Use this skill when the user is writing, fixing, or configuring code for an NDMF plugin (nadena.dev.ndmf) -- a Unity Editor pass that modifies a VRChat avatar only in the build-time copy, never the source scene, prefabs, or assets. Covers Plugin<T>/Pass<T>/BuildContext, build phases (Resolving, Generating, Transforming, Optimizing), and ordering a plugin with BeforePlugin/AfterPlugin relative to Modular Avatar, VRCFury, or Avatar Optimizer (AAO). Also covers making a custom component AAO-friendly (no 'unknown component' warnings), rewriting animator and animation-clip references, NDMF Preview (IRenderFilter, ComputeContext) for live Scene-view results, ErrorReport, and tests. Also use it to turn a destructive avatar editor tool into one that applies at build time, even if the user never says 'NDMF'. Don't use it for end-user troubleshooting that needs no code (package version errors, how to set up Modular Avatar components) or for generic Unity editor tools."
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
  incrementally between 1.3.0 and 1.15.0, not present from day one. If there's
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
4. Report problems through `ErrorReport`, not exceptions. NDMF catches an
   exception thrown from a pass, but that pass stops halfway, the user gets a
   raw stack trace as an `InternalError`, and the VRChat upload is blocked,
   even when the problem only deserved a warning. With `ErrorReport` you choose
   the severity: `NonFatal` warns and lets the upload proceed, and `Error`
   blocks it with a readable message. It needs a `Localizer`; see the
   `ErrorReport` entry in `references/api-reference.md`.
5. Leave no component you define for Avatar Optimizer (AAO, qualified name
   `"com.anatawa12.avatar-optimizer"`) to find. Do this for every plugin that
   defines a component, whether or not the user mentions AAO. AAO is
   installed alongside a large share of NDMF tools, and its Optimizing-phase
   pass flags every component type it doesn't recognize as *unknown*: it
   keeps the component, keeps everything the component references, and warns
   the user on every build. To users, that warning reads as "this tool is
   incompatible with AAO." For a build-time settings component:
   - Destroy it (`Object.DestroyImmediate`) on **every** exit path of the
     pass, including the "feature disabled / nothing to do" early return.
     That skipped early return is the usual reason a component leaks; a
     `try`/`finally` makes the destroy unconditional.
   - Find it with `GetComponentsInChildren<T>(true)` and destroy *every*
     instance, so you also catch components on inactive GameObjects and
     duplicates you ignored.
   - If the pass runs in **Optimizing**, declare
     `.BeforePlugin("com.anatawa12.avatar-optimizer")` (the string overload is
     a soft dependency, safe without AAO installed). Destroying the component
     only helps if it happens *before* AAO runs. If AAO's passes go first,
     they have already flagged the component, and kept everything it
     references, by the time your pass removes it. Do this even if your pass
     happens to run first today: without a constraint, NDMF orders plugins
     within a phase by assembly discovery order, which can change between
     projects. Passes in earlier phases always run before AAO.
   - `INDMFEditorOnly` does **not** hide a component from AAO. VRCSDK strips
     `IEditorOnly` components only after NDMF's Optimizing phase has finished
     (see `api-reference.md`'s VRChat pipeline section), so AAO still sees
     them.

   Load `references/aao-compatibility.md` only in two cases, which go beyond
   this:
   - The transform itself has to run **after** AAO, for example on AAO's
     merged meshes. The component still doesn't need to survive. Split the
     work: a pass before AAO reads the settings and destroys the component,
     and a pass with `.AfterPlugin("com.anatawa12.avatar-optimizer")` does
     the work.
   - A component genuinely has to exist when AAO runs, because it does
     something at runtime or AAO must keep objects it references alive. Then
     register it with AAO's API.
6. If the pass adds, removes, renames, reorders, or retypes VRChat Expression
   Parameters, load `references/expression-parameters.md` first. VRChat
   matches synced parameters between an avatar's PC, Android, and iOS builds
   by parameter name for avatars uploaded with VRChat SDK 3.11.0+, and by
   list position and type for older uploads. A plugin can't tell which SDK
   each build will be uploaded with, so the pass has to emit an identical
   list, with identical deterministic names, on every build target.
   Skipping a parameter on Android, or generating names or order that change
   from build to build, passes every build and upload. It only breaks sync
   between viewers on different platforms. Also declare the generated
   parameters with NDMF's `IParameterProvider` (1.4.0+) by default, so
   parameter-usage tools count them before the build. The file covers this,
   and generating MA Parameters as an alternative to writing the asset.

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
not after debugging why nothing shows up. If the preview has to move, scale,
or rotate a bone, or rebind a renderer to another bone, also load
`references/shadow-bones.md` (NDMF 1.15.0+).

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
before calling the plugin done, not only when something feels off. If Avatar
Optimizer is installed, run the smoke test with `AAO Trace And Optimize` on
the avatar, once with your feature enabled and once disabled, and confirm no
warning names one of your component types. If it isn't installed, say the AAO
check wasn't run; don't claim the plugin is compatible. Bugs that
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
