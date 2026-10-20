#!/usr/bin/env bash
# Calisan dotnet-backend konteynerinde demo veri
set -euo pipefail
echo "[docker-seed-demo] dotnet-backend icinde seed-demo..."
docker exec dotnet-backend dotnet EcommerceBackend.dll seed-demo
echo "[docker-seed-demo] Tamam."
