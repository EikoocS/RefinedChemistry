using HarmonyLib;
using RefinedChemistry.Machinery;
using System.Collections.Generic;
using System.Reflection;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.ValidateSelect))]
    internal static class EnderFlaskDragValidationPatch
    {
        private static bool Prefix(GameItemElement __instance) =>
            !EnderFlaskLinks.DeferDraggedValidation(__instance);
    }

    [HarmonyPatch]
    internal static class EnderFlaskDragRefreshPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ItemMouseDragHandler), "OnEventReset");
            yield return AccessTools.Method(typeof(ItemMultiSelectHandler), "EndGroupDrag");
            yield return AccessTools.Method(typeof(ItemMultiSelectHandler), "OnEventReset");
        }

        private static void Postfix() => EnderFlaskLinks.FlushDeferredRefresh();
    }
}
