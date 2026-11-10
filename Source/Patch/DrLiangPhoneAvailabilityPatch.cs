using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(PhoneUIManager))]
    internal static class DrLiangPhoneAvailabilityPatch
    {
        [HarmonyPrefix, HarmonyPatch(nameof(PhoneUIManager.HandleCall))]
        private static bool HandleCall(PhoneUIManager __instance, long currentNumber, ref long ___currentNumber)
        {
            if (currentNumber != DrLiangPhone.Number || DrLiangPhone.IsAvailable)
                return true;

            ___currentNumber = 0;
            __instance.phoneClientDrop.Play();
            __instance.statusDisplay.text = LocHelper.GetLocalizedUI("ui_phone_no_answer");
            return false;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(PhoneUIManager.WillAnswerCall))]
        private static void WillAnswerCall(long number, ref bool __result)
        {
            if (number == DrLiangPhone.Number && !DrLiangPhone.IsAvailable)
                __result = false;
        }
    }
}
