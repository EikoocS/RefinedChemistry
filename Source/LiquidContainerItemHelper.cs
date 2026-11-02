using System.Collections.Generic;
using UnityEngine;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    internal static class LiquidContainerItemHelper
    {
        internal static GameItem Create(string id, string spriteKey, int capacityInMl, int basePrice,
            bool isDropper = false, string liquidSpritePrefix = null)
        {
            var manifest = ModEntry.Instance.Manifest;
            var item = ItemDirectory.CreateEmptyItem(id)
                .SetSpriteAndShapeFromMod(manifest.ID, spriteKey);
            item.name = LocalizationHelper.Format(id + "_name");
            item.shortDescription = LocalizationHelper.Format(id + "_desc");
            item.flavorText = LocalizationHelper.Format(id + "_flavor");
            WaterHelper.InitLiquidContainerItem(item, capacityInMl, basePrice,
                isGraduated: true, isDropper: isDropper);
            string prefix = liquidSpritePrefix ?? spriteKey;
            item.getItemSpriteModifierForItemFunc = container => GetLiquidSprites(container, prefix);
            return item;
        }

        internal static void AdjustCapacity(GameItem item, int deltaInMl, int minInMl, int maxInMl)
        {
            int capacity = WaterHelper.GetTotalCapacityML(item);
            int next = Mathf.Clamp(capacity + deltaInMl, minInMl, maxInMl);
            if (next != capacity)
            {
                item.ModifyTag(WaterHelper.LIQUID_CONTAINER_CAPACITY,
                    state => state.SetInt(next * 1000));
            }
        }

        private static List<ItemSpriteModifier> GetLiquidSprites(GameItem item, string prefix)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
            {
                return null;
            }

            string color = WaterHelper.GetLiquidContainerColor(item);
            string colorBase = Liquid.GetColorBase(color);
            string spriteKey = prefix + "_" + colorBase;
            if (item.identifier == Beaker.ItemId && volume < WaterHelper.GetTotalCapacity(item))
            {
                spriteKey += "_half";
            }
            var mod = ModLoader.Mods.Find(m => m.Manifest.ID == ModEntry.Instance.Manifest.ID);
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(mod.Sprites[spriteKey], Vector2.zero,
                    color: MetaHelper.HexToColor(color))
            };
        }
    }
}
