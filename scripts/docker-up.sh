#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
DETACH=""
WAIT_API=""
EXTRA=()
while [[ $# -gt 0 ]]; do
  case "$1" in
    -d|--detach) DETACH=1; shift ;;
    --wait-api) WAIT_API=1; shift ;;
    --) shift; EXTRA+=("$@"); break ;;
    *) EXTRA+=("$1"); shift ;;
  esac
done

echo "[docker-up] Stack: postgres, redis, rabbitmq, dotnet-backend, shop (:3000)..."
if [[ -n "$DETACH" ]]; then
  docker compose up --build -d "${EXTRA[@]}"
else
  docker compose up --build "${EXTRA[@]}"
fi

if [[ -n "$WAIT_API" ]]; then
  "$(dirname "$0")/wait-api.sh"
fi
