# Tools and Git hooks

Repo-local tooling lives in **tools/** and versioned Git hooks in **.githooks/**.

## One-time setup

From repo root, run once to point Git at the versioned hooks:

**Windows (PowerShell):**
```powershell
./tools/setup-githooks.ps1
```

**Unix / Git Bash:**
```bash
./tools/setup-githooks.sh
```

To revert: `git config --unset core.hooksPath`.

## generate-slnx

**tools/generate-slnx.cs** keeps the solution file (`.slnx`) in sync with:

- **Deployment/** — top-level files only
- **docs/** — all `*.md` files, one solution folder per docs subdirectory
- **Solution Items** — root files (excluding `*.slnx`, `*.user`, `pull_request_template.md`)

Run manually from repo root:

```bash
dotnet run --file tools/generate-slnx.cs
```

Requires .NET SDK. The pre-commit hook runs this when staged paths include docs, Deployment, or root files; use `git commit --no-verify` to skip once.

## Pre-commit hook

**.githooks/pre-commit** (and **pre-commit.ps1** for PowerShell) runs the slnx generator when a commit touches relevant paths. If the generator updates the `.slnx`, the hook stages it. No other checks run in these hooks.

## Reference

- [tools/README.md](../../tools/README.md) — In-repo summary and usage.
