#!/usr/bin/env bash
set -euo pipefail
TIMEOUT="${1:-120}"
CONTAINER="${POSTGRES_CONTAINER:-dotnet-postgres}"

echo "[wait-postgres] (max ${TIMEOUT}s) container=$CONTAINER..."
end=$((SECONDS + TIMEOUT))
while [ "$SECONDS" -lt "$end" ]; do
  if docker exec "$CONTAINER" pg_isready -U ecommerce_user -d ecommerce_db >/dev/null 2>&1; then
    echo "[wait-postgres] PostgreSQL hazir."
    exit 0
  fi
  sleep 2
done
echo "[wait-postgres] Zaman asimi (docker calisiyor ve $CONTAINER ayakta mi?)." >&2
exit 1
