using System.Collections.Generic;
using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DecodeNodes))]
    internal static class AutoCylinderMigrationPatch
    {
        private static void Prefix(List<SaveItemNode> savedItems)
        {
            foreach (var item in savedItems)
                if (item.identifier == "auto_cylinder")
                    item.identifier = AdjustableCylinder.ItemId;
        }
    }
}
