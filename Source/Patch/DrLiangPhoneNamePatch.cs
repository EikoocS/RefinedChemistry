using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(StorePhoneClient), nameof(StorePhoneClient.GetLocalizedDisplayName))]
    internal static class DrLiangPhoneNamePatch
    {
        private static bool Prefix(StorePhoneClient __instance, ref string __result)
        {
            if (__instance.dialogFuncId != DrLiangPhone.DialogId)
                return true;

            __result = DrLiang.DisplayName;
            return false;
        }
    }
}
