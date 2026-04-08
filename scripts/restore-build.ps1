# dotnet restore + build
# Kullanim: .\scripts\restore-build.ps1 [-Configuration Release]

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

Write-Host "[restore-build] dotnet tool restore..." -ForegroundColor Cyan
dotnet tool restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[restore-build] dotnet restore..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[restore-build] dotnet build -c $Configuration --no-restore..." -ForegroundColor Cyan
dotnet build -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[restore-build] Tamam." -ForegroundColor Green
