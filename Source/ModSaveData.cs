using UnityEngine;

namespace RefinedChemistry
{
    internal static class ModSaveData
    {
        internal const string DrLiangIntroCompletedKey = "dr_liang_intro_completed";
        internal const string DrLiangRemovedKey = "dr_liang_removed";
        internal const string DrLiangNameKnownKey = "dr_liang_name_known";
        internal const string DrLiangTrustRawKey = "dr_liang_trust_raw";
        internal const string DrLiangTrustLimitKey = "dr_liang_trust_limit";
        internal const string DrLiangNeedleGiftBonusKey = "dr_liang_needle_gift_bonus";
        internal const string DrLiangNeedleSalePenaltyKey = "dr_liang_needle_sale_penalty";

        private static string ModId => RefinedChemistry.Instance.Manifest.ID;

        internal static bool DrLiangIntroCompleted
        {
            get => ModHelper.GetModDataBool(ModId, DrLiangIntroCompletedKey);
            set => ModHelper.SetModDataBool(ModId, DrLiangIntroCompletedKey, value);
        }

        internal static bool DrLiangRemoved
        {
            get => ModHelper.GetModDataBool(ModId, DrLiangRemovedKey);
            set => ModHelper.SetModDataBool(ModId, DrLiangRemovedKey, value);
        }

        internal static bool DrLiangNameKnown
        {
            get => ModHelper.GetModDataBool(ModId, DrLiangNameKnownKey);
            set => ModHelper.SetModDataBool(ModId, DrLiangNameKnownKey, value);
        }

        internal static float DrLiangTrustRaw
        {
            get => ModHelper.GetModDataFloat(ModId, DrLiangTrustRawKey);
            set => ModHelper.SetModDataFloat(ModId, DrLiangTrustRawKey,
                Mathf.Clamp(value, 0, DrLiangTrustLimit));
        }

        internal static int DrLiangTrustLimit
        {
            get => ModHelper.GetModDataInt(ModId, DrLiangTrustLimitKey, 20);
            set
            {
                ModHelper.SetModDataInt(ModId, DrLiangTrustLimitKey, Mathf.Clamp(value, 0, 100));
                DrLiangTrustRaw = Mathf.Min(DrLiangTrustRaw, DrLiangTrustLimit);
            }
        }

        internal static int DrLiangNeedleGiftBonus
        {
            get => ModHelper.GetModDataInt(ModId, DrLiangNeedleGiftBonusKey);
            set => ModHelper.SetModDataInt(ModId, DrLiangNeedleGiftBonusKey, Mathf.Clamp(value, 0, 10));
        }

        internal static int DrLiangNeedleSalePenalty
        {
            get => ModHelper.GetModDataInt(ModId, DrLiangNeedleSalePenaltyKey);
            set => ModHelper.SetModDataInt(ModId, DrLiangNeedleSalePenaltyKey, Mathf.Clamp(value, -10, 0));
        }
    }
}
