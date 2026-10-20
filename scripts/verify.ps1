# Proje kökünden: .\scripts\verify.ps1
# Docker ile API + vitrin derlemesini doğrular. Entegrasyon testleri: .\scripts\verify.ps1 -RunTests (Docker Desktop açık olmalı).

param(
    [switch] $RunTests
)

$ErrorActionPreference = "Stop"
$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Set-Location $root

Write-Host "==> Docker: API imajı (Release publish)" -ForegroundColor Cyan
docker build -t ecommerce-backend:local .

Write-Host "==> Docker: temp-shop imajı" -ForegroundColor Cyan
docker build -t ecommerce-shop:local ./temp-shop

if ($RunTests) {
    Write-Host "==> Entegrasyon testleri (Testcontainers + Docker soketi)" -ForegroundColor Cyan
    docker run --rm `
        -v "${root}:/src" `
        -v //var/run/docker.sock:/var/run/docker.sock `
        -w /src `
        -e TESTCONTAINERS_RYUK_DISABLED=true `
        mcr.microsoft.com/dotnet/sdk:10.0 `
        dotnet test EcommerceBackend.sln -c Release -v minimal
}

Write-Host "`nTamam. Stack: docker compose up --build -d" -ForegroundColor Green
