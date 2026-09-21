using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
#if !SERVER
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
#endif

namespace ExtendedChest
{
    public static class Hooks
    {
        public const int Tier1ItemType = 6196;
        public const int Tier2ItemType = 6197;
        public const int ItemType = Tier1ItemType;
        public const int Tier1Style = 52;
        public const int Tier2Style = 53;
        public const int Style = Tier1Style;
        public const string Tier1DisplayName = "Extended Chest Tier 1";
        public const string Tier2DisplayName = "Extended Chest Tier 2";
        public const string DisplayName = Tier1DisplayName;
        private static int tier1Capacity;
        private static int tier2Capacity;
        private static bool capacitiesLoaded;
        private static LocalizedText tier1Text;
        private static LocalizedText tier2Text;

        private static LocalizedText CreateText(string key, string value) =>
            (LocalizedText)Activator.CreateInstance(typeof(LocalizedText), BindingFlags.Instance | BindingFlags.NonPublic,
                null, new object[] { key, value }, null);
        public static LocalizedText Tier1Name() => tier1Text ?? (tier1Text = CreateText("ItemName.ExtendedChestTier1", Tier1DisplayName));
        public static LocalizedText Tier2Name() => tier2Text ?? (tier2Text = CreateText("ItemName.ExtendedChestTier2", Tier2DisplayName));
        public static LocalizedText Name() => Tier1Name();

        public static void SetupLanguage()
        {
            Lang.chestType[Tier1Style] = Tier1Name();
            Lang.chestType[Tier2Style] = Tier2Name();
        }

        public static void TileFrame(ushort type, ref short frame)
        {
            if (type != 21) return;
            int style = frame / 36;
            if (style == Tier1Style) frame = (short)(20 * 36 + frame % 36);
            if (style == Tier2Style) frame = (short)(23 * 36 + frame % 36);
        }

#if !SERVER
        public static Texture2D TileTexture(Texture2D original, Tile tile) =>
            tile != null && tile.type == 21 && tile.frameX / 36 == Tier2Style ? TextureAssets.Tile[467].Value : original;
#endif

        public static void MapOption(int type, Tile tile, ref int option)
        {
            if (type == 21 && IsExtendedStyle(tile.frameX / 36)) option = 20;
        }

        private static void LoadCapacities()
        {
            if (capacitiesLoaded) return;
            int tier1 = 200;
            int tier2 = 1000;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extended-chest.config");
            if (File.Exists(path))
            {
                foreach (string rawLine in File.ReadAllLines(path))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#") || line.StartsWith(";")) continue;
                    int separator = line.IndexOf('=');
                    if (separator <= 0 || separator == line.Length - 1)
                        throw new InvalidDataException("Invalid extended-chest.config line: " + rawLine);
                    string key = line.Substring(0, separator).Trim();
                    int value;
                    if (!int.TryParse(line.Substring(separator + 1).Trim(), out value))
                        throw new InvalidDataException("Invalid integer for " + key + " in extended-chest.config.");
                    if (key.Equals("Tier1Slots", StringComparison.OrdinalIgnoreCase)) tier1 = value;
                    else if (key.Equals("Tier2Slots", StringComparison.OrdinalIgnoreCase)) tier2 = value;
                    else throw new InvalidDataException("Unknown extended-chest.config key: " + key);
                }
            }
            if (tier1 < 40 || tier1 > 200)
                throw new InvalidDataException("Tier1Slots must be from 40 to 200.");
            if (tier2 < 200 || tier2 > 1000)
                throw new InvalidDataException("Tier2Slots must be from 200 to 1000.");
            if (tier2 < tier1)
                throw new InvalidDataException("Tier2Slots must be greater than or equal to Tier1Slots.");
            tier1Capacity = tier1;
            tier2Capacity = tier2;
            capacitiesLoaded = true;
        }

        public static int Tier1Capacity { get { LoadCapacities(); return tier1Capacity; } }
        public static int Tier2Capacity { get { LoadCapacities(); return tier2Capacity; } }
        public static int Capacity => Tier1Capacity;

        public static bool SetDefaults(Item item, int type)
        {
            if (type != Tier1ItemType && type != Tier2ItemType) return false;
            bool tier2 = type == Tier2ItemType;
            item.SetDefaults(tier2 ? 5784 : 1530);
            item.type = type;
            item.createTile = 21;
            item.placeStyle = tier2 ? Tier2Style : Tier1Style;
            item.material = ItemID.Sets.IsAMaterial[type];
            item.SetNameOverride(tier2 ? Tier2DisplayName : Tier1DisplayName);
            item.rare = tier2 ? 3 : 1;
            item.value = tier2 ? 50000 : 10000;
            return true;
        }

        public static void SetupSets()
        {
            ItemID.Sets.TextureCopyLoad[Tier1ItemType] = 1530;
            ItemID.Sets.TextureCopyLoad[Tier2ItemType] = 5784;
        }

        public static void SetupItemId()
        {
            ItemID.Search.Add("ExtendedChestTier1", Tier1ItemType);
            ItemID.Search.Add("ExtendedChestTier2", Tier2ItemType);
        }

        private static void AddRecipe(int result, int firstItem, int firstStack, int barType, int barStack)
        {
            if (Recipe.numRecipes >= Main.recipe.Length) throw new InvalidOperationException("No free recipe slot.");
            var recipe = new Recipe();
            recipe.createItem.SetDefaults(result);
            recipe.createItem.stack = 1;
            recipe.requiredItem[0].SetDefaults(firstItem);
            recipe.requiredItem[0].stack = firstStack;
            recipe.requiredItem[1].SetDefaults(barType);
            recipe.requiredItem[1].stack = barStack;
            recipe.requiredTile = 16;
            recipe.notDecraftable = true;
            Main.recipe[Recipe.numRecipes++] = recipe;
        }

        public static void SetupRecipe()
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
                if (Main.recipe[i].createItem.type == Tier1ItemType) return;
            AddRecipe(Tier1ItemType, 48, 5, 22, 10);
            AddRecipe(Tier2ItemType, Tier1ItemType, 1, 57, 25);
            AddRecipe(Tier2ItemType, Tier1ItemType, 1, 1257, 25);
            Recipe.TileUsedInRecipes[16] = true;
            int configuredTier1Capacity = Tier1Capacity;
            int configuredTier2Capacity = Tier2Capacity;
        }

        public static bool IsExtendedStyle(int style) => style == Tier1Style || style == Tier2Style;
        public static int Tier(Chest chest)
        {
            if (chest == null || chest.bankChest) return 0;
            Tile tile = Main.tile[chest.x, chest.y];
            if (tile == null || !tile.active() || tile.type != 21) return 0;
            int style = tile.frameX / 36;
            return style == Tier1Style ? 1 : style == Tier2Style ? 2 : 0;
        }
        public static bool IsExtended(Chest chest) => Tier(chest) != 0;
        public static int CapacityFor(Chest chest) => Tier(chest) == 2 ? Tier2Capacity : Tier1Capacity;
        public static Chest Created(Chest chest)
        {
            if (IsExtended(chest)) chest.Resize(CapacityFor(chest));
            return chest;
        }
        public static void Placed(int x, int y)
        {
            int index = Chest.FindChest(x, y - 1);
            if (index >= 0 && IsExtended(Main.chest[index])) Main.chest[index].Resize(CapacityFor(Main.chest[index]));
        }
    }
}
