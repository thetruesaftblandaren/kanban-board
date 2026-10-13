#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

BACKEND_PID=""
FRONTEND_PID=""

start_docker() {
  echo "Checking Docker..."

  if docker info >/dev/null 2>&1; then
    echo "Docker is already running."
    return 0
  fi

  echo "Docker is not running. Attempting to start Docker Desktop..."

  case "$(uname -s)" in
    Darwin)
      open -a Docker
      ;;
    Linux)
      echo "Please start Docker manually on Linux."
      return 1
      ;;
    MINGW*|MSYS*|CYGWIN*)
      if command -v powershell.exe >/dev/null 2>&1; then
        powershell.exe -NoProfile -Command \
          'Start-Process "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"'
      else
        echo "Could not find PowerShell to start Docker Desktop."
        return 1
      fi
      ;;
    *)
      echo "Unsupported operating system. Start Docker manually."
      return 1
      ;;
  esac

  echo "Waiting for Docker to become ready..."

  for _ in {1..60}; do
    if docker info >/dev/null 2>&1; then
      echo "Docker is ready!"
      return 0
    fi
    sleep 2
  done

  echo "Error: Docker did not start within 120 seconds."
  echo "Please start Docker Desktop manually and try again."
  return 1
}

# Ports used in local development. If you change them, update these too:
#   backend:  launchSettings.json, frontend/src/api/client.ts
#   frontend: vite config (default 5173), CORS origin in Program.cs
FRONTEND_URL="http://localhost:5173"
BACKEND_URL="http://localhost:5093/api/boards"
WATCHER_PID=""

wait_for_http() {
  local url="$1" name="$2" code

  for _ in {1..120}; do
    code="$(curl -s -o /dev/null -w '%{http_code}' "$url" || true)"
    if [[ "$code" != "000" ]]; then
      echo ">> $name is ready."
      return 0
    fi
    sleep 1
  done

  echo ">> $name did not respond within 120 seconds."
  return 1
}

open_browser() {
  local url="$1"

  case "$(uname -s)" in
    Darwin)
      open "$url"
      ;;
    Linux)
      if command -v xdg-open >/dev/null 2>&1; then
        xdg-open "$url" >/dev/null 2>&1 &
      else
        echo "Could not find xdg-open. Open $url manually."
      fi
      ;;
    MINGW*|MSYS*|CYGWIN*)
      powershell.exe -NoProfile -Command "Start-Process '$url'"
      ;;
    *)
      echo "Unsupported OS. Open $url manually."
      ;;
  esac
}

announce_when_ready() {
  wait_for_http "$BACKEND_URL" "Backend" || return 0
  wait_for_http "$FRONTEND_URL" "Frontend" || return 0

  echo ">> The app is ready at $FRONTEND_URL"

  if [[ "$OPEN_BROWSER" == true ]]; then
    open_browser "$FRONTEND_URL"
  fi
}

cleanup() {
  trap - EXIT INT TERM HUP

  if [[ -n "$WATCHER_PID" ]]; then
    kill "$WATCHER_PID" 2>/dev/null || true
  fi

  echo ""
  echo "Stopping services..."

  for pid in "$FRONTEND_PID" "$BACKEND_PID"; do
    if [[ -n "$pid" ]] && kill -0 "$pid" 2>/dev/null; then
      kill -TERM -- "-$pid" 2>/dev/null || true
    fi
  done

  [[ -n "$FRONTEND_PID" ]] && wait "$FRONTEND_PID" 2>/dev/null || true
  [[ -n "$BACKEND_PID" ]] && wait "$BACKEND_PID" 2>/dev/null || true

  docker compose stop || true

  echo "All services stopped."
}

trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
trap 'exit 129' HUP

OPEN_BROWSER=false
if [[ -t 0 ]]; then
  read -r -p "Open localhost in browser when ready? (y/n): " answer
  case "$answer" in
    [Yy]|[Yy][Ee][Ss]) OPEN_BROWSER=true ;;
  esac
fi

start_docker
docker compose up -d

echo "Starting .NET backend..."
(
  cd "$ROOT_DIR/backend/src/Kanban.Api"
  exec dotnet run
) &
BACKEND_PID=$!

echo "Starting React frontend (Vite)..."
(
  cd "$ROOT_DIR/frontend"
  exec npm run dev
) &
FRONTEND_PID=$!

announce_when_ready &
WATCHER_PID=$!

wait
