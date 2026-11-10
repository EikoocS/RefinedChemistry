using RefinedChemistry.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    internal static class ChemicalTank
    {
        internal const string ItemId = "chemical_tank";

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId);
            item.EnableTag(WaterTankHelper.WATER_TANK_TAG);
            item.SetGameItemType("HOUSEHOLD_GOOD");
            WaterHelper.InitLiquidContainerItem(item, 10000, 150, isGraduated: true);
            AudioHelper.InitSolidMetal(item);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            return item;
        }

        private static List<ItemSpriteModifier> GetLiquidSprites(GameItem item)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
            {
                return null;
            }

            float fill = (float)volume / WaterHelper.GetTotalCapacity(item);
            int level = fill <= 0.1f ? 10 : fill <= 0.25f ? 25 : fill <= 0.5f ? 50 :
                fill <= 0.75f ? 75 : fill <= 0.9f ? 90 : 100;
            string color = WaterHelper.GetLiquidContainerColor(item);
            string spriteKey = ItemId + "_" + Liquid.GetColorBase(color) + "_" + level;
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(RefinedChemistry.GetSprite(spriteKey), Vector2.zero,
                    color: MetaHelper.HexToColor(color)),
                new ItemSpriteModifier(RefinedChemistry.GetSprite(ItemId + "_hl_" + level),
                    Vector2.zero, priority: 1)
            };
        }
    }
}
