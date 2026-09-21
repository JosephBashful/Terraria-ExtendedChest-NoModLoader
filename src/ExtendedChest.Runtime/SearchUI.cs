using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ExtendedChest
{
    public static class SearchUI
    {
        private static string query = "";
        private static int currentChest = -1;
        private static bool focused;
        private static bool previousBlockInput;
        private static bool draggingScrollbar;
        private static int scrollbarDragOffset;
        private static readonly List<int> matches = new List<int>();

        private static void Focus(bool value)
        {
            if (focused == value) return;
            if (value) { previousBlockInput = Main.blockInput; Main.blockInput = true; }
            else Main.blockInput = previousBlockInput;
            focused = value;
        }

        public static void Update()
        {
            if (Main.gameMenu || Main.LocalPlayer == null || !Main.playerInventory ||
                Main.LocalPlayer.chest < 0 || Main.LocalPlayer.chest != currentChest ||
                Main.editChest || Terraria.GameContent.UI.NewCraftingUI.Visible)
            {
                Focus(false);
                draggingScrollbar = false;
            }
            if (focused) PlayerInput.WritingText = true;
        }

        private static void DrawScrollbar(SpriteBatch batch, Player player, int top)
        {
            var track = new Rectangle(498, top, 8, 170);
            int totalRows = Math.Max(1, (matches.Count + 9) / 10);
            int thumbHeight = Math.Max(20, track.Height * Math.Min(4, totalRows) / totalRows);
            int travel = track.Height - thumbHeight;
            int thumbY = track.Y;
            if (ChestUI.LastHighestChestRow > 0 && travel > 0)
                thumbY += (int)Math.Round((double)ChestUI.StartingRowForDrawing / ChestUI.LastHighestChestRow * travel);
            var thumb = new Rectangle(track.X, thumbY, track.Width, thumbHeight);
            bool overTrack = track.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;
            if (overTrack) player.mouseInterface = true;

            if (Main.mouseLeft && Main.mouseLeftRelease && overTrack)
            {
                draggingScrollbar = true;
                scrollbarDragOffset = thumb.Contains(Main.mouseX, Main.mouseY)
                    ? Main.mouseY - thumb.Y
                    : thumbHeight / 2;
                Main.mouseLeftRelease = false;
            }
            if (!Main.mouseLeft) draggingScrollbar = false;
            if (draggingScrollbar)
            {
                player.mouseInterface = true;
                int position = Math.Max(0, Math.Min(travel, Main.mouseY - track.Y - scrollbarDragOffset));
                ChestUI.StartingRowForDrawing = travel == 0
                    ? 0
                    : (int)Math.Round((double)position / travel * ChestUI.LastHighestChestRow);
                thumb.Y = track.Y + position;
            }

            batch.Draw(TextureAssets.MagicPixel.Value, track, new Color(18, 25, 38, 220));
            batch.Draw(TextureAssets.MagicPixel.Value, thumb,
                overTrack || draggingScrollbar ? Main.OurFavoriteColor : new Color(110, 130, 155, 255));
        }

        // Return true to replace only the extended chest's slot panel.
        public static bool Draw(SpriteBatch batch)
        {
            Player player = Main.LocalPlayer;
            if (player.chest < 0 || !Hooks.IsExtended(Main.chest[player.chest]))
            {
                Focus(false);
                return false;
            }
            if (currentChest != player.chest)
            {
                Focus(false);
                draggingScrollbar = false;
                query = "";
                currentChest = player.chest;
                ChestUI.StartingRowForDrawing = 0;
            }
            Chest chest = Main.chest[player.chest];
            Main.inventoryScale = 0.755f;
            int top = Main.instance.invBottom;
            var searchRect = new Rectangle(73, top + 174, 422, 28);
            bool hover = searchRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;
            if (hover) player.mouseInterface = true;
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                Focus(hover && !Main.editChest);
                if (hover) Main.mouseLeftRelease = false;
            }
            if (hover && Main.mouseRight && Main.mouseRightRelease)
            {
                query = "";
                ChestUI.StartingRowForDrawing = 0;
                Main.mouseRightRelease = false;
            }
            if (focused)
            {
                PlayerInput.WritingText = true;
                string updated = Main.GetInputText(query);
                if (updated.Length > 64) updated = updated.Substring(0, 64);
                if (query != updated) ChestUI.StartingRowForDrawing = 0;
                query = updated;
                if (Main.keyState.IsKeyDown(Keys.Escape) || Main.keyState.IsKeyDown(Keys.Enter)) Focus(false);
            }
            batch.Draw(TextureAssets.MagicPixel.Value, searchRect, focused ? new Color(35, 65, 95, 240) : new Color(25, 35, 55, 230));
            string text = query.Length == 0 && !focused ? "Search items..." : query + (focused ? "|" : "");
            ChatManager.DrawColorCodedStringWithShadow(batch, FontAssets.MouseText.Value, text,
                new Vector2(searchRect.X + 6, searchRect.Y + 3), Color.White, 0f, Vector2.Zero, new Vector2(0.8f));

            matches.Clear();
            int usedSlots = 0;
            for (int slot = 0; slot < chest.maxItems; slot++)
            {
                if (!chest.item[slot].IsAir) usedSlots++;
                if (query.Length == 0 || (!chest.item[slot].IsAir && chest.item[slot].Name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0))
                    matches.Add(slot);
            }
            ChestUI.LastHighestChestRow = Math.Max(0, (matches.Count + 9) / 10 - 4);
            ChestUI.StartingRowForDrawing = Math.Max(0, Math.Min(ChestUI.StartingRowForDrawing, ChestUI.LastHighestChestRow));
            ChestUI.LastChestDisplayRectangle = new Rectangle(73, top, 433, 170);
            DrawScrollbar(batch, player, top);
            ItemSlot.PrepareForChest(chest);
            for (int visible = 0; visible < 40; visible++)
            {
                int position = ChestUI.StartingRowForDrawing * 10 + visible;
                if (position >= matches.Count) break;
                int slot = matches[position];
                var point = new Vector2(73 + (visible % 10) * 56 * Main.inventoryScale, top + (visible / 10) * 56 * Main.inventoryScale);
                var rect = new Rectangle((int)point.X, (int)point.Y, 39, 39);
                if (rect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface)
                {
                    player.mouseInterface = true;
                    ItemSlot.Handle(chest.item, 3, slot);
                }
                ItemSlot.Draw(batch, chest.item, 3, slot, point);
            }
            string usage = "Slots: " + usedSlots + " used / " + (chest.maxItems - usedSlots) + " available";
            ChatManager.DrawColorCodedStringWithShadow(batch, FontAssets.MouseText.Value, usage,
                new Vector2(searchRect.X, searchRect.Bottom + 2), Color.LightGray, 0f, Vector2.Zero, new Vector2(0.75f));
            return true;
        }
    }
}
