using RefinedChemistry.Attributes;
using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.OnCustomerReported))]
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangReportPatch
    {
        private const int Reward = 100;
        private const int SecurityReputation = 5;

        private static void Prefix(PlayerStore __instance, out StoreClient __state)
        {
            StoreClient client = __instance.currentClientInstance?.GetClientBlueprint();
            __state = DrLiang.IsLiang(client) && !client.reported ? client : null;
        }

        private static void Postfix(PlayerStore __instance, StoreClient __state)
        {
            if (__state == null || !__state.reported)
                return;

            StoreClient client = __state;
            bool sellingIllegalGoods = client.IsSellingContraband() || client.IsSellingStolen();
            __instance.onSecAlarmLeftAction = () =>
            {
                __instance.playerCash += Reward;
                StoreReputation.ModSecReputation(StartingPerkContent.HandleReportRewardModelCitizen(
                    StartingPerkContent.HandleReportRewardConvict(SecurityReputation)));
                if (sellingIllegalGoods && client.reportDegradesFaction)
                    StoreReputation.ReportedClient(client.clientFaction);

                GameItem slip = DirectoryMaster.Item("sec_slip");
                string body = LocHelper.GetLocalizedMechanic("mech_report_wanted_body", Reward,
                    Localize("name"));
                body += "\n\n" + LocHelper.GetLocalizedMechanic("mech_report_rep_improved");
                if (client.reportDegradesFaction)
                    body += LocHelper.GetLocalizedMechanic("mech_report_faction_degraded",
                        StoreReputation.GetFactionNameByID(client.clientFaction));

                slip.ModifyTag("title", state => state.Enable().SetString(
                    LocHelper.GetLocalizedMechanic("mech_report_title")));
                slip.ModifyTag("body", state => state.Enable().SetString(body));
                NegociationUIManager.Instance.audioSourceCashRegister.Play();
                __instance.AddDirectSellingItemToTable(slip, isOwend: true);
                AchievementTracker.Instance.CheckModelCitizen();
                DrLiangData.State = DrLiangData.NpcState.REMOVED;
                __instance.storeClientManager.clientStack.RemoveAll(
                    queued => queued.identifier == DrLiangBuyer.ClientId ||
                        queued.identifier == DrLiangStory.Trust20ClientId);
            };
        }
    }
}
