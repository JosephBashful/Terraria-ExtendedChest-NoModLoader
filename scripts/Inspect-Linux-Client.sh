#!/usr/bin/env bash
set -euo pipefail

if [[ $# -gt 1 ]]; then
    echo "Usage: bash scripts/Inspect-Linux-Client.sh [Terraria installation directory]" >&2
    exit 2
fi

if [[ $# -eq 1 ]]; then
    game_path=$1
else
    candidates=(
        "$HOME/.local/share/Steam/steamapps/common/Terraria"
        "$HOME/.steam/steam/steamapps/common/Terraria"
        "$HOME/.steam/root/steamapps/common/Terraria"
        "$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/Terraria"
        "$HOME/.var/app/com.valvesoftware.Steam/data/Steam/steamapps/common/Terraria"
    )
    game_path=""
    for candidate in "${candidates[@]}"; do
        if [[ -f "$candidate/Terraria.exe" ]]; then
            game_path=$candidate
            break
        fi
    done

    if [[ -z "$game_path" ]]; then
        search_roots=(
            "$HOME/.local/share/Steam"
            "$HOME/.steam"
            "$HOME/.var/app/com.valvesoftware.Steam"
            "/mnt"
            "/media/$USER"
        )
        existing_roots=()
        for root in "${search_roots[@]}"; do
            [[ -d "$root" ]] && existing_roots+=("$root")
        done
        if [[ ${#existing_roots[@]} -gt 0 ]]; then
            detected_exe=$(find "${existing_roots[@]}" -type f \
                -path '*/steamapps/common/Terraria/Terraria.exe' -print -quit 2>/dev/null || true)
            [[ -n "$detected_exe" ]] && game_path=${detected_exe%/Terraria.exe}
        fi
    fi
fi

if [[ -z "${game_path:-}" || ! -d "$game_path" ]]; then
    echo "Terraria for Linux was not found. Pass its installation directory explicitly." >&2
    exit 1
fi

required=(Terraria.exe FNA.dll Terraria Terraria.bin.x86_64)
missing=()
for name in "${required[@]}"; do
    if [[ ! -f "$game_path/$name" ]]; then
        missing+=("$name")
    fi
done

if [[ ${#missing[@]} -gt 0 ]]; then
    echo "A Terraria installation was found at: $game_path" >&2
    echo "Missing native Linux client files: ${missing[*]}" >&2
    echo "This usually means Steam installed the Windows depot through Proton." >&2
    echo "In Steam, open Terraria > Properties > Compatibility, disable the forced compatibility tool," >&2
    echo "then reinstall or verify the game before running this inspection again." >&2
    exit 1
fi

echo "Terraria Linux client inspection"
echo "Path: $game_path"
echo
echo "SHA-256"
sha256sum \
    "$game_path/Terraria.exe" \
    "$game_path/FNA.dll" \
    "$game_path/Terraria" \
    "$game_path/Terraria.bin.x86_64"
echo
echo "File types"
file \
    "$game_path/Terraria.exe" \
    "$game_path/FNA.dll" \
    "$game_path/Terraria" \
    "$game_path/Terraria.bin.x86_64"
echo
echo "Top-level files"
find "$game_path" -maxdepth 1 -type f -printf '%f\n' | LC_ALL=C sort
echo
echo "Top-level directories"
find "$game_path" -mindepth 1 -maxdepth 1 -type d -printf '%f\n' | LC_ALL=C sort
