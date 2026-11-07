using UnityEngine;

namespace RefinedChemistry.NPC
{
    internal static class DrLiangData
    {
        internal enum NpcState
        {
            NORMAL,
            REMOVED,
            END
        }

        internal const string StateKey = ModSaveData.DrLiangStateKey;
        internal const string TrustRawKey = ModSaveData.DrLiangTrustRawKey;
        internal const string TrustMaxKey = ModSaveData.DrLiangTrustMaxKey;
        internal const string ClinicVolunteerTrustCorrectionKey = ModSaveData.DrLiangClinicVolunteerTrustCorrectionKey;
        private const int ClinicVolunteerCorrectionLimit = 90;

        internal static NpcState State
        {
            get => ModSaveData.DrLiangState;
            set => ModSaveData.DrLiangState = value;
        }

        internal static float TrustRaw
        {
            get => ModSaveData.DrLiangTrustRaw;
            set => ModSaveData.DrLiangTrustRaw = Mathf.Clamp(value, 0, TrustMax);
        }

        internal static int TrustMax
        {
            get => ModSaveData.DrLiangTrustMax;
            set
            {
                ModSaveData.DrLiangTrustMax = Mathf.Clamp(value, 0, 100);
                TrustRaw = Mathf.Min(TrustRaw, TrustMax);
            }
        }

        internal static int ClinicVolunteerTrustCorrection
        {
            get => ModSaveData.DrLiangClinicVolunteerTrustCorrection;
            set => ModSaveData.DrLiangClinicVolunteerTrustCorrection = Mathf.Clamp(value, -10, 10);
        }

        internal static float Trust => Mathf.Clamp(TrustRaw +
            ApplyModifier(ClinicVolunteerTrustCorrection, ClinicVolunteerCorrectionLimit), 0, 100);

        private static float ApplyModifier(int modifier, int limit)
        {
            float raw = TrustRaw;
            return raw < limit ? Mathf.Min(modifier, limit - raw) : 0;
        }

        internal static void AddTradeTrust(float amount)
        {
            float limit = Mathf.Min(20, TrustMax);
            if (TrustRaw < limit)
                TrustRaw = Mathf.Min(limit, TrustRaw + amount);
        }
    }
}
