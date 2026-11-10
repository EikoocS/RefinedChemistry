using RefinedChemistry.Attributes;
using RefinedChemistry.Machinery;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    internal static class EnderFlask
    {
        internal const string ItemId = "ender_flask";

        internal static void CreateTooltip(RichTextBuilder builder, GameItem item)
        {
            string id = EnderFlaskLinks.GetLinkId(item);
            if (id == null)
                return;
            builder.NextLine();
            builder.Add("<mspace=0.5em>");
            for (int i = 0; i < 8; i++)
            {
                if (i == 4)
                    builder.Add(" ");
                int r = Convert.ToInt32(id[i + 8].ToString(), 16) * 17;
                int g = Convert.ToInt32(id[i + 16].ToString(), 16) * 17;
                int b = Convert.ToInt32(id[i + 24].ToString(), 16) * 17;
                var color = (RenderHandler.ColorPalette)((r << 16) | (g << 8) | b);
                builder.Add(char.ToUpperInvariant(id[i]).ToString(), color);
            }
            builder.Add("</mspace>");
            builder.AddLine();
        }

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId);
            WaterHelper.InitLiquidContainerItem(item, 750, 50, isGraduated: true);
            AudioHelper.InitGlass(item);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            item.mayInventoryAddItemFunc = (source, inventory) => EnderFlaskLinks.CanPlace(source, inventory);
            item.isItemStateValidFunc = (source, inventory, _, _, _) => EnderFlaskLinks.CanPlace(source, inventory);

            var mayTarget = item.mayThisTargetItemFunc;
            var canTarget = item.canThisTargetItemFunc;
            var use = item.onThisTargetItemFunc;
            var tick = item.onOneSecondItemFunc;
            item.mayThisTargetItemFunc = (source, target) => IsEnderFlask(target)
                ? source != target && WaterHelper.CanUseWaterContainer(source) && WaterHelper.CanUseWaterContainer(target)
                : mayTarget.InvokeAllReduce(source, target, InvokeFuncExtensions.BoolReduceOR);
            item.canThisTargetItemFunc = (source, target) => source != target &&
                !EnderFlaskLinks.ShareContents(source, target) &&
                (IsBindingInteraction(source, target)
                    ? CanBind(source, target)
                    : canTarget.InvokeAllReduce(source, target, InvokeFuncExtensions.BoolReduceOR));
            item.onThisTargetItemFunc = (source, target) =>
            {
                if (IsBindingInteraction(source, target))
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

        internal static bool IsEnderFlask(GameItem item) => item?.identifier == ItemId;

        private static bool IsBindingInteraction(GameItem source, GameItem target) =>
            IsEnderFlask(target) && WaterHelper.GetTotalVolume(source) == 0 && WaterHelper.GetTotalVolume(target) == 0;

        private static bool CanBind(GameItem source, GameItem target) =>
            source != target && WaterHelper.CanUseWaterContainer(source) && WaterHelper.CanUseWaterContainer(target) &&
            !EnderFlaskLinks.ShareContents(source, target) &&
            IsBindingInteraction(source, target);

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
