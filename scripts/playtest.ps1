<#
.SYNOPSIS
  Run the automated playtest (Playwright UI tests) so the assistant or CI can "play" the game.

.DESCRIPTION
  Builds the test project, ensures Playwright browsers are installed, then runs UI tests
  that start an in-process Blazor host and drive the app with Playwright. No AppHost needed.
  Exit code 0 = all tests passed (game is playable); non-zero = failures.

.EXAMPLE
  .\scripts\playtest.ps1
  .\scripts\playtest.ps1 -SkipPlaywrightInstall
#>
[CmdletBinding()]
param(
    [switch] $SkipPlaywrightInstall
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
if (-not $RepoRoot) { $RepoRoot = (Get-Location).Path }
Set-Location $RepoRoot

$TestProject = Join-Path $RepoRoot "StarConflictsRevolt.Tests\StarConflictsRevolt.Tests.csproj"
$Filter = "FullyQualifiedName~StarConflictsRevolt.Tests.ClientTests.UITests"

Write-Host "Building test project..." -ForegroundColor Cyan
$build = dotnet build $TestProject -v q --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. If you see 'file is locked' or MSB3027, stop any running AppHost/Blazor/WebApi processes and try again." -ForegroundColor Red
    exit $LASTEXITCODE
}

if (-not $SkipPlaywrightInstall) {
    $playwrightScript = Join-Path $RepoRoot "StarConflictsRevolt.Tests\bin\Debug\net10.0\playwright.ps1"
    if (Test-Path $playwrightScript) {
        Write-Host "Installing Playwright browsers (if needed)..." -ForegroundColor Cyan
        & pwsh -NoProfile -Command "& { Set-Location '$RepoRoot'; & '$playwrightScript' install chromium }"
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Playwright browser install failed; continuing. Run: pwsh $playwrightScript install"
        }
    } else {
        Write-Warning "playwright.ps1 not found at $playwrightScript; run 'dotnet build' and retry, or install browsers manually."
    }
}

Write-Host "Running UI playtest (NavigationTests, HomePageTests, etc.)..." -ForegroundColor Cyan
dotnet test $TestProject --filter $Filter -v normal --no-build --nologo
$exitCode = $LASTEXITCODE
if ($exitCode -eq 0) {
    Write-Host "Playtest passed: game is playable." -ForegroundColor Green
} else {
    Write-Host "Playtest failed. Check output above." -ForegroundColor Red
}
exit $exitCode
