using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(ItemFeature), nameof(ItemFeature.GetClientExposeDialog))]
    internal static class DrLiangBuyerChemicalDialoguePatch
    {
        private static void Postfix(ItemFeature __instance, Dialogue __result)
        {
            if (DrLiangDialogue.IsCurrentClientLiang &&
                PlayerStore.Instance.currentClientInstance.GetClientBlueprint().identifier == DrLiangBuyer.ClientId &&
                __instance.category == "CATEGORY_CHEMICAL_PURITY")
                DrLiangDialogue.SetText(__result, "buy_expose_chemical_dialogue");
        }
    }
}
