# Adding or changing VRChat Expression Parameters

Load this when a pass adds, removes, renames, reorders, or retypes entries in
the avatar's `VRCExpressionParameters` -- directly, or by generating a
menu/animator whose parameters end up there -- or when anything about the
parameters a pass emits could depend on the build target. It also covers
declaring a component's parameters to NDMF with `IParameterProvider`, and
generating Modular Avatar's MA Parameters instead of editing the asset.

Contents:

- **Why: platform builds are matched by name (SDK 3.11.0+) or by position**
- **The rule for a pass**, and the usual ways a pass breaks it
- **Declaring the parameters with `IParameterProvider`**
- **Writing the asset yourself**
- **Alternative: generate MA Parameters instead**
- **Checking it**

## Why: platform builds are matched by name (SDK 3.11.0+) or by position

A cross-platform avatar is several separate uploads under one avatar ID: a PC
build, an Android build, and optionally an iOS build, built from one avatar
(with or without per-platform overrides). Viewers receive the wearer's synced
parameter values from whichever build the wearer is running, and apply them to
the build *they* loaded. How a value finds its parameter in the other build
depends on the VRChat SDK version:

- **Avatars uploaded with VRChat SDK 3.11.0 or later: by parameter name.**
  VRChat's announcement of SDK 3.11.0-beta.1 says it "Restored name-based
  mapping for synced avatar parameters for avatars uploaded in SDK `3.11.0`
  and later", and addresses community package maintainers directly: "as of
  this SDK version, parameters representing the same thing must have the
  same name across builds of the same avatar for different platforms." A
  parameter whose name differs between the PC and Android builds has nothing
  to match in the other build, whatever its position.
- **Older uploads: by position and type.** VRChat's creator docs (Animator
  Parameters, "Cross-Platform Parameter Sync", last updated 2026-10-28) state
  that values are matched "by their position in the parameters list and their
  parameter type, not by the names of the parameters": "For a given parameter
  to sync between PC and Quest, it has to be in the same position in the
  parameter list, and have the same parameter type." They recommend using the
  same Expression Parameters asset for every platform "even if one version
  doesn't make use of all the parameters."

The same announcement says name-based mapping "was originally added in
`3.10.6` but then pulled back because of incompatibilities with user tooling",
and that it is limited to 3.11.0+ "to address incompatibilities with community
packages that were using different names for the same parameter across
platforms." (On the client side, VRChat 2026.4.1, build 1936, shipped
name-based mapping and then "reverted for now".) A plugin that generates names
per build is exactly what broke the first time.

The matching itself happens in the VRChat client, keyed on the SDK version an
avatar was uploaded with, so the SDK 3.11.0 release notes don't mention it,
and as of 2026-11-21 the Animator Parameters page still described only the
position rule. These details are open: whether the type (and the 3.10.6 bit
width and signedness) must still match, how an avatar is matched when its
platform builds were uploaded with different SDK versions, and what viewers
see for a name that one build lacks. The announcement came with a beta, so
also check that the behavior shipped as described in the stable release.

Nothing errors when the lists differ: the build and the upload both succeed.
The symptom only shows up across platforms at runtime. A PC wearer flips a
toggle, and Android viewers see nothing change, or, under the position rule,
see whatever parameter sits at that slot in the Android build change instead.
A plugin author testing on one platform never sees it.

A plugin can't tell which SDK the user will upload each platform build with,
and most plugins support SDKs older than 3.11.0. So keep both identical:

- **Names**, because SDK 3.11.0+ matches on them. Users who fix cross-platform
  order with Modular Avatar's Sync Parameter Sequence also depend on names. It
  records the primary platform's parameters, and its docs say "Your primary
  platform avatar must contain all of the parameters that are synced on any
  variant of your avatar. If any are missing, the build will fail." A name
  that differs per build never matches that record.
- **Order and types**, because older SDKs match on them, and nothing
  documented yet says the name rule ignores the type.

A pass that emits the same names, in the same order, with the same types,
works under either rule.

These are VRChat behaviors, not NDMF ones, and they can change. Before relying
on the details for a design decision, re-check
`https://creators.vrchat.com/avatars/animator-parameters/`, the SDK release
notes under `https://creators.vrchat.com/releases/`, and the client release
notes under `https://docs.vrchat.com/docs/`. The docs only talk about "PC and
Quest"; treat iOS builds the same way.

## The rule for a pass

