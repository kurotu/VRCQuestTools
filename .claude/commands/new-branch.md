---
description: Start requested work on a new branch cut from the latest master
argument-hint: <task description>
---

Before doing any of the requested work below, set up a clean branch:

1. Fetch the latest `master` from the remote and make sure it's up to date locally.
2. Create a new branch from the latest `master`. Choose a descriptive, kebab-case name that reflects the task (following this repository's existing conventions, e.g. `feature/<short-description>` or `fix/<short-description>`).
3. Switch to that new branch. Do not continue work on whatever branch is currently checked out.
4. If the current branch has uncommitted changes unrelated to this task, do not discard, stash, or silently carry them over — pause and ask the user how they'd like to handle them before creating the new branch.

Once the new branch is ready, perform the following task on it:

$ARGUMENTS

When the task is complete, commit the changes on this branch.
