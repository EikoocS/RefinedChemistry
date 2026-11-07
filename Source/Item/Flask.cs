using RefinedChemistry.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    internal static class Flask
    {
        internal const string ItemId = "flask";
        private const int InitialCapacity = 750;
        private const int MaxCapacity = 9000;

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId, "round_flask");
            WaterHelper.InitLiquidContainerItem(item, InitialCapacity, 50, isGraduated: true);
            AudioHelper.InitGlass(item);
            item.getItemSpriteModifierForItemFunc = GetLiquidSprites;
            ToggleAppearanceHelper.MakeSpriteCyclable(item, RefinedChemistry.Instance.Manifest.ID,
                "round_flask", "square_flask", "triangular_flask");
            item.onToggleSlotItemFunc += (toggled, _, _) =>
                ((GameItemElement)toggled).HandleInnateModSprite();

            var mayTarget = item.mayThisTargetItemFunc;
            var canTarget = item.canThisTargetItemFunc;
            var use = item.onThisTargetItemFunc;
            item.mayThisTargetItemFunc = (source, target) =>
                GetChemicalUpgrade(target) > 0 || mayTarget(source, target);
            item.canThisTargetItemFunc = (source, target) =>
                IsUpgradeInteraction(source, target)
                    ? CanUpgrade(source, target)
                    : canTarget(source, target);
            item.onThisTargetItemFunc = (source, target) =>
            {
                if (IsUpgradeInteraction(source, target))
                {
                    if (GetChemicalUpgrade(target) > 0)
                    {
                        Upgrade(source, target, GetChemicalUpgrade(target));
                    }
                    else
                    {
                        Upgrade(target, source, InitialCapacity);
                    }
                }
                else
                {
                    use(source, target);
                }
            };

            return item;
        }

        internal static void RegisterUpgradeMaterials(ModItemDirectory directory)
        {
            foreach (string id in new[] { "common_chemical", "restricted_chemical" })
            {
                var original = DirectoryMaster.GetFactory<GameItem>(id);
                directory.Set(id, () =>
                {
                    var item = original();
                    InitUpgradeMaterial(item);
                    return item;
                });
            }
        }

        private static void InitUpgradeMaterial(GameItem item)
        {
            item.maySelectSlotItemFunc += (_, _, _) => true;
            item.canSelectSlotItemFunc += (_, _, _) => true;
            item.mayThisTargetItemFunc += (_, target) => target?.identifier == ItemId;
            item.canThisTargetItemFunc += (_, target) => target?.identifier == ItemId &&
                WaterHelper.GetTotalCapacityML(target) < MaxCapacity;
            item.onThisTargetItemFunc += (source, target) =>
            {
                if (target?.identifier == ItemId)
                {
                    Upgrade(target, source, GetChemicalUpgrade(source));
                }
            };
        }

        private static int GetChemicalUpgrade(GameItem item)
        {
            return item?.identifier switch
            {
                "common_chemical" => InitialCapacity,
                "restricted_chemical" => 1500,
                _ => 0
            };
        }

        private static bool IsUpgradeInteraction(GameItem source, GameItem target)
        {
            return GetChemicalUpgrade(target) > 0 ||
                (target?.identifier == ItemId && source != target && WaterHelper.GetTotalVolume(source) == 0);
        }

        private static bool CanUpgrade(GameItem source, GameItem target)
        {
            var flask = GetChemicalUpgrade(target) > 0 ? source : target;
            return WaterHelper.GetTotalCapacityML(flask) < MaxCapacity;
        }

        private static void Upgrade(GameItem flask, GameItem material, int increase)
        {
            if (WaterHelper.GetTotalCapacityML(flask) >= MaxCapacity)
            {
                return;
            }
            int capacity = WaterHelper.GetTotalCapacityML(flask);
            int next = Mathf.Clamp(capacity + increase, InitialCapacity, MaxCapacity);
            flask.ModifyTag(WaterHelper.LIQUID_CONTAINER_CAPACITY, state => state.SetInt(next * 1000));
            if (material.unitCount > 1)
            {
                material.SetUnitCount(material.unitCount - 1);
            }
            else
            {
                material.Destroy();
            }
            SpriteHelper.UpdateWaterContainerSprite(flask);
        }

        private static List<ItemSpriteModifier> GetLiquidSprites(GameItem item)
        {
            int volume = WaterHelper.GetTotalVolume(item);
            if (volume == 0)
            {
                return null;
            }
            int capacity = WaterHelper.GetTotalCapacity(item);
            int level = volume * 4 <= capacity ? 25 : volume * 2 <= capacity ? 50 :
                volume * 4 <= capacity * 3 ? 75 : 100;
            string color = WaterHelper.GetLiquidContainerColor(item);
            string prefix = item.spritePath;
            return new List<ItemSpriteModifier>
            {
                new ItemSpriteModifier(RefinedChemistry.GetSprite(prefix + "_" + Liquid.GetColorBase(color) + "_" + level),
                    Vector2.zero, color: MetaHelper.HexToColor(color)),
                new ItemSpriteModifier(RefinedChemistry.GetSprite(prefix + "_hl_" + level), Vector2.zero, priority: 1)
            };
        }
    }
}
