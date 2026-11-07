using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(LockHelper), nameof(LockHelper.CanOpenLock))]
    internal static class MedicalTechBoxLockPatch
    {
        private static bool Prefix(GameItem box, string keyLockId, ref bool __result)
        {
            if (LockHelper.GetLock(box) != MedicalTechItems.LockId)
                return true;

            __result = keyLockId == MedicalTechItems.LockId;
            return false;
        }
    }
}
