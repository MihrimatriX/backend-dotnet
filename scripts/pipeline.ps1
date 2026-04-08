# Tam otomasyon: restore/build -> (istege bagli) docker stack -> bekle -> migration -> test
#
# Kullanim:
#   .\scripts\pipeline.ps1                          # sadece restore + build + tum testler (Docker Testcontainers icin)
#   .\scripts\pipeline.ps1 -WithDockerStack         # compose ayaga + API bekle + migrate + test + compose indir
#   .\scripts\pipeline.ps1 -WithDockerStack -KeepStack

param(
    [switch]$WithDockerStack,
    [switch]$KeepStack,
    [switch]$IntegrationOnly,
    [switch]$SkipTests,
    [switch]$SkipMigrate
)

$ErrorActionPreference = 'Stop'
$scriptRoot = $PSScriptRoot

Write-Host '========== pipeline basliyor ==========' -ForegroundColor Magenta

& "$scriptRoot\restore-build.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($WithDockerStack) {
    & "$scriptRoot\docker-up.ps1" -Detached
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$scriptRoot\wait-postgres.ps1"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    & "$scriptRoot\wait-api.ps1"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    if (-not $SkipMigrate) {
        & "$scriptRoot\migrate.ps1"
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}

if (-not $SkipTests) {
    if ($IntegrationOnly) {
        & "$scriptRoot\test.ps1" -IntegrationOnly
    }
    else {
        & "$scriptRoot\test.ps1"
    }
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

if ($WithDockerStack -and -not $KeepStack) {
    & "$scriptRoot\docker-down.ps1"
}

Write-Host '========== pipeline tamam ==========' -ForegroundColor Green
