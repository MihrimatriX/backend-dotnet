#!/usr/bin/env bash
set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
WITH_STACK=0
KEEP_STACK=0
INTEGRATION_ONLY=0
SKIP_TESTS=0
SKIP_MIGRATE=0

for arg in "$@"; do
  case "$arg" in
    --with-docker-stack) WITH_STACK=1 ;;
    --keep-stack) KEEP_STACK=1 ;;
    --integration-only) INTEGRATION_ONLY=1 ;;
    --skip-tests) SKIP_TESTS=1 ;;
    --skip-migrate) SKIP_MIGRATE=1 ;;
  esac
done

echo "========== pipeline basliyor =========="
"$SCRIPT_DIR/restore-build.sh"

if [[ "$WITH_STACK" -eq 1 ]]; then
  "$SCRIPT_DIR/docker-up.sh" -d
  "$SCRIPT_DIR/wait-postgres.sh"
  "$SCRIPT_DIR/wait-api.sh"
  if [[ "$SKIP_MIGRATE" -eq 0 ]]; then
    "$SCRIPT_DIR/migrate.sh"
  fi
fi

if [[ "$SKIP_TESTS" -eq 0 ]]; then
  if [[ "$INTEGRATION_ONLY" -eq 1 ]]; then
    "$SCRIPT_DIR/test.sh" Debug integration
  else
    "$SCRIPT_DIR/test.sh"
  fi
fi

if [[ "$WITH_STACK" -eq 1 && "$KEEP_STACK" -eq 0 ]]; then
  "$SCRIPT_DIR/docker-down.sh"
fi

echo "========== pipeline tamam =========="
