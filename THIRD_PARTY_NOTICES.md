# Third-party notices

This repository declares dependencies through NuGet and does not include their binaries.

## Mono.Cecil

- project: https://github.com/jbevain/cecil
- package: `Mono.Cecil` 0.11.6
- license: MIT
- use: controlled reading and rewriting of the user's local assembly.

## Microsoft .NET Framework reference assemblies

- package: `Microsoft.NETFramework.ReferenceAssemblies.net45` 1.0.3
- use: compiling the runtime for the framework used by the game.
- the package is marked `PrivateAssets="all"` and is not redistributed by this repository.

Terraria, XNA, and ReLogic are references supplied by the user's local installation. They are not vendored or
distributed by this project.
