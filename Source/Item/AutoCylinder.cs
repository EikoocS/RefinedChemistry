using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    [ScrollAble(ItemId, nameof(OnScroll), nameof(GetCapacityText))]
    internal static class AutoCylinder
    {
        internal const string ItemId = "auto_cylinder";

        internal const int maxCapacity = 100 * 1000;
        internal const int minCapacity = 10 * 1000;

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId, "beaker");
            WaterHelper.InitLiquidContainerItem(item, maxCapacity / 1000, 150, isGraduated: true);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            item.SetShape(new GridShapeBuilder().SetData([ 1, 1, 1, 1 ], 2));
            AudioHelper.InitGlass(item);
            return item;
        }

        internal static string GetCapacityText(GameItem item)
        {
            float capacityMl = WaterHelper.GetTotalCapacity(item) / 1000f;
            float totalMl = WaterHelper.GetTotalVolume(item) / 1000f;
            float freeMl = Mathf.Max(0, WaterHelper.GetFreeCapacity(item)) / 1000f;
            string text = LocalizationHelper.Format("auto_cylinder_capacity", capacityMl, totalMl, freeMl);
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
            }
        }

        private static List<ItemSpriteModifier> GetLiquidSprites(GameItem item)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
            {
                return null;
            }

            string color = WaterHelper.GetLiquidContainerColor(item);
            string spriteKey = "beaker_" + Liquid.GetColorBase(color);
            if (volume < WaterHelper.GetTotalCapacity(item))
            {
                spriteKey += "_half";
            }
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(RefinedChemistry.GetSprite(spriteKey), Vector2.zero,
                    color: MetaHelper.HexToColor(color))
            };
        }
    }
}
