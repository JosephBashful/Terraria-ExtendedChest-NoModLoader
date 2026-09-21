# Contributing

Changes must contain only original project code. Do not add Terraria files, assets, DLLs, executables, or decompiled sources.

Before proposing a change:

1. run `scripts/Build-Client.ps1` with a supported Steam copy;
2. test the result with a disposable world and character;
3. verify that `git status` does not include `dist`, `.local`, or game files;
4. state clearly which Terraria version was tested.

When updating for a new Terraria version, add its hash only after every injection point has been reviewed and tested.
Never broaden or remove the hash check for convenience.
