# Calisan dotnet-backend konteynerinde demo veriyi yukler (PostgreSQL stack).
# Once: docker compose up -d (veya scripts/docker-up.ps1 -Detached)
#
#   .\scripts\docker-seed-demo.ps1

$ErrorActionPreference = 'Stop'
Write-Host '[docker-seed-demo] dotnet-backend icinde seed-demo calistiriliyor...' -ForegroundColor Cyan
docker exec dotnet-backend dotnet EcommerceBackend.dll seed-demo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host '[docker-seed-demo] Tamam.' -ForegroundColor Green
