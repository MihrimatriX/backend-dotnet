# EF Core migration'lari PostgreSQL veritabanina uygular.
# docker-compose ile postgres ayaktayken varsayilan baglanti localhost:5432 kullanilir.
# Not: SQLite gelistirme ortaminda uygulama EnsureCreated kullanir; bu script Npgsql icindir.
#
# Kullanim:
#   .\scripts\migrate.ps1
#   .\scripts\migrate.ps1 -ConnectionString "Host=...;..."

param(
    [string]$ConnectionString = 'Host=localhost;Port=5432;Database=ecommerce_db;Username=ecommerce_user;Password=ecommerce_password;'
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

Write-Host "[migrate] dotnet tool restore..." -ForegroundColor Cyan
dotnet tool restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[migrate] dotnet ef database update..." -ForegroundColor Cyan
dotnet ef database update `
    --project EcommerceBackend.csproj `
    --startup-project EcommerceBackend.csproj `
    --connection $ConnectionString
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[migrate] Tamam." -ForegroundColor Green
