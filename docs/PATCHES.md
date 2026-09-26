# Client and server patches

The patcher modifies a verified local copy of a native Terraria 1.4.5.8 client or dedicated server for Windows or
Linux. It never overwrites the source executable. Windows runtimes are compiled against XNA; Linux runtimes are
compiled against the `FNA.dll` shipped with the user's native installation. Client and server runtimes are compiled
against `Terraria.exe` and `TerrariaServer.exe` respectively, preventing accidental cross-target patching.

The patched client restores its working directory to the directory containing its executable before the graphics
backend initializes the relative `Content` URI. This keeps local Steam assets available if Steam changes the launch
directory.

- separates the multiplayer protocol with the identifier `Terraria-ExtendedChest-v2-326`;
- expands the item registry from 6,196 to 6,198 entries;
- registers item/style 6196/52 as `Extended Chest Tier 1` and item/style 6197/53 as `Extended Chest Tier 2`;
- maps both placed tiers within Terraria's original `Containers` texture sheet, using the Crimson and Flesh variants,
  and aliases their item icons to the matching locally loaded artwork;
- supplies the interaction-range check missing from the non-vanilla Tier 2 container style;
- expands the client-side chest coin-state array from 200 to 1,000 entries so opening Tier 2 cannot overrun it;
- adds equivalent Iron and Lead Tier 1 recipes and separate Demonite and Crimtane Tier 2 recipes;
- reads `Tier1Slots` and `Tier2Slots` from `extended-chest.config`, then resizes newly placed chests up to the tier
  limits of 200 and 1,000 slots;
- widens packet 32's chest-slot index from 8 to 16 bits for slots above 255;
- intercepts the extended chest panel to add searching, mouse-wheel scrolling, a draggable vertical scrollbar, and a
  live used/available slot count;
- adds a reference to `ExtendedChest.Runtime.dll`.

Variable-capacity storage, slot persistence, and ingredient consumption use facilities already present in Terraria
1.4.5.8. Packet 32 is changed symmetrically on patched clients and servers; it is intentionally incompatible with
vanilla and earlier revisions of this mod.

To prevent partial patching of unknown versions, the patcher rejects every SHA-256 hash except:

```text
Windows client: 960A03BFF6050CF7BE16DFC1A7B19E10FC2C4F8F835A6A3B135A50DD9E6BA2F3
Linux client:   AE6ADF9CCD9131CFADF5FDC60CEA5F97DE4ED24084CE7F29582133AAA7A5DF3A
Windows server: 328872C65A4A7A94F050EAD1566F1567AF15C3193D4D7008A460507D3686D7C2
Linux server:   4B87890AC53D40F61DB5F928693A379ACF4CCBD8ED3B47EB32FB096F145DF034
```

The server target omits client-only content-root, chest UI, search UI, map, and tile-drawing hooks. It retains the item,
recipe, chest sizing, placement, interaction-range, protocol, and 16-bit chest-slot packet changes required by both
peers.

The patcher does not alter Steam verification, does not modify the source executable, and does not include code or
assets extracted from the game.
