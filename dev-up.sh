#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Copy .env if not present
if [ ! -f .env ]; then
  cp .env.example .env
  echo "[info] Created .env from .env.example"
fi

# Start infrastructure
echo "[info] Starting PostgreSQL and pgAdmin..."
docker compose up -d

# Wait for PostgreSQL to be healthy
echo "[info] Waiting for PostgreSQL..."
until docker compose exec -T postgres pg_isready -U seems_user -d seems_platform_dev > /dev/null 2>&1; do
  sleep 1
done
echo "[info] PostgreSQL is ready."

# Run backend
echo "[info] Starting ASP.NET Core API (with auto-migrate & seed)..."
cd src/backend
dotnet run --project Seems.Api --launch-profile https 2>&1 &
BACKEND_PID=$!

echo ""
echo "=========================================="
echo "  SEEMS Platform — Dev Environment"
echo "=========================================="
echo "  API:      https://localhost:5001"
echo "  Swagger:  https://localhost:5001/swagger"
echo "  pgAdmin:  http://localhost:8080"
echo "    Email:    admin@seems.local"
echo "    Password: admin"
echo ""
echo "  Admin login:"
echo "    Email:    admin@seems.local"
echo "    Password: (see ADMIN_DEFAULT_PASSWORD in .env)"
echo "=========================================="
echo ""
echo "Press Ctrl+C to stop..."

# Trap exit to clean up
trap "kill $BACKEND_PID 2>/dev/null; echo '[info] Backend stopped.'" EXIT
wait $BACKEND_PID
