Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

<############################
Standalone repository bootstrapper

This script scaffolds a multi-universe / multi-book writing repo:
- universes/<universe>/universe.yaml
- universes/<universe>/books/<book>/book.yaml
- chapters/*.md (one chapter per file)
- scripts/metrics.ps1
- build.ps1 (pandoc -> PDF via xelatex)
- .github/workflows/build-books.yml

Usage:
  pwsh ./bootstrap.ps1

Notes:
- Intended to be run in an EMPTY folder.
- It will create or overwrite a set of files.
#############################>

# ---------- Config ----------
$UniverseId   = 'inheritance-of-a-starship'
$UniverseName = 'Inheritance of a Starship'
$BookId       = 'book-01'
$BookTitle    = 'Inheritance of a Starship'

# Audible target: ~14 hours => ~130k words
$TargetWords  = 130000

# ---------- Helpers ----------
function Ensure-Dir([Parameter(Mandatory)] [string]$Path) {
  New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Write-File([Parameter(Mandatory)] [string]$Path, [Parameter(Mandatory)] [string]$Content) {
  $dir = Split-Path -Parent $Path
  if ($dir) { Ensure-Dir $dir }

  # Normalize newlines to LF for stable diffs across platforms.
  $normalized = $Content -replace "`r`n", "`n"
  Set-Content -Path $Path -Value $normalized -Encoding utf8
}

function Append-Gitignore([Parameter(Mandatory)] [string]$Path, [Parameter(Mandatory)] [string[]]$Lines) {
  $existing = @()
  if (Test-Path $Path) {
    $existing = Get-Content $Path -ErrorAction SilentlyContinue
  }

  $set = [System.Collections.Generic.HashSet[string]]::new([string[]]$existing)
  foreach ($l in $Lines) { [void]$set.Add($l) }

  Write-File $Path (($set | Sort-Object) -join "`n")
}

function Assert-EmptyOrConfirm {
  $entries = Get-ChildItem -Force -ErrorAction SilentlyContinue
  if ($null -eq $entries -or $entries.Count -eq 0) { return }

  Write-Warning "This folder is not empty. Running this will create/overwrite files."
  $answer = Read-Host "Continue? (y/N)"
  if ($answer -notin @('y', 'Y', 'yes', 'YES', 'Yes')) {
    throw 'Aborted by user.'
  }
}

Assert-EmptyOrConfirm

# ---------- Root files ----------
Write-File '.editorconfig' @"
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true

[*.md]
trim_trailing_whitespace = false

[*.yml]
indent_style = space
indent_size = 2

[*.yaml]
indent_style = space
indent_size = 2
"@

Append-Gitignore '.gitignore' @(
  'out/',
  '.DS_Store',
  '*.log',
  '*.aux',
  '*.toc',
  '*.out',
  '*.synctex.gz',
  '*.fdb_latexmk',
  '*.fls'
)

Write-File 'README.md' @'
# $UniverseName (Repo)

This repo stores **multiple universes**, each with **multiple books**, where each chapter is a **Markdown document**.
It can compile books to **PDF** and generate **metrics** (word count, target completion, estimated audio length).

## Structure

- `universes/<universe>/universe.yaml` — universe metadata
- `universes/<universe>/books/<book>/book.yaml` — book metadata (includes `target_words`)
- `universes/<universe>/books/<book>/chapters/*.md` — one chapter per file (lexical order)
- `universes/<universe>/reference/sections/*.md` — reference manual for editors (optional compile)
- `build/` — templates
- `scripts/metrics.ps1` — generates per-book metrics
- `build.ps1` — compiles PDFs

## Local build

Prereqs:
- `pandoc`
- `xelatex` (TeX Live / MikTeX)

Build one book:

```pwsh
pwsh ./build.ps1 -Universe inheritance-of-a-starship -Book book-01
```

Build reference manual (if present):

```pwsh
pwsh ./build.ps1 -Universe inheritance-of-a-starship
```

Build everything in a universe:

```pwsh
pwsh ./build.ps1 -Universe inheritance-of-a-starship -All
```

Generate metrics:

```pwsh
pwsh ./scripts/metrics.ps1 -Universe inheritance-of-a-starship -Book book-01
```
'@

# ---------- Build template ----------
Ensure-Dir 'build'
Write-File 'build/template.tex' @'
\documentclass[12pt]{article}
\usepackage[margin=1in]{geometry}
\usepackage{hyperref}
\usepackage{parskip}
\usepackage{setspace}
\usepackage{titlesec}

\setstretch{1.15}
\titleformat{\section}{\Large\bfseries}{\thesection}{0.75em}{}
\titleformat{\subsection}{\large\bfseries}{\thesubsection}{0.75em}{}

\hypersetup{
colorlinks=true,
linkcolor=blue,
urlcolor=blue
}

\begin{document}
$if(title)$
\begin{center}
{\LARGE $title$}\par
$if(subtitle)$
\vspace{0.5em}
{\large $subtitle$}\par
$endif$
$if(author)$
\vspace{1em}
{\large $author$}\par
$endif$
\vspace{2em}
\end{center}
$endif$

$body$

\end{document}
'@

# ---------- Scripts: metrics ----------
Ensure-Dir 'scripts'
Write-File 'scripts/metrics.ps1' @'
param(
  [Parameter(Mandatory = $true)][string] $Universe,
  [Parameter(Mandatory = $true)][string] $Book
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$BookRoot    = Join-Path 'universes' $Universe | Join-Path -ChildPath ('books/' + $Book)
$ChaptersDir = Join-Path $BookRoot 'chapters'
$BookMeta    = Join-Path $BookRoot 'book.yaml'

if (-not (Test-Path $ChaptersDir)) { throw "Missing: $ChaptersDir" }
if (-not (Test-Path $BookMeta))    { throw "Missing: $BookMeta" }

$ChapterFiles = Get-ChildItem $ChaptersDir -Filter *.md | Sort-Object Name
if ($ChapterFiles.Count -eq 0) { throw "No chapters found in $ChaptersDir" }

function Read-TargetWords([Parameter(Mandatory)] [string]$Path) {
  $line = (Get-Content $Path) |
    Where-Object { $_ -match '^\s*target_words\s*:\s*\d+\s*$' } |
    Select-Object -First 1

  if (-not $line) { return $null }
  [int]($line -replace '^\s*target_words\s*:\s*', '')
}

function Get-WordCount([Parameter(Mandatory)] [string]$Text) {
  # Remove fenced code blocks (```...```
  $noCode = [regex]::Replace($Text, '```[\s\S]*?```', '', [System.Text.RegularExpressions.RegexOptions]::Multiline)

  # Remove images ![alt](url)
  $noImages = [regex]::Replace($noCode, '!\[[^\]]*\]\([^\)]*\)', '')

  # Replace links [text](url) -> text
  $noLinks = [regex]::Replace($noImages, '\[([^\]]+)\]\([^\)]*\)', '$1')

  # Strip some remaining markdown punctuation (note: includes backtick)
  $plain = $noLinks -replace '[#>*_`~\-]+', ' '

  ([regex]::Matches($plain, "\b[\p{L}\p{N}’']+\b")).Count
}

function Count-Tokens([Parameter(Mandatory)] [string]$Text, [Parameter(Mandatory)] [string[]]$Needles) {
  $count = 0
  foreach ($n in $Needles) {
    $count += [regex]::Matches($Text, [regex]::Escape($n)).Count
  }
  $count
}

$TargetWords = Read-TargetWords $BookMeta

$chapters = @()
$TotalWords = 0
$TotalTodos = 0

foreach ($f in $ChapterFiles) {
  $raw = Get-Content $f.FullName -Raw
  $words = Get-WordCount $raw
  $todos = Count-Tokens $raw @('TODO', 'FIXME', 'TK')

  $TotalWords += $words
  $TotalTodos += $todos

  $chapters += [pscustomobject]@{
    file  = $f.Name
    words = $words
    todos = $todos
  }
}

$WordsPerHour = 9300.0
$hours = [math]::Round($TotalWords / $WordsPerHour, 2)
$minutes = [math]::Round($hours * 60, 0)

$completion = if ($TargetWords -and $TargetWords -gt 0) {
  [math]::Min(1.0, [math]::Round($TotalWords / [double]$TargetWords, 4))
} else {
  $null
}

$ThinChapters  = ($chapters | Where-Object { $_.words -lt 800 }).Count
$EmptyChapters = ($chapters | Where-Object { $_.words -eq 0 }).Count

$OutDir = Join-Path 'out' (Join-Path $Universe 'metrics')
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

$metrics = [pscustomobject]@{
  universe                    = $Universe
  book                        = $Book
  chapters                    = $chapters.Count
  total_words                 = $TotalWords
  target_words                = $TargetWords
  completion                  = $completion
  estimated_audio_hours       = $hours
  estimated_audio_minutes     = $minutes
  todo_count                  = $TotalTodos
  empty_chapters              = $EmptyChapters
  thin_chapters_lt_800_words  = $ThinChapters
  generated_utc               = (Get-Date).ToUniversalTime().ToString('o')
  per_chapter                 = $chapters
}

$metricsJson = Join-Path $OutDir ("$Book.metrics.json")
$metrics | ConvertTo-Json -Depth 6 | Set-Content -Encoding utf8 $metricsJson

$md = @()
$md += "# Metrics — $Universe / $Book"
$md += ''
$md += "- Chapters: **$($metrics.chapters)**"
$md += ("- Words: **$($metrics.total_words)**" + $(
  if ($TargetWords) { " / $TargetWords (**$([math]::Round(($completion) * 100, 1))%**)" } else { '' }
))
$md += "- Est. audio: **$hours h** (~$minutes min) @ 9,300 wph"
$md += "- TODO/FIXME/TK: **$($metrics.todo_count)**"
$md += "- Empty chapters: **$EmptyChapters**"
$md += "- Thin chapters (<800 words): **$ThinChapters**"
$md += ''
$md += '## Per chapter'
$md += ''
$md += '| Chapter | Words | TODOs |'
$md += '|---|---:|---:|'
foreach ($c in $chapters) {
  $md += "| $($c.file) | $($c.words) | $($c.todos) |"
}

$metricsMd = Join-Path $OutDir ("$Book.metrics.md")
($md -join "`n") | Set-Content -Encoding utf8 $metricsMd

Write-Host "Metrics: $metricsJson"
Write-Host "Summary: $metricsMd"
'@

# ---------- Build script ----------
Write-File 'build.ps1' @'
param(
  [Parameter(Mandatory = $true)][string] $Universe,
  [string] $Book = '',
  [switch] $All
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Require([Parameter(Mandatory)] [string]$Cmd) {
  if (-not (Get-Command $Cmd -ErrorAction SilentlyContinue)) {
    throw "Missing '$Cmd'. Install it and try again."
  }
}

Require 'pandoc'

$repo = Split-Path -Parent $PSCommandPath
$uPath = Join-Path $repo (Join-Path 'universes' $Universe)
if (-not (Test-Path $uPath)) { throw "Universe not found: $Universe" }

$outDir = Join-Path $repo (Join-Path 'out' $Universe)
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$template = Join-Path $repo (Join-Path 'build' 'template.tex')

function Get-MdFiles([Parameter(Mandatory)] [string]$Path) {
  Get-ChildItem -Path $Path -Filter *.md | Sort-Object Name | ForEach-Object FullName
}

function Build-Pdf(
  [Parameter(Mandatory)] [string]$Title,
  [Parameter(Mandatory)] [string]$MetaFile,
  [Parameter(Mandatory)] [string[]]$Inputs,
  [Parameter(Mandatory)] [string]$OutputFile
) {
  Write-Host "Building: $Title -> $OutputFile"
  & pandoc @Inputs `
    --metadata-file=$MetaFile `
    --from=gfm `
    --pdf-engine=xelatex `
    --template=$template `
    -o $OutputFile
}

# Reference manual build (if present) when -All or no -Book is specified
$refMeta = Join-Path $uPath (Join-Path 'reference' 'reference.yaml')
$refSections = Join-Path $uPath (Join-Path 'reference' 'sections')
if ((Test-Path $refMeta) -and (Test-Path $refSections)) {
  if ($All -or [string]::IsNullOrWhiteSpace($Book)) {
    $refInputs = Get-MdFiles $refSections
    $refOut = Join-Path $outDir 'reference.pdf'
    Build-Pdf 'Reference manual' $refMeta $refInputs $refOut
  }
}

# Books
$booksRoot = Join-Path $uPath 'books'
if (-not (Test-Path $booksRoot)) { throw "No books folder in universe: $Universe" }

$bookFolders = if ($All) {
  Get-ChildItem -Path $booksRoot -Directory | Sort-Object Name
} elseif (-not [string]::IsNullOrWhiteSpace($Book)) {
  @((Get-Item (Join-Path $booksRoot $Book)))
} else {
  @()
}

foreach ($bf in $bookFolders) {
  $bookMeta = Join-Path $bf.FullName 'book.yaml'
  $chapters = Join-Path $bf.FullName 'chapters'

  if (-not (Test-Path $bookMeta)) { throw "Missing book.yaml: $($bf.Name)" }
  if (-not (Test-Path $chapters)) { throw "Missing chapters folder: $($bf.Name)" }

  $inputs = Get-MdFiles $chapters
  $bookOut = Join-Path $outDir ("$($bf.Name).pdf")
  Build-Pdf $bf.Name $bookMeta $inputs $bookOut
}

Write-Host "Done. Outputs in: $outDir"
'@

# ---------- Universe + book content ----------
$uRoot = Join-Path 'universes' $UniverseId
Ensure-Dir $uRoot

Write-File (Join-Path $uRoot 'universe.yaml') @"
id: $UniverseId
name: $UniverseName
language: en-US
author: Frank
"@

# Book scaffold
$bookRoot = Join-Path $uRoot (Join-Path 'books' $BookId)
Ensure-Dir $bookRoot
Ensure-Dir (Join-Path $bookRoot 'chapters')
Ensure-Dir (Join-Path $bookRoot 'assets/images')

Write-File (Join-Path $bookRoot 'book.yaml') @"
title: "$BookTitle"
subtitle: ""
language: en-US
rights: "Copyright © Frank"
target_words: $TargetWords
"@

Write-File (Join-Path $bookRoot 'chapters/00-frontmatter.md') @"
# $BookTitle

*Draft build output. Formatting will evolve.*
"@

Write-File (Join-Path $bookRoot 'chapters/01-ch01.md') @"
# Chapter 1 — Lunch Break

Dockside Station — Midday

TODO: Write the inheritance call scene.
TODO: Establish protagonist's baseline life and tone.
"@

Write-File (Join-Path $bookRoot 'chapters/02-ch02.md') @"
# Chapter 2 — The Appointment

TODO: Lawyer meeting logistics and constraints.
"@

Write-File (Join-Path $bookRoot 'chapters/03-ch03.md') @"
# Chapter 3 — First Boarding

TODO: Initial ship entry, first sensory contradictions, AI text-only.
"@

# Reference scaffold
Ensure-Dir (Join-Path $uRoot 'reference/sections')
Ensure-Dir (Join-Path $uRoot 'reference/assets/diagrams')

Write-File (Join-Path $uRoot 'reference/reference.yaml') @"
title: "Reference Manual — $UniverseName"
subtitle: "Editor perusal copy"
language: en-US
rights: "Internal"
"@

Write-File (Join-Path $uRoot 'reference/sections/01-overview.md') @"
# Overview

- Universe: $UniverseName
- Core premise: inheritance of a neglected ship, refurb, liability accumulation.
"@

Write-File (Join-Path $uRoot 'reference/sections/02-timeline.md') @"
# Timeline

TODO: Add a simple absolute timeline later (avoid lore dumps early).
"@

Write-File (Join-Path $uRoot 'reference/sections/03-law.md') @"
# Law & Jurisdiction

TODO: Define the legal baselines that matter for the story (only what will bite later).
"@

Write-File (Join-Path $uRoot 'reference/sections/04-factions.md') @"
# Factions

TODO: Keep minimal until they appear on-page.
"@

Write-File (Join-Path $uRoot 'reference/sections/05-technology.md') @"
# Technology

TODO: Define what the ship can/can't do, especially constraints that drive plot.
"@

Write-File (Join-Path $uRoot 'reference/sections/06-characters.md') @"
# Characters

TODO: Track stable character facts and state changes as the series grows.
"@

# Optional canon folder
Ensure-Dir (Join-Path $uRoot 'canon')
Write-File (Join-Path $uRoot 'canon/glossary.md') @"
# Glossary

TODO: Terms you don't want to drift over a long series.
"@

# ---------- GitHub Actions workflow ----------
Ensure-Dir '.github/workflows'
Write-File '.github/workflows/build-books.yml' @'
name: build-books

on:
  pull_request:
  push:
    branches: [ "main" ]
  workflow_dispatch:

jobs:
  detect:
    runs-on: ubuntu-latest
    outputs:
      matrix: ${{ steps.set-matrix.outputs.matrix }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - id: changed
        uses: tj-actions/changed-files@v45
        with:
          files: |
            universes/**
            build/**
            build.ps1
            scripts/**

      - id: set-matrix
        shell: pwsh
        run: |
          $files = @"
${{ steps.changed.outputs.all_changed_files }}
"@ -split "`n" | Where-Object { $_ -and $_.Trim() -ne '' }

          $buildSystemChanged = $files | Where-Object {
            $_ -like 'build/*' -or $_ -eq 'build.ps1' -or $_ -like 'scripts/*'
          }

          $bookKeys = [System.Collections.Generic.HashSet[string]]::new()

          foreach ($f in $files) {
            if ($f -match '^universes/([^/]+)/books/([^/]+)/') {
              $null = $bookKeys.Add("$($Matches[1])|$($Matches[2])")
            }
          }

          if ($buildSystemChanged.Count -gt 0) {
            Get-ChildItem 'universes' -Directory | ForEach-Object {
              $u = $_.Name
              $booksRoot = "universes/$u/books"
              if (Test-Path $booksRoot) {
                Get-ChildItem $booksRoot -Directory | ForEach-Object {
                  $null = $bookKeys.Add("$u|$($_.Name)")
                }
              }
            }
          }

          $include = @()
          foreach ($k in $bookKeys) {
            $p = $k.Split('|')
            $include += @{ universe = $p[0]; book = $p[1] }
          }

          $matrixJson = (@{ include = $include } | ConvertTo-Json -Compress)
          "matrix=$matrixJson" | Out-File -FilePath $env:GITHUB_OUTPUT -Append

  build:
    needs: detect
    if: ${{ fromJson(needs.detect.outputs.matrix).include[0] != null }}
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix: ${{ fromJson(needs.detect.outputs.matrix) }}
    steps:
      - uses: actions/checkout@v4

      - name: Install pandoc + LaTeX
        run: |
          sudo apt-get update
          sudo apt-get install -y pandoc texlive-xetex texlive-fonts-recommended texlive-latex-extra

      - name: Build PDF
        shell: pwsh
        run: |
          pwsh ./build.ps1 -Universe ${{ matrix.universe }} -Book ${{ matrix.book }}

      - name: Metrics
        shell: pwsh
        run: |
          pwsh ./scripts/metrics.ps1 -Universe ${{ matrix.universe }} -Book ${{ matrix.book }}

      - name: Upload PDF
        uses: actions/upload-artifact@v4
        with:
          name: ${{ matrix.universe }}-${{ matrix.book }}-pdf
          path: out/${{ matrix.universe }}/${{ matrix.book }}.pdf

      - name: Upload metrics
        uses: actions/upload-artifact@v4
        with:
          name: ${{ matrix.universe }}-${{ matrix.book }}-metrics
          path: out/${{ matrix.universe }}/metrics/${{ matrix.book }}.metrics.*

  pr-comment:
    needs: [detect, build]
    if: ${{ github.event_name == 'pull_request' && fromJson(needs.detect.outputs.matrix).include[0] != null }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Download metrics artifacts
        uses: actions/download-artifact@v4
        with:
          path: artifacts

      - name: Build PR comment body
        id: body
        shell: pwsh
        run: |
          $mdFiles = Get-ChildItem -Recurse artifacts -Filter *.metrics.md | Sort-Object FullName
          if ($mdFiles.Count -eq 0) {
            'body=No metrics produced.' | Out-File $env:GITHUB_OUTPUT -Append
            exit 0
          }

          $lines = @('## Book metrics', '')
          foreach ($f in $mdFiles) {
            $lines += "### $(($f.BaseName -replace '\\.metrics$', ''))"
            $lines += (Get-Content $f.FullName -Raw)
            $lines += ''
          }

          'body<<EOF' | Out-File $env:GITHUB_OUTPUT -Append
          ($lines -join "`n") | Out-File $env:GITHUB_OUTPUT -Append
          'EOF' | Out-File $env:GITHUB_OUTPUT -Append

      - name: Comment on PR
        uses: marocchino/sticky-pull-request-comment@v2
        with:
          header: book-metrics
          message: ${{ steps.body.outputs.body }}
'@

Write-Host 'Bootstrap complete.'
Write-Host 'Next:'
Write-Host '  git init'
Write-Host '  git add .'
Write-Host '  git commit -m "Bootstrap"'
Write-Host ''
Write-Host 'Local build:'
Write-Host "  pwsh ./build.ps1 -Universe $UniverseId -Book $BookId"
Write-Host 'Metrics:'
Write-Host "  pwsh ./scripts/metrics.ps1 -Universe $UniverseId -Book $BookId"
