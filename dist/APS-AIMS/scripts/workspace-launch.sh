#!/bin/bash
set -e

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
APP_DIR="$ROOT/app"
APP="$APP_DIR/APS.AIMS.Api"
HEALTH="http://127.0.0.1:5175/api/health"
URL="http://127.0.0.1:5175"

if [ ! -f "$APP" ]; then
  echo "APS AIMS macOS runtime was not found: $APP"
  echo "Build an osx-arm64 or osx-x64 Workspace package first."
  exit 1
fi

if curl -fsS "$HEALTH" >/dev/null 2>&1; then
  open "$URL"
  exit 0
fi

export ASPNETCORE_ENVIRONMENT="Workspace"
export ASPNETCORE_URLS="http://127.0.0.1:5175"
export Workspace__DisableHttpsRedirection="true"

cd "$APP_DIR"
chmod +x "$APP"
"$APP" >/tmp/aps-aims.log 2>&1 &
PID=$!

for i in $(seq 1 90); do
  if curl -fsS "$HEALTH" >/dev/null 2>&1; then
    open "$URL"
    exit 0
  fi

  if ! kill -0 "$PID" >/dev/null 2>&1; then
    echo "APS AIMS stopped before the local server became ready."
    echo "See /tmp/aps-aims.log"
    exit 1
  fi

  sleep 0.5
done

kill "$PID" >/dev/null 2>&1 || true
echo "APS AIMS did not become ready within 45 seconds."
exit 1
