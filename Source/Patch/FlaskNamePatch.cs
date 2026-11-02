using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.GetDisplayName))]
    internal static class FlaskNamePatch
    {
        private static void Postfix(GameItem __instance, ref string __result)
        {
            if (__instance.identifier == Flask.ItemId && !GeneralHelper.HasCustomName(__instance) &&
                LiquidCompositionText.TryBuildPercentage(__instance, out string composition))
            {
                __result = composition;
            }
        }
    }
}
