#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"
chmod +x "$SCRIPT_DIR/scripts/workspace-launch.sh"
"$SCRIPT_DIR/scripts/workspace-launch.sh"
