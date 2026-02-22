# Run from repo root to configure git to use versioned hooks.
# Windows-first onboarding without requiring Git Bash/WSL.

$ErrorActionPreference = "Stop"

$hooksDir = Join-Path (Split-Path -Parent $PSScriptRoot) ".githooks"
$hooksPath = ".githooks"

Push-Location (Split-Path -Parent $PSScriptRoot)
try {
    & git config core.hooksPath $hooksPath
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to set core.hooksPath"
    }
    Write-Host "Git hooks path set to: $hooksPath"
    Write-Host "Run 'git config --unset core.hooksPath' to revert to default .git/hooks"
} finally {
    Pop-Location
}
