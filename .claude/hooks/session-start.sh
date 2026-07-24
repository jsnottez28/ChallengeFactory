#!/bin/bash
set -euo pipefail

# Only needed for Claude Code on the web (remote sessions); local dev machines
# are expected to already have the .NET SDK installed.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

REPO_ROOT="${CLAUDE_PROJECT_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
DOTNET_INSTALL_DIR="$HOME/.dotnet"

if ! command -v dotnet >/dev/null 2>&1 || [ ! -x "$DOTNET_INSTALL_DIR/dotnet" ]; then
  bash "$REPO_ROOT/dotnet-install.sh" --channel 8.0 --install-dir "$DOTNET_INSTALL_DIR"
fi

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_INSTALL_DIR\""
    echo "export PATH=\"$DOTNET_INSTALL_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
  } >> "$CLAUDE_ENV_FILE"
fi

export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

dotnet restore "$REPO_ROOT/ChallengeFactory.sln"
