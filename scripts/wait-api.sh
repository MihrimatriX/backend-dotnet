#!/usr/bin/env bash
set -euo pipefail
URL="${1:-http://localhost:5000/health}"
TIMEOUT="${2:-180}"
echo "[wait-api] $URL (max ${TIMEOUT}s)..."
for ((i=0; i<TIMEOUT; i+=3)); do
  if curl -sf -o /dev/null "$URL"; then
    echo "[wait-api] API hazir."
    exit 0
  fi
  sleep 3
done
echo "[wait-api] Zaman asimi." >&2
exit 1
