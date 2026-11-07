using System.Collections.Generic;
using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    // TODO: 发布新的化学反应辅助烧杯时移除此迁移补丁。
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DecodeNodes))]
    internal static class BeakerMigrationPatch
    {
        private static void Prefix(List<SaveItemNode> savedItems)
        {
            foreach (var item in savedItems)
                if (item.identifier == "beaker")
                    item.identifier = AdjustableCylinder.ItemId;
        }
    }
}
