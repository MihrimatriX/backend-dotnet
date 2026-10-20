#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
CONN="${POSTGRES_CONNECTION_STRING:-Host=localhost;Port=5432;Database=ecommerce_db;Username=ecommerce_user;Password=ecommerce_password;}"

echo "[migrate] dotnet tool restore..."
dotnet tool restore
echo "[migrate] dotnet ef database update..."
dotnet ef database update \
  --project EcommerceBackend.csproj \
  --startup-project EcommerceBackend.csproj \
  --connection "$CONN"
echo "[migrate] Tamam."
