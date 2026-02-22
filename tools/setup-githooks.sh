#!/usr/bin/env bash
# Run from repo root to configure git to use versioned hooks.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
HOOKS_DIR="$REPO_ROOT/.githooks"

cd "$REPO_ROOT"

if ! command -v dotnet &>/dev/null; then
  echo "Note: dotnet not found — pre-commit will skip generate-slnx if .slnx exists"
fi

git config core.hooksPath "$HOOKS_DIR"
echo "Git hooks path set to: $HOOKS_DIR"
echo "Run 'git config --unset core.hooksPath' to revert to default .git/hooks"
