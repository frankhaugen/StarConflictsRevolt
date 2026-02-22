# Pre-commit hook: runs generate-slnx when relevant paths are staged.
# Requires PowerShell 7+. Copy tools/ and .githooks/ to any repo — discovers .slnx dynamically.

$ErrorActionPreference = "Stop"

# --- Sentinel values (do not modify without updating both pre-commit and pre-commit.ps1) ---
# Recursion guard: skip if generator is already running
$RecursionGuardEnv = "SLNX_GENERATOR_RUNNING"

# Root-level files excluded from triggering the generator
$ExcludedRootPatterns = @("*.slnx", "*.user")
$ExcludedRootFiles = @("pull_request_template.md")

# --- End sentinel values ---

if ([Environment]::GetEnvironmentVariable($RecursionGuardEnv, "Process") -eq "1") {
    exit 0
}

try {
    $repoRoot = & git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0) {
        exit 0
    }
} catch {
    exit 0
}

$scriptPath = Join-Path $repoRoot "tools/generate-slnx.cs"
if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) {
    exit 0
}

$slnxFiles = Get-ChildItem -Path $repoRoot -Filter "*.slnx" -File -ErrorAction SilentlyContinue |
    Sort-Object Name |
    Select-Object -First 1
if (-not $slnxFiles) {
    exit 0
}
$slnxFileName = $slnxFiles.Name

$staged = & git diff --cached --name-only 2>$null
if (-not $staged) {
    exit 0
}

$stagedList = $staged -split "`n" | Where-Object { $_.Trim() -ne "" }

function Test-IsRelevantPath {
    param([string]$path)
    if ($path -match "^docs/") { return $true }
    if ($path -match "^Deployment/") { return $true }
    if ($path -notmatch "/") {
        foreach ($pat in $ExcludedRootPatterns) {
            if ($path -like $pat) { return $false }
        }
        foreach ($name in $ExcludedRootFiles) {
            if ($path -eq $name) { return $false }
        }
        return $true
    }
    return $false
}

$relevant = @($stagedList | Where-Object { Test-IsRelevantPath $_ })
if ($relevant.Count -eq 0) {
    exit 0
}

# Skip if only .slnx file(s) are staged
$nonSlnx = @($stagedList | Where-Object { $_ -notmatch "\.slnx$" })
if ($nonSlnx.Count -eq 0) {
    exit 0
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "generate-slnx: warning: dotnet not found, skipping (commit allowed)"
    exit 0
}

Set-Location $repoRoot
Set-Item -Path "env:$RecursionGuardEnv" -Value "1"
try {
    $output = & dotnet run --file $scriptPath -- $slnxFileName 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "generate-slnx: generator failed. Run manually to fix:"
        Write-Host "  dotnet run --file tools/generate-slnx.cs -- $slnxFileName"
        Write-Host ""
        Write-Host $output
        exit 1
    }

    $diffOutput = & git diff -- $slnxFileName 2>$null
    if ($diffOutput) {
        & git add $slnxFileName
        Write-Host "generate-slnx: updated and staged $slnxFileName"
    } else {
        Write-Host "generate-slnx: no changes"
    }
} finally {
    Remove-Item -Path "env:$RecursionGuardEnv" -ErrorAction SilentlyContinue
}

exit 0
