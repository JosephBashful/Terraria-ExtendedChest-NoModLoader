# Contributing

Changes must contain only original project source code, scripts, and documentation. Do not add Terraria files, assets,
DLLs, Windows or Linux executables, native libraries, generated clients, archives, or decompiled sources.

Before proposing a change:

1. run the matching client or server build script with a supported local Terraria installation;
2. test the result with a disposable world and character;
3. verify that `git status` does not include `dist`, `.local`, compiled output, or game files;
4. state clearly which Terraria version was tested.

For multiplayer changes, test a client and server built from the same commit with identical capacity settings. Verify
connect, placement, access beyond slot 40, save/reload, reconnect, and chest destruction on a disposable world.

When updating for a new Terraria version, add its hash only after every injection point has been reviewed and tested.
Never broaden or remove the hash check for convenience.
