using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(LockHelper), nameof(LockHelper.CanOpenLock))]
    internal static class UtoMedicalBoxLockPatch
    {
        private static bool Prefix(GameItem box, string keyLockId, ref bool __result)
        {
            if (LockHelper.GetLock(box) != UtoMedicalBox.LockId)
                return true;

            __result = keyLockId == UtoMedicalBox.LockId;
            return false;
        }
    }
}
