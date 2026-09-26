# Legal and distribution notice

This is an unofficial project. It is not affiliated with, endorsed by, or sponsored by Re-Logic.
Terraria and its trademarks, executables, images, sounds, and other content belong to their respective owners.

The MIT License included in this repository applies only to the project's original code.
It grants no rights to Terraria files or content.

## What this repository contains

- patcher source code;
- Extended Chest runtime source code;
- tools that operate on the user's local copy;
- client and dedicated-server build scripts and documentation.

## What must not be published

- original or modified copies of `Terraria.exe` or `TerrariaServer.exe`;
- the native Linux launchers or executables (`Terraria`, `TerrariaServer`, `Terraria.bin.x86_64`, or
  `TerrariaServer.bin.x86_64`);
- DLLs, runtimes, or archives copied from an official game or server installation;
- `FNA.dll`, Mono framework assemblies, the native `lib64` directory, or `steam_appid.txt`;
- the `Content` directory or individual game assets;
- decompiled Terraria source code;
- the `dist` directory, because it is generated from local game files;
- the `.local` directory, because it may contain locally extracted references.

The Windows and Linux build scripts read from the user's own supported Steam installation or matching official
Re-Logic dedicated-server package and write generated clients or servers only to the Git-ignored `dist` directory.
The patcher accepts only explicitly reviewed executable hashes, preserves client Steam initialization and
verification, and does not provide or download Terraria, remove DRM, or bypass any client ownership requirement.

Before publishing a fork or release, review the current Re-Logic rules:
https://forums.terraria.org/index.php?threads/modding-pc-only-rules-guidelines.286/

## GitHub Releases

Do not attach the `dist` directory to a release. This project's distribution policy is source-only: users compile the
original patcher/runtime code and apply it locally to their own supported installation. Always check the current game
owner and platform rules before publishing a fork or release; this notice is a project policy, not legal advice.
