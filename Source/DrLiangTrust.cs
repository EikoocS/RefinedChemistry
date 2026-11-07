using HarmonyLib;
using UnityEngine;

namespace RefinedChemistry
{
    internal static class DrLiangTrust
    {
        private const int NeedleGiftBonusLimit = 90;
        private const int NeedleSalePenaltyLimit = 90;

        internal static float Value => Mathf.Clamp(ModSaveData.DrLiangTrustRaw +
            ApplyModifier(ModSaveData.DrLiangNeedleGiftBonus, NeedleGiftBonusLimit) +
            ApplyModifier(ModSaveData.DrLiangNeedleSalePenalty, NeedleSalePenaltyLimit), 0, 100);

        private static float ApplyModifier(int modifier, int limit)
        {
            float raw = ModSaveData.DrLiangTrustRaw;
            return raw < limit ? Mathf.Min(modifier, limit - raw) : 0;
        }

        private static void AddTradeTrust(float amount)
        {
            float limit = Mathf.Min(20, ModSaveData.DrLiangTrustLimit);
            if (ModSaveData.DrLiangTrustRaw < limit)
                ModSaveData.DrLiangTrustRaw = Mathf.Min(limit, ModSaveData.DrLiangTrustRaw + amount);
        }

        internal static void OnItemBoughtByLiang(GameItem item, int price)
        {
            if (item.IsTag("MEDICAL"))
                AddTradeTrust(price / (ContrabandHelper.IsContraband(item) ? 40f : 10f));
        }

        internal static void OnItemSoldByLiang(GameItem item, int price)
        {
            AddTradeTrust(price / 20f);
        }

        [HarmonyPatch(typeof(StoreReputation), nameof(StoreReputation.CheckSoldNaloxgone))]
        private static class NeedleGiftPatch
        {
            private static void Postfix(GameItem gameItem)
            {
                if (gameItem.identifier != "pure_white_injector")
                    return;

                if (gameItem.IsConditionActive("freeFree"))
                    ModSaveData.DrLiangNeedleGiftBonus += 5 * gameItem.unitCount;
                else if (gameItem.IsConditionActive("freeNormal"))
                    ModSaveData.DrLiangNeedleSalePenalty -= 5 * gameItem.unitCount;
            }
        }
    }
}
