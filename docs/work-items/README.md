# Work items

Work items are **discrete tasks** for the project: features, fixes, refactors, or docs. They live in markdown so they’re easy to edit, review in PRs, and keep in version control.

## What goes where

| File | Purpose |
|------|--------|
| **[current.md](current.md)** | Active work: things you’re doing or plan to do next. One list; reorder by priority if you like. |
| **[done.md](done.md)** | Done pile: items moved here when finished. Keeps a record of what was completed and when. |

## How to use work items

### Creating a work item

1. Open **[current.md](current.md)**.
2. Add a new entry using the format below.
3. Optionally assign area, priority, or acceptance criteria.

### Format for one item

Use a consistent block so items are easy to scan and move:

```markdown
### [Area] Short title
- **Summary:** One line describing the task.
- **Acceptance (optional):** What “done” looks like.
- **Added:** YYYY-MM-DD (optional).
```

You can drop “Acceptance” or “Added” if you don’t need them. “Area” can be e.g. `Backend`, `Blazor`, `Docs`, `Aspire`, `Tests`.

### Moving an item to the done pile

1. Open **[current.md](current.md)** and copy the full block for the item (including the `###` heading).
2. Open **[done.md](done.md)** and paste at the **top** of the “Done” section (newest first).
3. Add a **Completed:** line with the date (and optionally a commit or PR link).
4. Remove the item from **current.md**.

Example of how it looks in done:

```markdown
### [Backend] Add health check for event store
- **Summary:** Expose readiness for RavenDB in /health.
- **Completed:** 2025-02-21
- *(Optional)* **Ref:** abc1234 or PR #42
```

### Keeping the lists useful

- **current.md:** Keep only what you’re actively working on or about to start. If the list gets long, move “someday” ideas to a separate backlog or trim.
- **done.md:** Append-only by date. No need to remove old entries; it’s a simple history of completed work.

## Quick reference

| Action | Where |
|--------|--------|
| Add a new task | [current.md](current.md) |
| Mark something done | Move block to [done.md](done.md), add **Completed:** date, remove from current |
| See what’s in progress | [current.md](current.md) |
| See what’s finished | [done.md](done.md) |
