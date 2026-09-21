# Legal and distribution notice

This is an unofficial project. It is not affiliated with, endorsed by, or sponsored by Re-Logic.
Terraria and its trademarks, executables, images, sounds, and other content belong to their respective owners.

The MIT License included in this repository applies only to the project's original code.
It grants no rights to Terraria files or content.

## What this repository contains

- patcher source code;
- Extended Chest runtime source code;
- tools that operate on the user's local copy;
- build scripts and documentation.

## What must not be published

- original or modified copies of `Terraria.exe` or `TerrariaServer.exe`;
- DLLs, runtimes, or archives copied from an official game or server installation;
- the `Content` directory or individual game assets;
- decompiled Terraria source code;
- the `dist` directory, because it is generated from local game files;
- the `.local` directory, because it may contain locally extracted references.

The patcher preserves the game's Steam initialization and verification. It does not provide or download Terraria,
remove DRM, or enable the game to run without a legitimate Steam installation.

Before publishing a fork or release, review the current Re-Logic rules:
https://forums.terraria.org/index.php?threads/modding-pc-only-rules-guidelines.286/

## GitHub Releases

Do not attach the `dist` directory to a release. You may publish the source code and, if desired, compiled tools
that contain only original project code. The most conservative distribution method is to publish source code only
and require users to compile and apply the patch locally.

