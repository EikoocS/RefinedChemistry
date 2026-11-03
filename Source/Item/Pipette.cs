using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    [ScrollAble(ItemId, nameof(OnScroll), nameof(GetCapacityText))]
    internal class Pipette
    {
        internal const string ItemId = "pipette";
        protected virtual string Id => ItemId;
        internal virtual int basePrice => 750;
        internal const int capacity = 10 * 1000; // ul

        internal const int stage0_MaxCapacity = 10 * 1000;
        internal const int stage0_MinCapacity = 1000;
        internal const int stage1_MaxCapacity = 1000;
        internal const int stage1_MinCapacity = 100;


        internal virtual GameItem Create()
        {
            var item = ItemHelper.Create(Id);
            WaterHelper.InitLiquidContainerItem(item, capacity / 1000, basePrice, isGraduated: true, isDropper: true);
            item.getItemSpriteModifierForItemFunc = container => GetLiquidSprites(container, ItemId);
            AudioHelper.InitPlasticBottle(item);
            return item;
        }

        internal static string GetCapacityText(GameItem item)
        {
            float capacity = WaterHelper.GetTotalCapacity(item);
            string capacityUnit = capacity >= 1000 ? "ml" : "μl";
            float total = WaterHelper.GetTotalVolume(item);
            string totalUnit = total >= 1000 ? "ml" : "μl";
            string text = LocalizationHelper.Format("pipette_capacity",
                capacity>=1000?capacity/1000f:capacity,
                capacityUnit,
                total>=1000?total/1000f:total,
                totalUnit);
            return text;
        }

        protected static void OnScroll(GameItem item, int delta)
        {
            int currentCapacity = WaterHelper.GetTotalCapacity(item);

            bool stage0 =currentCapacity > stage1_MaxCapacity ||
                (currentCapacity == stage1_MaxCapacity && delta > 0);

            int step = stage0 ? 1000 : 1;
            int min = stage0 ? stage0_MinCapacity : stage1_MinCapacity;
            int max = stage0 ? stage0_MaxCapacity : stage1_MaxCapacity;

            int targetCapacity = Mathf.Clamp(currentCapacity + delta * step, min, max);

            if (targetCapacity != currentCapacity)
            {
                item.ModifyTag(WaterHelper.LIQUID_CONTAINER_CAPACITY,
                    state => state.SetInt(targetCapacity));
            }

        }

        internal List<ItemSpriteModifier> GetLiquidSprites(GameItem item, string prefix)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
            {
                return null;
            }

            string color = WaterHelper.GetLiquidContainerColor(item);
            string colorBase = Liquid.GetColorBase(color);
            string spriteKey = prefix + "_" + colorBase;
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(RefinedChemistry.GetSprite(spriteKey), Vector2.zero,
                    color: MetaHelper.HexToColor(color))
            };
        }
    }

}
