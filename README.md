# Extended Chest - Mod for Terraria 1.4.5.8 

While waiting for the official tmodloader support, I made this direct mod prototype.
It works without tModLoader, adds two extended chest tiers, item-name searching, and integration
with vanilla crafting from nearby chests. 
Tier 1 supports up to 200 configurable slots; Tier 2 provides 1,000 slots.

This repository contains only original code and patching tools. **It does not contain Terraria, modified executables,
game DLLs, assets, or decompiled game sources.** The patch is built and applied locally to the user's legitimately
installed Steam copy.

## Project status

- supported version: Terraria 1.4.5.8 native clients for Windows and Linux;
- tested clients: Windows 11 and Linux Mint with the native Steam/FNA client;
- single player: tested and working;
- multiplayer: still in development, but planned for the very near future;
- tModLoader: not required;
- graphics: dark-red vanilla-style appearances sourced at runtime from the user's Terraria installation;
- item and search interface language: English.

## Features

- distinct `Extended Chest Tier 1` and `Extended Chest Tier 2` items;
- Tier 1: 200 slots by default, configurable from 40 to 200;
- Tier 2: 1,000 slots by default, configurable from 200 to 1,000;
- case-insensitive item-name search;
- mouse-wheel grid scrolling;
- a draggable vertical scrollbar that follows the active search results;
- a live count of used and available slots;
- Tier 1 recipe: 5 Wooden Chests and 10 Iron Bars at an Anvil;
- Tier 2 recipes: 1 Tier 1 chest and either 25 Demonite Bars or 25 Crimtane Bars at an Anvil;
- vanilla crafting from materials stored on nearby chest beyond slot 40;
- native variable-capacity persistence and a modded 16-bit slot-index packet for Tier 2 synchronization.

Configure both tiers in `extended-chest.config`:

```ini
Tier1Slots=200
Tier2Slots=1000
```

Tier 2 cannot be smaller than Tier 1. Configured capacities apply to newly placed chests. Existing saved chests keep
their current capacity, so reducing a setting cannot silently delete stored items.

## Requirements

1. Terraria 1.4.5.8 purchased and installed through Steam.
2. Steam running when the client starts.
3. [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
4. NuGet access during the first build to obtain Mono.Cecil and the .NET Framework reference assemblies.
5. On Windows 10 or 11: Microsoft XNA Framework 4.0, normally installed with Terraria.
6. On Linux: the native Steam client files, including `FNA.dll`, `Terraria`, and `Terraria.bin.x86_64`.

The Linux build does not target the Windows client through Proton. In Steam's Terraria compatibility settings, do not
force a Proton version when preparing the native Linux build.

## Build and prepare the Windows client

Open PowerShell in the repository root and run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Client.ps1
```

If Terraria is installed in another Steam library:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Client.ps1 `
  -GamePath 'D:\SteamLibrary\steamapps\common\Terraria'
```

The script:

1. verifies the supported 1.4.5.8 hash;
2. extracts the ReLogic compile reference locally from the user's assembly;
3. builds the runtime and patcher;
4. generates the client under `dist\ExtendedChest`;
5. links the Steam installation's `Content` directory without adding it to the repository;
6. leaves the Steam-installed `Terraria.exe` untouched.

The `.local` and `dist` directories are ignored by Git and must never be published.

## Build and prepare the Linux client

On Linux Mint, open a terminal in the repository root and run:

```bash
bash scripts/Build-Client-Linux.sh
```

Run the build as the normal desktop user, not as `root`. Verify the SDK first with `dotnet --list-sdks`; if the
command is missing, install .NET SDK 8 or later using Microsoft's Linux instructions linked above.

If Terraria is installed in another Steam library, pass its directory explicitly:

```bash
bash scripts/Build-Client-Linux.sh "/mnt/games/SteamLibrary/steamapps/common/Terraria"
```

The Linux script verifies the native 1.4.5.8 assembly, compiles the runtime against the installed `FNA.dll`, creates
`dist/ExtendedChest-linux`, copies the required Mono/FNA runtime files only into that ignored local output, and links
`Content` to the Steam installation. The original Steam files remain untouched, and none of the copied files are part
of the repository.

To inspect an installation without building or changing it:

```bash
bash scripts/Inspect-Linux-Client.sh "/path/to/steamapps/common/Terraria"
```

## Launch and first test

Keep Steam running, then launch:

```text
dist\ExtendedChest\Start-ExtendedChest.cmd
```

On Linux:

```bash
./dist/ExtendedChest-linux/Start-ExtendedChest.sh
```

The launcher uses a separate save directory:

```text
dist\ExtendedChest\Saves
```

For the first test, create a new character and world. Craft both chest tiers at an Anvil, place items in their final
slots, test searching, then close and reopen the world.

Search controls:

- left click: focus the search field;
- Enter or Escape: stop typing;
- right click: clear the filter;
- mouse wheel over the grid: change page;
- drag or click the vertical scrollbar: move through chest rows.

## Steam and original files

The generated client continues to initialize Steam and requires a valid game copy. The project does not disable or
bypass Steam verification. The original executable remains in the Steam directory and can still be launched normally
from the Steam Library.

A Terraria update may make the patch incompatible. In that case, the SHA-256 check stops the process. Never add a new
hash without reviewing and testing every injection point.

## Saves and compatibility

The new item uses an ID unknown to vanilla Terraria. Do not open and resave worlds or characters containing an
Extended Chest with the vanilla client. Keep backups and initially use disposable test data.

The mod uses a multiplayer protocol identifier distinct from vanilla. Clients and servers must use the same revision.
Windows and Linux clients built from this repository use the same protocol revision.

## Publishing and license

The original project code is released under the MIT License. Terraria and all related content remain the property of
their respective owners. Read [LEGAL.md](LEGAL.md) before creating a release or fork.

Do not upload `dist`, `.local`, Windows or Linux Terraria executables, FNA/Mono/game DLLs, native libraries, assets,
decompiled sources, or archives generated from the local installation. The repository and its releases must remain
source-only. See [CONTRIBUTING.md](CONTRIBUTING.md) for contribution rules.

The applied IL changes are documented in [docs/PATCHES.md](docs/PATCHES.md).

## Disclaimer

This is an unofficial project. It is not affiliated with, endorsed by, or sponsored by Re-Logic. Use backups and run
the software at your own risk.