Given the same avatar, the pass must produce the same Expression Parameters on
every platform build: the same entries, in the same order, with the same
`name`, `valueType`, and `networkSynced`. On VRChat SDK 3.10.6+, which adds
per-parameter bit width, signedness, and lerp duration (the docs describe them
under "Show Advanced Options"; see "Writing the asset yourself" for the
fields), keep those identical too. Don't reason about which entries are
"safe" to vary. Under the position rule, the docs don't say whether unsynced
entries count toward a synced parameter's position (the reverted 2026.4.1
release note's "synced parameter buffer" hints they don't, but that's not a
guarantee). Under the name rule, what matters beyond the name isn't documented
yet. Keep the whole list identical; that is correct under any reading.

Your pass can only control its own output. Other tools, or a per-platform
override with a different hierarchy, can still make the lists diverge. The
goal is that your plugin is never the cause. Don't try to repair other tools'
lists from your pass; end users have dedicated tools for that (Modular Avatar's
Sync Parameter Sequence component, for example).

### Ways a pass breaks it

1. **Deciding the parameter set from the build target.** Watch for
   `EditorUserBuildSettings.activeBuildTarget`, `#if UNITY_ANDROID`,
   `#if UNITY_IOS`, `#if UNITY_STANDALONE`, or any "is this mobile?" helper
   that decides *whether a parameter exists*, where it goes, or its type. If a
   feature can't work on mobile (its shader or component is stripped there,
   say), skip the renderer/animation work on that platform but still emit the
   parameter, in the same slot, with the same type. Likewise, don't shrink a
   parameter on mobile (Int to Bool, or a narrower bit width) to save synced
   bits: the position rule matches on type, and nothing documented says the
   name rule (SDK 3.11.0+) ignores it. The same applies to a
   settings flag like "enable on Android": it may gate the effect, not the
   parameter.

2. **Order that depends on object identity or discovery.** A list sorted by
   `GetInstanceID()`, `Object.FindObjectsOfType` results, or anything keyed by
   a Unity object's hash can come out in a different order in another Editor
   session or after a build-target switch. Iterating a `HashSet<T>` or
   `Dictionary<TKey, TValue>` is stable in practice when you only insert, but
   it isn't guaranteed, so don't let it decide the order either. Iterate in
   hierarchy order (`GetComponentsInChildren<T>(true)` on the avatar root) or
   sort by a stable key with `StringComparer.Ordinal` (culture-sensitive
   comparison can order differently on a machine with another locale). If the
   hierarchy itself can differ between platform variants, sort by name rather
   than hierarchy position.

3. **Names that aren't deterministic.** `Guid.NewGuid()`, `GetInstanceID()`,
   `GetHashCode()` of a Unity object, a counter shared across avatars, or a
   timestamp in a generated parameter name produces a different name on every
   build. From SDK 3.11.0, which matches builds by name, this alone breaks
   sync for that parameter even when its position and type line up. The
   converse matters too: a name must mean the same thing on every platform.
   A name built from a running counter over the components that survive on
   this platform (`Toggle_1`, `Toggle_2`, ...) pairs the wrong parameters
   together once Android strips one, which is worse than not syncing. Derive names from what the user configured (a name field on the
   component) or the object's path relative to the avatar root, under a fixed
   plugin-specific prefix. If you need a short unique suffix, hash that stable
   string with a fixed algorithm (e.g. SHA-1 of the path), not with object
   identity.

4. **Removing "unused" parameters per platform.** A pass that drops parameters
   the current build's animators don't read shifts every later parameter on
   that platform only, which breaks the position rule. What the name rule
   (SDK 3.11.0+) does with a name that one build lacks isn't documented, and
   the pass can't tell which rule the user's uploads will get anyway. If the
   decision depends on
   platform-specific content, keep the entry instead. An unused parameter
   costs synced bits, but keeps the slots aligned.

## Declaring the parameters with `IParameterProvider`

Declare the parameters your component generates with NDMF's parameter
introspection API (NDMF 1.4.0+). Do this by default, not only when asked:
without it, parameter-usage displays (Modular Avatar's parameter usage window,
for example) show the user a synced-bit total that leaves your parameters out
until after the build, and tools that remap parameter names can't see them.

Declaring a parameter does **not** add it to the Expression Parameters asset.
Your pass still has to do that. Give both one shared, pure method that
describes the parameters for a component, and call it from the provider and
from the pass. That keeps the usage estimate equal to what the build emits, and
because the method doesn't read the build target, every platform gets the same
list.

