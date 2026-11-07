using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(SecData), nameof(SecData.ApplyCrimeModifier),
        new[] { typeof(StoreClient), typeof(int) })]
    internal static class DrLiangBuyerEvidencePatch
    {
        private static void Postfix(StoreClient client, ref int __result)
        {
            if (client?.identifier == DrLiangBuyer.ClientId)
                __result /= 2;
        }
    }
}
