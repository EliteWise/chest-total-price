using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SpecialOrders.Rewards;
using HarmonyLib;
using System;

namespace ChestTotalPrice
{
    public class ModEntry : Mod
    {
        private ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            bool exampleBool = this.Config.ExampleBoolean;

            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                postfix: new HarmonyMethod(typeof(ChestMenuDrawPatch), nameof(ChestMenuDrawPatch.Draw_Postfix))
            );
        }

        private static void drawTotalValue(Chest chest, SpriteBatch b, int numDigits, int temp, int chestXpos, int chestYpos, int xOffset, int policeSize, int moneyIconY)
        {
            for (int i = numDigits - 1; i >= 0; i--)
            {
                int digit = (temp / (int)Math.Pow(10, i)) % 10;
                b.Draw(Game1.mouseCursors,
                    new Vector2(385 + (chestXpos + xOffset), chestYpos - 33),
                    new Rectangle(286, 502 - digit * 8, 5, 8),
                    Color.Maroon,
                    0f, Vector2.Zero, 3f + policeSize, SpriteEffects.None, 1f);
                xOffset += chest.specialChestType.Get() == Chest.SpecialChestTypes.BigChest ? 14 : 18;
            }
            b.Draw(Game1.mouseCursors_1_6,
                new Vector2(385 + (chestXpos + xOffset), chestYpos + moneyIconY - 31),
                new Rectangle(132, 0, 15, 16),
                Color.White);
        }

        public static class ChestMenuDrawPatch
        {

            public static void Draw_Postfix(ItemGrabMenu __instance, SpriteBatch b)
            {
                IList<Item> actualInventory = __instance.ItemsToGrabMenu.actualInventory;
                int totalPrice = 0;

                foreach (var item in actualInventory)
                {
                    totalPrice += Utility.getSellToStorePriceOfItem(item, true);
                }

                int chestYpos = __instance.yPositionOnScreen;
                int chestXpos = __instance.xPositionOnScreen;

                int xOffset = 0;
                int temp = totalPrice;
                int numDigits = totalPrice.ToString().Length;

                if (__instance.sourceItem is Chest chest)
                {
                    if (chest.specialChestType.Get() == Chest.SpecialChestTypes.None)
                    {
                        drawTotalValue(chest, b, numDigits, temp, chestXpos, chestYpos, xOffset, 0, 0);

                    }
                    else if (chest.specialChestType.Get() == Chest.SpecialChestTypes.BigChest)
                    {
                        drawTotalValue(chest, b, numDigits, temp, chestXpos, chestYpos - 47, xOffset, -1, - 4);
                    }
                }

            }

        }
    }
}