```csharp
using System.Collections.Generic;
using nadena.dev.ndmf;
using UnityEngine;

internal static class GlowToggleParameters
{
    // Pure and deterministic: no build target, no instance IDs.
    // The pass calls this too, so the build matches what usage UIs show.
    public static string NameFor(GlowToggle c) => "GlowToggle/" + c.parameterName;
}

[ParameterProviderFor(typeof(GlowToggle))]
internal class GlowToggleParameterProvider : IParameterProvider
{
    private readonly GlowToggle _component;

    // Required: a public constructor taking the ForType component.
    public GlowToggleParameterProvider(GlowToggle component) => _component = component;

    public IEnumerable<ProvidedParameter> GetSuppliedParameters(BuildContext context = null)
    {
        // Return NEW objects on every call; NDMF mutates them (e.g. EffectiveName on remap).
        yield return new ProvidedParameter(
            GlowToggleParameters.NameFor(_component),
            ParameterNamespace.Animator,
            _component,
            GlowTogglePlugin.Instance,         // Plugin<T>.Instance
            AnimatorControllerParameterType.Bool)
        {
            WantSynced = true,
            DefaultValue = 0,                  // NDMF 1.5.0+
        };
    }

    // RemapParameters has a default no-op body on Unity 2021+; override it only
    // if your component renames parameters for its children.
    // ObserveDependencies (NDMF 1.15.0+) also has a no-op default; see below.
}
```

Points that are easy to get wrong:

- The types live in the `nadena.dev.ndmf` namespace but in the
  **`nadena.dev.ndmf.vrchat`** assembly. Add it to the Editor asmdef's
  `references`, or the attribute and interface won't resolve.
- NDMF finds providers by scanning for `[ParameterProviderFor]` and calling a
  public constructor that takes the component. If there isn't one, NDMF logs
  an error once when it builds its provider table after a domain reload (easy
  to miss) and treats the component as providing nothing. The attribute also
  matches subclasses and implemented interfaces, so one provider can cover a
  base type.
- `GetSuppliedParameters` is called **outside** builds too (`context` is
  `null` then, e.g. for usage UIs). NDMF's interface documentation asks you
  not to change the number or type of synced parameters depending on the
  context, so that usage estimates stay correct. Treat it as the same rule as
  above: no build-target branches, no per-build names.
- Set `IsAnimatorOnly = true` for animator-internal parameters that must not
  go into the Expression Parameters asset, `WantSynced = false` for local-only
  ones, and `IsHidden = true` for internal parameters that usage UIs should
  not list.
- Providers are queried for components that exist at the time of the query.
  If your pass destroys its settings component (as SKILL.md Step 3 asks),
  compute everything it needs before destroying it.
- **If the provider reads anything besides its own component** (a referenced
  ScriptableObject or preset asset, another component, a child object),
  implement `ObserveDependencies` (NDMF 1.15.0+) and `Observe` those objects
  there. NDMF already observes the source component, but nothing else.
  Without this, preview-time consumers (`ParameterInfo.ForPreview`) keep
  showing the old parameters after the user edits the referenced asset.

  ```csharp
  public void ObserveDependencies(nadena.dev.ndmf.preview.ComputeContext context)
  {
      if (_component.preset != null) context.Observe(_component.preset);
  }
  ```

  NDMF calls it before every query, from builds and `ForUI` too, where the
  context is a no-op `ComputeContext.NullContext`. So keep it cheap and
  side-effect free, and don't treat a call as a sign that preview is running.
  The method is a default interface member, so declaring it compiles on
  older NDMF as well (NDMF just never calls it there). It needs the
  `nadena.dev.ndmf.preview` types, which exist from 1.5.0.
