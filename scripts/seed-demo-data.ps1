# Veritabanina demo veri yukler: adres, odeme, siparis, sepet, favori, bildirim,
# giris gecmisi, kullanici/ gizlilik ayarlari, yardim makaleleri, SSS, destek talebi.
#
# Oncelikle baglanti appsettings / ortam degiskenleri ile ayarli olmalidir (PostgreSQL veya SQLite).
# Temel katalog + kullanicilar DataSeeder ile yoksa bu komut da onlari olusturur.
#
# Kullanim:
#   .\scripts\seed-demo-data.ps1
#   $env:ASPNETCORE_ENVIRONMENT='Development'; .\scripts\seed-demo-data.ps1
#
# Tekrar calistirma: ayni demo kayitlari zaten varsa atlanir (idempotent).

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\_repo-root.ps1"
Set-Location (Get-RepoRoot)

Write-Host "[seed-demo] dotnet run -- seed-demo ..." -ForegroundColor Cyan
dotnet run --project EcommerceBackend.csproj -- seed-demo
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "[seed-demo] Tamam." -ForegroundColor Green
