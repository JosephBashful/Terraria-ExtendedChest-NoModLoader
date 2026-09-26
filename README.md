# Extended Chest - Mod for Terraria 1.4.5.8 

While waiting for the official tmodloader support, I made this direct mod prototype.
It works without tModLoader, adds two extended chest tiers, item-name searching, and integration
with vanilla crafting from nearby chests. 
Tier 1 supports up to 200 configurable slots; Tier 2 provides 1,000 slots.

This repository contains only original code and patching tools. **It does not contain Terraria, modified executables,
game DLLs, assets, or decompiled game sources.** Client patches are built locally from the user's legitimately
installed Steam copy; server patches may also use the matching official Re-Logic dedicated-server package.

## Project status

- supported version: Terraria 1.4.5.8 native clients for Windows and Linux;
- tested clients: Windows 11 and Linux Mint with the native Steam/FNA client;
- tested dedicated server: Linux with the official Re-Logic 1.4.5.8 package;
- single player: tested and working;
- multiplayer: tested between native Linux client and server, but still experimental; every peer must use the same
  revision and capacities;
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
- Tier 1 recipes: 5 Wooden Chests and either 10 Iron Bars or 10 Lead Bars at an Anvil;
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

1. For clients: Terraria 1.4.5.8 purchased and installed through Steam.
2. For dedicated servers: the matching official Re-Logic server package or Steam installation.
3. Steam running when a client starts; the standalone dedicated server does not require Steam.
4. [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) or later.
5. NuGet access during the first build to obtain Mono.Cecil and the .NET Framework reference assemblies.
6. On Windows 10 or 11: Microsoft XNA Framework 4.0, normally installed with Terraria.
7. On Linux clients: the native Steam files, including `FNA.dll`, `Terraria`, and `Terraria.bin.x86_64`.

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

## Build and test a dedicated server

The server patch is experimental. Always begin with a disposable world and keep backups of every production world.
All connecting clients must be built from the same commit and must use the same `Tier1Slots` and `Tier2Slots` values
as the server.

On a native Linux server, run from the repository root:

```bash
bash scripts/Build-Server-Linux.sh "/opt/terraria/1458/Linux"
```

The script supports the official standalone Re-Logic dedicated-server package as well as a native Steam installation.
It verifies the original Linux `TerrariaServer.exe`, compiles a server-specific runtime against FNA, and creates an
isolated server under `dist/ExtendedChest-server-linux`. A `Content` directory is linked when present but is not
required by the headless dedicated-server package. The source installation is never overwritten.
Running a public server as `root` is discouraged; use a dedicated unprivileged service account for production.

Start a test instance on a non-production port:

```bash
./dist/ExtendedChest-server-linux/Start-ExtendedChest-Server.sh -port 7778
```

On Windows, prepare the dedicated server with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\Build-Server.ps1
```

Then launch `dist\ExtendedChest-server-windows\Start-ExtendedChest-Server.cmd`. Both launchers store test worlds under
their own `Saves` directory. Do not point an experimental build at a production world until placement, slots above 40,
search synchronization, save/reload, chest destruction, and reconnect behavior have all been verified.

## Steam and original files

The generated client continues to initialize Steam and requires a valid game copy. The project does not disable or
bypass Steam verification. The standalone dedicated server does not require Steam, but must come from the matching
official Re-Logic server distribution. Original executables remain untouched.

A Terraria update may make the patch incompatible. In that case, the SHA-256 check stops the process. Never add a new
hash without reviewing and testing every injection point.

## Saves and compatibility

The new item uses an ID unknown to vanilla Terraria. Do not open and resave worlds or characters containing an
Extended Chest with the vanilla client. Keep backups and initially use disposable test data.

The mod uses a multiplayer protocol identifier distinct from vanilla. A modified client is intentionally rejected by
a vanilla server, and a vanilla client is intentionally rejected by a modified server. Windows and Linux peers built
from the same revision use the same protocol, but their configured Tier 1 and Tier 2 capacities must also match.

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
