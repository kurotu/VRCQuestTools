---
description: Start requested work on a new branch cut from the latest master
argument-hint: <task description>
---

Before doing any of the requested work below, set up a clean branch:

1. Fetch the latest `master` from the remote and make sure it's up to date locally.
2. Create a new branch from the latest `master`. Choose a descriptive, kebab-case name that reflects the task (e.g. `feature/<short-description>` or `fix/<short-description>`, a pattern common in this repo's recent branches).
3. Switch to that new branch. Do not continue work on whatever branch is currently checked out.

If the current branch has uncommitted changes unrelated to this task, leave them exactly as they are — do not stash, commit, revert, or otherwise touch them.

Once the new branch is ready, perform the following task on it:

$ARGUMENTS

When the task is complete, commit the changes on this branch.
