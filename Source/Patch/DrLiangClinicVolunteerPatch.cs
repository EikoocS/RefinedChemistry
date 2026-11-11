using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(StoreReputation), nameof(StoreReputation.CheckSoldNaloxgone))]
    internal static class DrLiangClinicVolunteerPatch
    {
        private static void Postfix(GameItem gameItem)
        {
            if (gameItem.identifier != "pure_white_injector")
                return;

            if (gameItem.IsConditionActive("freeFree"))
                DrLiangData.ClinicVolunteerTrustCorrection += 5 * gameItem.unitCount;
            else if (gameItem.IsConditionActive("freeNormal"))
                DrLiangData.ClinicVolunteerTrustCorrection -= 5 * gameItem.unitCount;
        }
    }
}
