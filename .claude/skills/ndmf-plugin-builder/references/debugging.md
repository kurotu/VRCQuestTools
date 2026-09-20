# Single-Step Build Debugger (NDMF >= 1.14.1)

NDMF 1.14.1 added an in-Editor tool for stepping through an avatar build one
pass (or one extension-context activation/deactivation) at a time, on an
isolated clone, so you can inspect intermediate state instead of only the
final result. There's no official doc page for it yet -- the only changelog
entry is "Added single-step debugger functionality" -- so treat the installed
source (`Packages/nadena.dev.ndmf/Editor/UI/SingleStepBuildWindow.cs`) as the
primary reference if anything here looks off in a later version.

**Version gate:** this ships in 1.14.1, not 1.14.0. If the project's installed
NDMF predates 1.14.1, this tool doesn't exist yet -- fall back to the
"verify by hand" flow in `testing.md` instead.

## Opening it

`Tools > NDM Framework > Debug Tools > Single-step build`. The "Target
avatar" field auto-fills from whatever GameObject is selected in the
Hierarchy when the window opens.

## What "Start debugging" does

It builds against a **clone** of the target avatar, never the original --
the clone is instantiated a couple meters away in the scene and named
`<original name> (NDMF Single-Step)`. That clone is also automatically
excluded from NDMF's regular live preview, so a half-built intermediate state
never bleeds into the Scene view preview of the real avatar.

Under the hood it decomposes the normal build into a flat list of steps --
one per pass body execution, and one per extension-context activate/deactivate
-- instead of running the whole pipeline in one call.

## Stepping through it

- **Step Forward** runs exactly one step.
- Double-clicking any step in the list jumps straight to just before that
  step, running forward or backward as needed.
- **Step Back is not a real undo.** Passes aren't guaranteed reversible, so
  stepping backward actually discards the clone and rebuilds a fresh one,
  replaying every step from scratch up to (but not including) the target
  step. If a pass you're debugging has side effects beyond mutating the
  avatar hierarchy the debugger tracks -- writing external files, mutating
  shared/static state, anything not undone by simply discarding the clone --
  stepping backward re-triggers those side effects rather than undoing them.
  Keep this in mind before using Step Back to debug a pass you suspect of
  doing something like that.

## Ending a session

- **Stop and Keep Clone** detaches the clone from the debugger and leaves it
  in the scene at whatever step it had reached, so you can keep inspecting it
  (Inspector, hierarchy, a screenshot) after the debugger window itself moves
  on.
- Closing the window or triggering a domain reload (e.g. a recompile)
  discards the live session, but the window remembers which step it was on
  and offers **Resume debug session** next time it opens -- as long as the
  rebuilt plugin/pass list still maps that step uniquely. If enough changed
  (passes added/removed/reordered) that it no longer resolves uniquely,
  resume isn't offered and you restart from the beginning instead.

## When to reach for this vs. the usual verify-by-hand flow

- Final output wrong, and you just need to see the end state? The
  build-and-inspect habit in `testing.md`'s "verify by hand" section is
  enough.
- Need to know *when in the pipeline* something goes wrong -- e.g. whether a
  bug appears before or after another plugin's pass runs, or whether an
  extension context is active when you expect it to be -- this window shows
  that directly, one step at a time, without scattering temporary
  `Debug.Log` calls or breakpoints across passes you don't own.
- It's an interactive Editor tool for a human driving the Editor, not
  something to script or assert against from a test.
