#!/usr/bin/env bash
set -euo pipefail

expected_hash=ae6adf9ccd9131cfadf5fdc60cea5f97de4ed24084ce7f29582133aaa7a5df3a
configuration=${CONFIGURATION:-Release}

if [[ ${EUID:-$(id -u)} -eq 0 ]]; then
    echo "Do not build as root. Run this script from your normal desktop user." >&2
    exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
    echo ".NET SDK 8 or later was not found (missing 'dotnet' command)." >&2
    echo "Install the SDK for your normal user, open a new terminal, and verify with: dotnet --list-sdks" >&2
    exit 1
fi

if [[ $# -gt 1 ]]; then
    echo "Usage: bash scripts/Build-Client-Linux.sh [Terraria installation directory]" >&2
    exit 2
fi

repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd -P)

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
fi

if [[ -z "${game_path:-}" || ! -d "$game_path" ]]; then
    echo "Terraria for Linux was not found. Pass its installation directory explicitly." >&2
    exit 1
fi

game_path=$(cd -- "$game_path" && pwd -P)
required=(Terraria.exe FNA.dll Terraria Terraria.bin.x86_64 Content lib64)
for name in "${required[@]}"; do
    if [[ ! -e "$game_path/$name" ]]; then
        echo "Missing required Linux client path: $game_path/$name" >&2
        exit 1
    fi
done

actual_hash=$(sha256sum "$game_path/Terraria.exe" | awk '{ print $1 }')
if [[ "$actual_hash" != "$expected_hash" ]]; then
    echo "Unsupported Terraria Linux build." >&2
    echo "Expected 1.4.5.8: $expected_hash" >&2
    echo "Found:            $actual_hash" >&2
    exit 1
fi

reference_dir="$repo_root/.local/refs-linux"
runtime_output="$repo_root/.local/runtime-linux-$configuration"
dist_dir="$repo_root/dist/ExtendedChest-linux"
mkdir -p "$reference_dir" "$runtime_output" "$dist_dir"
for writable_dir in "$reference_dir" "$runtime_output" "$dist_dir"; do
    if [[ ! -w "$writable_dir" ]]; then
        owner=$(stat -c '%U:%G' "$writable_dir" 2>/dev/null || echo unknown)
        echo "Build directory is not writable: $writable_dir (owner: $owner)" >&2
        echo "Restore ownership to your desktop user, then retry the build without sudo." >&2
        exit 1
    fi
done

dotnet build "$repo_root/tools/ResourceExtractor/ResourceExtractor.csproj" --configuration "$configuration"
extractor="$repo_root/tools/ResourceExtractor/bin/$configuration/net8.0/ResourceExtractor.dll"
dotnet "$extractor" "$game_path/Terraria.exe" Terraria.Libraries.ReLogic.ReLogic.dll "$reference_dir/ReLogic.dll"

dotnet build "$repo_root/src/ExtendedChest.Runtime/ExtendedChest.Runtime.csproj" \
    --configuration "$configuration" \
    "-p:GamePath=$game_path" \
    "-p:ReferencePath=$reference_dir" \
    -p:GraphicsBackend=FNA \
    --output "$runtime_output"

dotnet build "$repo_root/src/ExtendedChest.Patcher/ExtendedChest.Patcher.csproj" --configuration "$configuration"
patcher="$repo_root/src/ExtendedChest.Patcher/bin/$configuration/net8.0/ExtendedChest.Patcher.dll"
runtime="$runtime_output/ExtendedChest.Runtime.dll"

while IFS= read -r -d '' source; do
    cp -f -- "$source" "$dist_dir/"
done < <(find "$game_path" -maxdepth 1 -type f -print0)
mkdir -p "$dist_dir/lib64"
cp -a -- "$game_path/lib64/." "$dist_dir/lib64/"

content_link="$dist_dir/Content"
if [[ -L "$content_link" ]]; then
    ln -sfn -- "$game_path/Content" "$content_link"
elif [[ -e "$content_link" ]]; then
    echo "Cannot replace existing non-symbolic path: $content_link" >&2
    exit 1
else
    ln -s -- "$game_path/Content" "$content_link"
fi

dotnet "$patcher" "$game_path/Terraria.exe" "$runtime" "$reference_dir" "$dist_dir/Terraria.exe"
cp -f -- "$repo_root/config/extended-chest.config" "$dist_dir/"
cp -f -- "$repo_root/scripts/Start-ExtendedChest.sh" "$dist_dir/"
cp -f -- "$repo_root/README.md" "$dist_dir/"
chmod +x "$dist_dir/Start-ExtendedChest.sh" "$dist_dir/Terraria" "$dist_dir/Terraria.bin.x86_64"

echo
echo "Linux build completed. Steam must be running."
echo "Launch: $dist_dir/Start-ExtendedChest.sh"
echo "Test saves will be stored in: $dist_dir/Saves"
