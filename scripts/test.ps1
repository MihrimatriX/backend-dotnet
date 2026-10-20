# Testleri calistirir. Entegrasyon testleri icin Docker (Testcontainers) acik olmali.
# Kullanim:
#   .\scripts\test.ps1
#   .\scripts\test.ps1 -IntegrationOnly

param(
    [switch]$IntegrationOnly,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

if ($IntegrationOnly) {
    Write-Host "[test] Filtre: Category=Integration (Docker gerekli)" -ForegroundColor Yellow
}

Write-Host "[test] dotnet test -c $Configuration..." -ForegroundColor Cyan
$testArgs = @('test', 'EcommerceBackend.sln', '-c', $Configuration)
if ($IntegrationOnly) {
    $testArgs += @('--filter', 'Category=Integration')
}

dotnet @testArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[test] Tamam." -ForegroundColor Green
