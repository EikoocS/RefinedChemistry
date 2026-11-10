using RefinedChemistry.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    internal static class EnderFlask
    {
        internal const string ItemId = "ender_flask";

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId);
            WaterHelper.InitLiquidContainerItem(item, 1000, 50, isGraduated: true);
            AudioHelper.InitGlass(item);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            item.mayInventoryAddItemFunc = (source, inventory) => EnderFlaskLinks.CanPlace(source, inventory);
            item.isItemStateValidFunc = (source, inventory, _, _, _) => EnderFlaskLinks.CanPlace(source, inventory);

            var mayTarget = item.mayThisTargetItemFunc;
            var canTarget = item.canThisTargetItemFunc;
            var use = item.onThisTargetItemFunc;
            var tick = item.onOneSecondItemFunc;
            item.mayThisTargetItemFunc = (source, target) => IsFlask(target)
                ? source != target && WaterHelper.CanUseWaterContainer(source) && WaterHelper.CanUseWaterContainer(target)
                : mayTarget.InvokeAllReduce(source, target, InvokeFuncExtensions.BoolReduceOR);
            item.canThisTargetItemFunc = (source, target) => IsFlask(target)
                ? CanBind(source, target)
                : canTarget.InvokeAllReduce(source, target, InvokeFuncExtensions.BoolReduceOR);
            item.onThisTargetItemFunc = (source, target) =>
            {
                if (IsFlask(target))
                {
                    if (CanBind(source, target))
                        EnderFlaskLinks.Bind(source, target);
                }
                else
                {
                    use(source, target);
                }
            };
            item.onOneSecondItemFunc = (source, inventory, slot) =>
                EnderFlaskLinks.Tick(source, inventory, slot, tick);
            return item;
        }

        internal static bool IsFlask(GameItem item) => item?.identifier == ItemId;

        private static bool CanBind(GameItem source, GameItem target) =>
            source != target && WaterHelper.CanUseWaterContainer(source) && WaterHelper.CanUseWaterContainer(target) &&
            !EnderFlaskLinks.ShareContents(source, target) &&
            (WaterHelper.GetTotalVolume(source) == 0 || WaterHelper.GetTotalVolume(target) == 0);

        private static List<ItemSpriteModifier> GetLiquidSprites(GameItem item)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
                return null;
            int capacity = WaterHelper.GetTotalCapacity(item);
            int level = volume * 4 <= capacity ? 25 : volume * 2 <= capacity ? 50 :
                volume * 4 <= capacity * 3 ? 75 : 100;
            string color = WaterHelper.GetLiquidContainerColor(item);
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(RefinedChemistry.GetSprite(ItemId + "_" + Liquid.GetColorBase(color) + "_" + level),
                    Vector2.zero, color: MetaHelper.HexToColor(color)),
                new ItemSpriteModifier(RefinedChemistry.GetSprite(ItemId + "_hl_" + level),
                    Vector2.zero, priority: 1)
            };
        }
    }
}