- **Narrower Int/Float parameters** (NDMF 1.15.0+): `ProvidedParameter` has
  `NumericBitWidth` (2-8, clamped; `null` means the default 8),
  `Signedness`, and `LerpDuration` (seconds, clamped to 0-300). `BitUsage`,
  which usage UIs add up, now follows `NumericBitWidth` for a synced
  Int/Float. As with everything here, declaring it doesn't change the asset.
  If your pass writes a narrower entry into Expression Parameters (the SDK
  fields are in "Writing the asset yourself"), set the same width on the
  provider so the estimate matches the build, and use the same width on
  every platform (see "The rule for a pass"). If you don't narrow, leave
  `NumericBitWidth` at `null` rather than setting 8.
  With `com.vrchat.avatars` 3.10.6+, NDMF's built-in descriptor provider also
  reports these options for the entries already in the user's asset. When two
  declarations of one name disagree, the first declarer wins (the avatar
  descriptor's asset, for a name already in it), and `ParameterInfo` reports
  the disagreement to the `onConflict` callback as `BitWidthMismatch`,
  `SignednessMismatch`, or `LerpDurationMismatch`. A declaration that leaves an
  option `null` doesn't conflict; it takes the other declaration's value.

### Choosing a narrower width

Two situations come up in practice.

- **A pass that generates a selector or counter.** For example, an Int that
  picks one of `k` outfits. The pass knows every value it will assign (menu
  control values, animator transition conditions, parameter drivers). Use the
  smallest width that holds all of them. VRChat's docs describe an Int as
  "0 to 255 (8 bits, unsigned)" by default; narrowing it lowers the maximum,
  and 2 bits is the minimum. An unsigned `n`-bit Int therefore holds
  `0..2^n-1`, so `k` options need `max(2, ceil(log2(k)))` bits. Signed
  "reduces the maximum value in exchange for being able to express negative
  numbers". For a Float, narrowing "reduces the float's precision", and
  unsigned "reduces the range of the float in exchange for higher precision"
  (0 to 1 instead of -1 to 1). Keep the default value and every value you
  assign inside the range the inspector label shows. VRChat's docs don't say
  what happens to a value outside it. Compute the width from the
  component's settings only, never from the build target ("The rule for a
  pass").
- **A component that lets users set these options.** Give each option a
  "not set" state (bit width `0` / a `Default` choice / "custom lerp" off).
  When it's not set, leave the `ProvidedParameter` property `null` and leave
  the asset entry's field as it is. The user's own asset settings and other
  tools' declarations then apply without being reported as conflicts. Only an
  option the user actually set should override them.

### Reading parameters back with `ParameterInfo`

`ParameterInfo.ForContext(context).GetParametersForObject(avatarRoot)` returns
the parameters declared on the avatar by every provider (yours, other tools',
and NDMF's built-in ones for the avatar descriptor's existing asset entries,
PhysBones, and contacts), in declaration order: each object's components, then
its children in hierarchy order, skipping `EditorOnly`-tagged objects. Use
`ParameterInfo.ForUI` from editor UI and `ParameterInfo.ForPreview(computeContext)`
from NDMF Preview code.

It **merges** declarations by name. Only the first declarer's
`ProvidedParameter` survives, and later ones are folded into it (types
reconciled, sync flags combined, and from 1.15.0 the numeric options above).
Two consequences:

- Don't use it to enumerate *your own* parameters by filtering on
  `p.Plugin == YourPlugin.Instance` or `p.Source`. A name that already exists
  in the user's asset is declared first by the avatar descriptor at the root,
  and two of your components that share a name come back as one entry with
  the first one's `Source`. Either way, a pass that builds one layer per
  returned entry silently skips some. Enumerate your own components and call
  your shared method instead.
- Do use it to look up the **effective** name of a parameter you declared:
  find the entry whose `OriginalName` is your name and read `EffectiveName`.
  It differs from the original only when a remapping tool (an MA Parameters
  rename on an ancestor, for example) applies. Those remaps are only visible
  while that tool's components still exist, so a pass that wants to honor
  them must run before that tool processes them (for Modular Avatar,
  `.BeforePlugin("nadena.dev.modular-avatar")`). Whatever name you settle on,
  use the same string for the asset entry, the animator parameter, and the
  menu control, or they silently stop matching.

## Writing the asset yourself

- If `VRCAvatarDescriptor.customExpressions` is `false`, VRChat ignores the
  descriptor's assets and uses its default menu and parameters. Set it to
  `true` when you add parameters.
- If `expressionParameters` (or `expressionsMenu`) is `null`, don't start from
  an empty asset: that drops VRChat's default parameters (`VRCEmote`,
  `VRCFaceBlendH`, `VRCFaceBlendV`) that the default menu and playable layers
  use. Start from a copy of the VRChat SDK's `DefaultExpressionParameters`
  asset (shipped under `Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/
  Expressions Menu/`), or create one with those entries.
