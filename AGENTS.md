# Project workflow

This repository contains the Obsidian design vault and the Godot .NET/C# game in `tervain--shattered-realms/`.

- Read relevant notes in `Design/` before changing game behaviour. Keep useful design decisions and milestone notes in this vault.
- Preserve the accepted movement/combat baseline unless the developer requests a change or a concrete defect requires it.
- After each completed coherent change, run appropriate checks, review the diff, commit the intended work with a descriptive message, and push to the configured GitHub remote. The developer has requested this checkpoint workflow. Report the commit and whether it was pushed; retain local commits if network/authentication prevents a push.
- Do not stage unrelated user edits, generated caches, credentials, or local workspace settings.
- Prefer a revert commit when the developer asks to undo published work. Preserve unrelated uncommitted changes; do not reset or force-push shared history without explicit authorization.
- Use `codex/` as the prefix for new development branches.
- The project is public under MIT. Keep third-party license notices with imported assets or code.

See `Design/Version Control.md` for the developer-facing workflow.
