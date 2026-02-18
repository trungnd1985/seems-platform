#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "[info] Stopping containers..."
docker compose down

echo "[info] Dev environment stopped."
echo ""
echo "To also remove data volumes: docker compose down -v"
