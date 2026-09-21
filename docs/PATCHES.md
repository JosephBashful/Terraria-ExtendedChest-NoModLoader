# Client patches

The patcher modifies a verified local copy of Terraria 1.4.5.8 for Windows. It never overwrites the Steam executable.

- separates the multiplayer protocol with the identifier `Terraria-ExtendedChest-v2-326`;
- expands the item registry from 6,196 to 6,198 entries;
- registers item/style 6196/52 as `Extended Chest Tier 1` and item/style 6197/53 as `Extended Chest Tier 2`;
- aliases item and placed-chest rendering to dark-red Crimson and Crimtane artwork loaded from the user's game;
- adds the Tier 1 recipe and separate Demonite and Crimtane Tier 2 recipes;
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
960A03BFF6050CF7BE16DFC1A7B19E10FC2C4F8F835A6A3B135A50DD9E6BA2F3
```

The patcher does not alter Steam verification, does not modify the Steam-installed executable, and does not include
code or assets extracted from the game.
