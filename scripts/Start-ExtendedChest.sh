#!/usr/bin/env bash
set -euo pipefail

cd -- "$(dirname -- "$0")"

if [[ ! -f Terraria.exe || ! -x Terraria.bin.x86_64 ]]; then
    echo "Patched Linux client was not found." >&2
    echo "Run scripts/Build-Client-Linux.sh from the repository first." >&2
    exit 1
fi

mkdir -p Saves
export MONO_IOMAP=all
exec ./Terraria.bin.x86_64 -savedirectory "$PWD/Saves" "$@"
