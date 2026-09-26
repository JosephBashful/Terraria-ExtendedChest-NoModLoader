#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "$0")"

if [[ ! -f TerrariaServer.exe || ! -x TerrariaServer.bin.x86_64 ]]; then
    echo "Patched Linux server was not found." >&2
    echo "Run scripts/Build-Server-Linux.sh from the repository first." >&2
    exit 1
fi

mkdir -p Saves
export MONO_IOMAP=all
exec ./TerrariaServer.bin.x86_64 -savedirectory Saves "$@"
