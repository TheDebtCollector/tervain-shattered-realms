---
tags:
  - tervain
  - development
  - git
created: 2026-10-25
---

# Version Control

## Repository

This Obsidian vault and the Godot project are versioned together. GitHub repository: https://github.com/TheDebtCollector/tervain-shattered-realms

The project is public and uses the MIT license selected by the developer. Git tracks source code, scenes, source asset files and their Godot import/UID metadata, design notes, and project configuration. It excludes generated Godot caches, build output, local IDE settings, and local Obsidian settings.

## Checkpoints after changes

After a completed, coherent change:

1. Run the checks appropriate to that change.
2. Review the diff and confirm that the commit includes only intended project work.
3. Commit with a clear message explaining the change.
4. Push the commit to GitHub. If pushing fails, retain the local commit and report the unsynced state.
5. Report the commit identifier and result to the developer.

Commit after meaningful completed changes, rather than every individual file edit. Never claim that a local commit is backed up online until the push has succeeded.

## Terms

- **Commit:** a named local checkpoint of tracked files.
- **Push:** uploads local commits to GitHub.
- **Pull:** downloads and integrates commits from GitHub. Pull does not undo changes.
- **Revert:** creates a new commit undoing a chosen earlier commit while preserving history.
- **Tag:** a stable name for an important checkpoint.

The initial checkpoint is tagged `sandbox-baseline-v1`: movement and combat feel accepted, sandbox mining/building implemented, and project documentation included.

## If a change is disappointing

Tell Codex which change to undo. For a committed and pushed change, prefer a revert commit and push that result. This keeps the history and makes the undo itself recoverable. Check for uncommitted work before any restoration so unrelated edits are preserved.

To inspect the baseline without changing the working branch:

```powershell
git show sandbox-baseline-v1
```

For an isolated experiment, start a branch such as `codex/building-experiment` from the current working version. A branch preserves the original line of development while the experiment is tested.

Git records project files and design notes. It does not provide game save/load; the prototype's in-game terrain changes remain temporary.


## Continue on another device

See [[Continue on Another Device]] for cloning/pulling `main`, opening this repository as an Obsidian vault, installing the Godot .NET prerequisites, and running the current project.
