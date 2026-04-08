# Tum docker-compose servislerini durdurur
param(
    [switch]$Volumes
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

$downArgs = @('compose', 'down')
if ($Volumes) {
    $downArgs += '-v'
}

Write-Host "[docker-down] docker $($downArgs -join ' ')..." -ForegroundColor Cyan
docker @downArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[docker-down] Tamam." -ForegroundColor Green
