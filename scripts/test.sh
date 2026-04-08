#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
CONFIG="${1:-Debug}"
INTEGRATION_ONLY="${2:-}"

if [[ "$INTEGRATION_ONLY" == "integration" ]]; then
  echo "[test] Filtre: Category=Integration (Docker gerekli)"
  dotnet test EcommerceBackend.sln -c "$CONFIG" --filter "Category=Integration"
else
  dotnet test EcommerceBackend.sln -c "$CONFIG"
fi
echo "[test] Tamam."
