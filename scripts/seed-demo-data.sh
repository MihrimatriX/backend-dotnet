#!/usr/bin/env bash
# Veritabanina demo veri yukler (PostgreSQL veya SQLite; appsettings / env ile).
# Tekrar calistirmada isaretli kayit varsa atlanir.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
echo "[seed-demo] dotnet run -- seed-demo ..."
dotnet run --project EcommerceBackend.csproj -- seed-demo
echo "[seed-demo] Tamam."