- Otherwise the descriptor still points at the user's source asset. Clone it
  (`Object.Instantiate`), assign the clone to the descriptor, and call
  `ObjectRegistry.RegisterReplacedObject(original, clone)`, as in the
  duplication pattern in `common-patterns.md`. Do this unconditionally
  rather than trying to detect whether an earlier pass already cloned it.
- `parameters` is an array: build a new array and assign it rather than trying
  to grow it in place.
- **Carry the user's existing entries over as they are** (put the same
  `Parameter` objects from the cloned asset into the new array). Don't rebuild
  them as `new Parameter { name = p.name, valueType = p.valueType, ... }`.
  VRChat SDK 3.10.6 added four fields to `VRCExpressionParameters.Parameter`
  (`bitWidth`, `flippedRepresentation`, `useCustomLerpDuration`,
  `customLerpDuration`). A copy that lists fields by hand drops them, so the
  user's narrowed parameters silently go back to 8 bits, their sign and lerp
  settings are lost, and the synced-bit total changes. Code written before
  3.10.6 still compiles, so nothing warns you.
- **The 3.10.6 options on an entry you add** (checked against SDK 3.10.6):
  - A new `Parameter` defaults to `bitWidth = 8`, the type's usual sign
    (Int unsigned, Float signed), and no custom lerp. Entries in assets saved
    before 3.10.6 also read as 8 bits. So not setting anything is safe.
  - `bitWidth` (a `byte`) is used for Int and Float only; Bool is always 1
    bit. The effective width (`GetBitWidthByType()`) is clamped to 2-8,
    so `0` doesn't mean "default": it reads as 2 bits.
  - Signedness is stored as `flippedRepresentation`, *relative to the type*:
    `true` makes an Int signed and a Float unsigned. Read it back with
    `GetRepresentationByType()` rather than interpreting the flag yourself.
  - Lerp duration takes effect only with `useCustomLerpDuration = true`.
    `GetCustomLerpDuration()` returns `null` otherwise, and clamps to 0-300
    seconds.
  - `VRCExpressionParameters.CalcTotalCost()` counts the narrowed width, so a
    narrower entry also leaves more of the synced-bit budget for others.
  - These members don't exist before SDK 3.10.6. If the plugin supports older
    SDKs, add a `versionDefines` entry to your Editor asmdef
    (`{"name": "com.vrchat.avatars", "expression": "3.10.6", "define":
    "YOURPLUGIN_VRCSDK_PARAM_OPTIONS"}`) and wrap the code that sets them in
    `#if`. NDMF's own `NDMF_VRCSDK3_AVATARS_PARAMETER_ADVANCED_FEATURES`
    define comes from NDMF's asmdef and isn't visible in your assembly.
- If an entry with your name already exists, reuse it instead of appending a
  duplicate. If its type differs from yours, report it through `ErrorReport`
  rather than silently retyping it.

## Alternative: generate MA Parameters instead

