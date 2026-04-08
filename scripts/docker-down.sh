#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
echo "[docker-down] docker compose down..."
if [[ "${1:-}" == "-v" ]]; then
  docker compose down -v
else
  docker compose down
fi
echo "[docker-down] Tamam."
