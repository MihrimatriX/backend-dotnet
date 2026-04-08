#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
CONFIG="${1:-Debug}"

echo "[restore-build] dotnet tool restore..."
dotnet tool restore
echo "[restore-build] dotnet restore..."
dotnet restore
echo "[restore-build] dotnet build -c $CONFIG --no-restore..."
dotnet build -c "$CONFIG" --no-restore
echo "[restore-build] Tamam."
