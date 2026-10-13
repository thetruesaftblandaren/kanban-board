#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"
API_DIR="$ROOT_DIR/backend/src/Kanban.Api"

require() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Error: '$1' is required but was not found. $2"
    exit 1
  fi
}

require docker "Install Docker Desktop."
require dotnet "Install the .NET 10 SDK."
require node "Install Node.js (LTS)."
require openssl "It is included with Git Bash."

# 1. .env (keep an existing one so the database password never changes under a running volume)
if [[ ! -f .env ]]; then
  echo "Creating .env with a generated database password..."
  printf 'POSTGRES_PASSWORD=%s\n' "$(openssl rand -hex 16)" > .env
else
  echo ".env already exists, keeping it."
fi
DB_PASSWORD="$(grep '^POSTGRES_PASSWORD=' .env | cut -d= -f2- | tr -d '\r')"
if [[ -z "$DB_PASSWORD" ]]; then
  echo "Error: POSTGRES_PASSWORD is missing from .env"
  exit 1
fi

# 2. Backend secrets
echo "Configuring backend secrets..."
cd "$API_DIR"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=kanban;Username=kanban;Password=${DB_PASSWORD}" >/dev/null

existing_secrets="$(dotnet user-secrets list || true)"
if ! grep -q '^Jwt:Key' <<<"$existing_secrets"; then
  dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48 | tr -d '\n')" >/dev/null
fi

# 3. Frontend dependencies
echo "Installing frontend dependencies..."
cd "$ROOT_DIR/frontend"
npm ci

# 4. Database
echo "Starting the database..."
cd "$ROOT_DIR"
docker compose up -d --wait

echo "Applying migrations..."
dotnet tool restore
cd "$API_DIR"
dotnet ef database update

echo ""
echo "Setup complete. Start everything with: ./scripts/dev.sh"
echo ""
if [[ -t 0 ]]; then
  read -r -p "Press Enter to close..."
fi
