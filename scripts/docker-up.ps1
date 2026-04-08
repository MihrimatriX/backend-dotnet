# PostgreSQL + Redis + RabbitMQ + API
# Kullanim:
#   .\scripts\docker-up.ps1
#   .\scripts\docker-up.ps1 -Detached
#   .\scripts\docker-up.ps1 -Detached -WaitApi
# Ek docker compose argumanlari: .\scripts\docker-up.ps1 -Detached -- --force-recreate

param(
    [switch]$Detached,
    [switch]$WaitApi,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ExtraArgs
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

Write-Host '[docker-up] Stack: postgres, redis, rabbitmq, dotnet-backend, shop (temp-shop :3000)...' -ForegroundColor Cyan

$composeArgs = @('compose', 'up', '--build')
if ($Detached) {
    $composeArgs += '-d'
}
if ($ExtraArgs -and $ExtraArgs.Count -gt 0) {
    $composeArgs += $ExtraArgs
}

docker @composeArgs
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($WaitApi) {
    & "$PSScriptRoot\wait-api.ps1"
}