If the plugin already depends on Modular Avatar, or can reasonably require it,
consider having the pass generate an MA Parameters component
(`nadena.dev.modular_avatar.core.ModularAvatarParameters`) on the build copy
and leaving the Expression Parameters asset to MA. NDMF's own
`IParameterProvider` documentation points to this route for plugins that need
name remapping ("converting to e.g. Modular Avatar Parameters objects at build
time").

What MA then handles for you:

- Writing the entries into the Expression Parameters asset. You don't clone
  or edit the asset yourself.
- Merging and reporting duplicate or conflicting declarations from several
  sources.
- Remapping: a user's MA Parameters on an ancestor object can rename your
  parameter, and "Auto rename" (`internalParameter = true`) gives it a unique
  name, so you don't have to invent a collision suffix.

```csharp
using nadena.dev.modular_avatar.core;

// One generated object holds everything that must see MA's renames.
var holder = new GameObject("GlowToggle");
holder.transform.SetParent(toggle.transform, false);

var maParams = holder.AddComponent<ModularAvatarParameters>();
maParams.parameters.Add(new ParameterConfig
{
    nameOrPrefix = GlowToggleParameters.NameFor(toggle),  // still deterministic
    syncType = ParameterSyncType.Bool,     // the type; NotSynced means animator-only
    localOnly = false,                     // true = in the asset but not network-synced
    saved = true,
    defaultValue = 0,
    hasExplicitDefaultValue = true,
});

// The animator and the menu go on the same holder (or below it), so a rename
// reaches them too.
var merge = holder.AddComponent<ModularAvatarMergeAnimator>();
merge.animator = generatedFxController;    // uses the same parameter name
holder.AddComponent<ModularAvatarMenuItem>(); // configure its Control likewise
```

**Narrower parameters through MA Parameters (MA 1.19.0+).** From MA 1.19.0,
`ParameterConfig` also carries the VRChat SDK 3.10.6 options, so a generated
MA Parameters entry can narrow a parameter without touching the asset:

```csharp
new ParameterConfig
{
    nameOrPrefix = OutfitParameters.NameFor(selector),
    syncType = ParameterSyncType.Int,
    bitWidth = 3,                                     // 0 = not set (8 bits)
    representation = ParameterRepresentation.Default, // or Unsigned / Signed
    useCustomLerpDuration = false,                    // with customLerpDuration (seconds)
    defaultValue = 0,
    hasExplicitDefaultValue = true,
};
```

These fields don't exist before MA 1.19.0. Declare `>=1.19.0` in the
package's MA dependency, or set them only behind a version define. MA writes
them into the asset only when the VRChat SDK is 3.10.6+. It clamps the
default value into the narrowed range, so a 3-bit Int with a default of 20
becomes 7 (checked with an MA 1.19.0 build). That keeps the asset valid but
hides the mistake, so compute the width from the values you assign. MA also declares
them through NDMF's parameter introspection, but keep your own provider's
`NumericBitWidth` in step with what you generate (see "Keep your own
`IParameterProvider`" below).

What to watch for:

- **It's a hard dependency.** The Editor asmdef needs a reference to
  `nadena.dev.modular-avatar.core`, and the package should declare MA in
  `vpmDependencies`. Without MA installed, nothing reads the component and the
  parameters never reach the asset. If MA should stay optional, keep the
  direct-asset path as the fallback; don't half-depend on it.
- **Run before MA.** Generate the components in the Generating phase (NDMF's
  phase for generating components that Modular Avatar will consume). A
  component added after MA has run is ignored.
- **`[DisallowMultipleComponent]`.** The target object may already carry the
  user's own MA Parameters, and Unity then refuses a second one (`AddComponent`
  doesn't give you a usable component). Put yours on a generated GameObject,
  as above.
- **Renames only reach what MA processes, and only below the MA Parameters
  object.** An MA rename (Auto rename, or a user's MA Parameters on an
  ancestor) applies to MA Merge Animator controllers, MA Menu Items and Menu
  Installers, and PhysBone/Contact parameters on that object and its
  children. It isn't applied to a layer your pass writes straight into the
  avatar's FX controller, to a menu you edit directly, or to components on the
  *parent* of the generated object. Those keep the original name and silently
  stop matching. If you go through MA Parameters, put the Merge Animator and
  Menu Item on the same generated object or below it, or don't rely on
  renaming.
- **The cross-platform rule still applies.** MA appends the parameters in
  hierarchy order, so a pass that generates MA Parameters only on some
  platforms, or with per-build names, breaks sync the same way. MA doesn't
  document how Auto rename picks names, so if a name must be stable across
  platform builds (from SDK 3.11.0, every synced name must), set an explicit
  deterministic name instead of Auto rename.
- **Keep your own `IParameterProvider`.** A component your pass creates only
  exists during the build. Usage UIs query the edit-time scene, so they only
  see these parameters if your source component declares them.

## Checking it

- Grep the pass for `activeBuildTarget`, `UNITY_ANDROID`, `UNITY_IOS`,
  `UNITY_STANDALONE`, `NewGuid`, `GetInstanceID`, and `FindObjectsOfType`
  near the code that builds the parameter list.
- In an EditMode test (see `testing.md`), build two identical avatars, run
  the pass on each, and assert that the resulting
  `(name, valueType, networkSynced)` lists are equal index by index (on SDK
  3.10.6+, also compare `GetBitWidthByType()` and `GetRepresentationByType()`).
  Comparing index by index covers both rules: the names that SDK 3.11.0+
  matches on, and the position and type that older SDKs match on. Fresh
  objects get fresh instance IDs, so this catches identity-based names. It
  doesn't reliably catch identity-based *ordering*: identically built avatars
  tend to get instance IDs in the same relative order. To catch that too,
  create the second avatar's objects in a different order, then set the same
  sibling order.
- For a real check, build the same avatar for Windows and Android and diff the
  two built parameter lists the same way.
