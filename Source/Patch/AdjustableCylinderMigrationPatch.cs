using System.Collections.Generic;
using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DecodeNodes))]
    internal static class AdjustableCylinderMigrationPatch
    {
        private static void Prefix(List<SaveItemNode> savedItems)
        {
            foreach (var item in savedItems)
            {
                // TODO: 发布新的化学反应辅助烧杯时移除 beaker 迁移。
                if (item.identifier != "beaker" && item.identifier != "auto_cylinder" &&
                    item.identifier != AdjustableCylinder.ItemId)
                {
                    continue;
                }

                item.identifier = AdjustableCylinder.ItemId;
                item.name = LocalizationHelper.Item(AdjustableCylinder.ItemId, "name");
                item.shortDescription = LocalizationHelper.Item(AdjustableCylinder.ItemId, "desc");
                item.flavorText = LocalizationHelper.Item(AdjustableCylinder.ItemId, "flavor");
                item.spriteAtlasPath = RefinedChemistry.Instance.Manifest.ID;
                item.spritePath = AdjustableCylinder.ItemId;
                item.itemShape.SetDataFill(1, 4);
                item.itemModifiedShape.SetDataFill(1, 4);
            }
        }
    }
}
