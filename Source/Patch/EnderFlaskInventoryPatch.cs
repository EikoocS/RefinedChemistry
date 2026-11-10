using HarmonyLib;
using RefinedChemistry.Machinery;
using System.Collections.Generic;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(SlotMarker), nameof(SlotMarker.TryAcceptOnce))]
    internal static class EnderFlaskPlacementPatch
    {
        private static bool Prefix(SlotMarker __instance, ref int __result)
        {
            if (EnderFlaskLinks.CanPlace(__instance.item, __instance.inventory))
                return true;
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameGridInventory), nameof(GameGridInventory.CheckShapePlacement))]
    internal static class EnderFlaskGroupPlacementPatch
    {
        private static void Postfix(GameGridInventory __instance, GameItem item,
            List<GameItem> ignoredItems, ref bool __result)
        {
            if (__result)
                __result = EnderFlaskLinks.CanPlace(item, __instance, ignoredItems);
        }
    }
}
