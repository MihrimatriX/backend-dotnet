#!/usr/bin/env bash
# API duman testi — PowerShell script'ini çağırır (Windows/macOS/Linux + PowerShell 7).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
if command -v pwsh >/dev/null 2>&1; then
  exec pwsh -File "$ROOT/scripts/api-smoke-test.ps1" "$@"
fi
if command -v powershell.exe >/dev/null 2>&1; then
  exec powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$ROOT/scripts/api-smoke-test.ps1" "$@"
fi
echo "pwsh veya powershell bulunamadı. https://github.com/PowerShell/PowerShell/releases" >&2
exit 1
