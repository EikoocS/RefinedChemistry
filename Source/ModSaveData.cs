using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    internal static class ModSaveData
    {
        internal const string DrLiangStateKey = "npc.dr_liang.state";
        internal const string DrLiangTrustRawKey = "npc.dr_liang.trust.raw";
        internal const string DrLiangTrustMaxKey = "npc.dr_liang.trust.max";
        internal const string DrLiangClinicVolunteerTrustCorrectionKey = "npc.dr_liang.trust.correct.clinic_volunteer";

        private static string ModId => RefinedChemistry.Instance.Manifest.ID;

        internal static DrLiangData.NpcState DrLiangState
        {
            get => (DrLiangData.NpcState)ModHelper.GetModDataInt(ModId, DrLiangStateKey, (int)DrLiangData.NpcState.NORMAL);
            set => ModHelper.SetModDataInt(ModId, DrLiangStateKey, (int)value);
        }

        internal static float DrLiangTrustRaw
        {
            get => ModHelper.GetModDataFloat(ModId, DrLiangTrustRawKey);
            set => ModHelper.SetModDataFloat(ModId, DrLiangTrustRawKey, value);
        }

        internal static int DrLiangTrustMax
        {
            get => ModHelper.GetModDataInt(ModId, DrLiangTrustMaxKey, 0);
            set => ModHelper.SetModDataInt(ModId, DrLiangTrustMaxKey, value);
        }

        internal static int DrLiangClinicVolunteerTrustCorrection
        {
            get => ModHelper.GetModDataInt(ModId, DrLiangClinicVolunteerTrustCorrectionKey);
            set => ModHelper.SetModDataInt(ModId, DrLiangClinicVolunteerTrustCorrectionKey, value);
        }
    }
}
