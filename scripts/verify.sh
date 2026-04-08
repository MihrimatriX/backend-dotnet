#!/usr/bin/env bash
# Proje kökünden: bash scripts/verify.sh
# Entegrasyon testi: bash scripts/verify.sh --tests  (Docker Desktop / docker.sock gerekli)

set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "==> Docker: API"
docker build -t ecommerce-backend:local .

echo "==> Docker: temp-shop"
docker build -t ecommerce-shop:local ./temp-shop

if [[ "${1:-}" == "--tests" ]]; then
  echo "==> Entegrasyon testleri (sdk:10.0 konteyneri, Testcontainers)"
  docker run --rm \
    -v "$ROOT:/src" \
    -v /var/run/docker.sock:/var/run/docker.sock \
    -w /src \
    -e TESTCONTAINERS_RYUK_DISABLED=true \
    mcr.microsoft.com/dotnet/sdk:10.0 \
    dotnet test EcommerceBackend.sln -c Release -v minimal
fi

echo ""
echo "Tamam. Stack: docker compose up --build -d"
