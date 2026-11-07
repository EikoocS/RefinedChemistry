using RefinedChemistry.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    [ScrollAble(ItemId, nameof(OnScroll), nameof(GetCapacityText))]
    [LocalizableAttributes("item", ItemId)]
    internal static partial class AdjustableCylinder
    {
        internal const string ItemId = "adjustable_cylinder";

        internal const int maxCapacity = 200 * 1000;
        internal const int minCapacity = 10 * 1000;

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId);
            WaterHelper.InitLiquidContainerItem(item, maxCapacity / 1000, 150, isGraduated: true);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            AudioHelper.InitGlass(item);
            return item;
        }

        internal static string GetCapacityText(GameItem item)
        {
            float capacityMl = WaterHelper.GetTotalCapacity(item) / 1000f;
            float totalMl = WaterHelper.GetTotalVolume(item) / 1000f;
            float freeMl = Mathf.Max(0, WaterHelper.GetFreeCapacity(item)) / 1000f;
            string text = Localize("capacity", capacityMl, totalMl, freeMl);
            return text;
        }

        private static void OnScroll(GameItem item, int delta)
        {
            int currentCapacity = WaterHelper.GetTotalCapacity(item);
            int targetCapacity = Mathf.Clamp(currentCapacity + delta * 1000, minCapacity, maxCapacity);
            if (targetCapacity != currentCapacity)
            {
                item.ModifyTag(WaterHelper.LIQUID_CONTAINER_CAPACITY,
                    state => state.SetInt(targetCapacity));
                SpriteHelper.UpdateWaterContainerSprite(item);
            }
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
