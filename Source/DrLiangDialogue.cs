using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace RefinedChemistry
{
    internal static class DrLiangDialogue
    {
        private static bool IsCurrentClientLiang => PlayerStore.HasInstance &&
            DrLiang.IsLiang(PlayerStore.Instance.currentClientInstance?.GetClientBlueprint());

        private static void SetText(Dialogue dialogue, string key, params object[] args)
        {
            dialogue.SetText(dialogue.title, LocalizationHelper.Format("dr_liang_" + key, args));
        }

        internal static Dialogue SetLines(Dialogue dialogue, string title, string key, params object[] args)
        {
            string[] lines = LocalizationHelper.Format(key, args).Split('\n');
            Dialogue last = dialogue.SetText(title, lines[0]);
            foreach (string line in lines.Skip(1))
                last = last.NextDialogue().SetText(title, line);
            return last;
        }

        internal static void Configure(StoreClient client)
        {
            client.customOnArrestDialogue = StoreClientDialogList.ClientArrestDialog();
            SetText(client.customOnArrestDialogue, "arrest_dialogue");
        }

        internal static void ConfigureTrade(StoreClient client)
        {
            SetText(client.acceptDealDialogue, "deal_accepted_dialogue");
        }

        internal static void ConfigureBuyer(StoreClient client)
        {
            SetLines(client.mainDialogue, client.displayName, "dr_liang_buy_dialogue", client.GetBudget());
            SetText(client.acceptDealDialogue, "buy_deal_accepted_dialogue");
            SetText(client.allDoneDialogue, "buy_done_dialogue");
            client.placeRightItemWhenSellingToDialogue = new Dialogue().SetText(client.displayName,
                LocalizationHelper.Format("dr_liang_buy_right_item_dialogue"));
            client.placedWrongItemWhenSellingToDialogue = new Dialogue().SetText(client.displayName,
                LocalizationHelper.Format("dr_liang_buy_wrong_item_dialogue"));
        }

        [HarmonyPatch(typeof(ItemFeature), nameof(ItemFeature.GetClientExposeDialog))]
        private static class BuyerChemicalDialoguePatch
        {
            private static void Postfix(ItemFeature __instance, Dialogue __result)
            {
                if (IsCurrentClientLiang &&
                    PlayerStore.Instance.currentClientInstance.GetClientBlueprint().identifier == DrLiang.BuyerVisitClientId &&
                    __instance.category == "CATEGORY_CHEMICAL_PURITY")
                    SetText(__result, "buy_expose_chemical_dialogue");
            }
        }

        [HarmonyPatch]
        private static class BargainDialoguePatch
        {
            private static readonly (FieldInfo Field, string Key)[] Lines = new[]
            {
                ("acceptGratefulLines", "bargain_accept_grateful"),
                ("acceptVeryGratefulLines", "bargain_accept_very_grateful"),
                ("acceptReluctantLines", "bargain_accept_reluctant"),
                ("acceptVeryReluctantLines", "bargain_accept_very_reluctant"),
                ("rejectLines", "bargain_reject"),
                ("hardRejectLines", "bargain_hard_reject"),
                ("leaveLines", "bargain_leave"),
                ("budgetIncreaseLines", "bargain_budget_increase"),
                ("budgetDecreaseLines", "bargain_budget_decrease"),
                ("talkBadLines", "bargain_talk_bad"),
                ("innocentBlackmailLines", "bargain_innocent_blackmail"),
                ("falseThreatenLines", "bargain_false_threaten")
            }.Select(entry => (AccessTools.Field(typeof(BargainUIManager), entry.Item1),
                entry.Item2 + "_dialogue")).ToArray();

            private static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(BargainUIManager), "CreateClientDialog");
                yield return AccessTools.Method(typeof(BargainUIManager), "AppendClientLine");
            }

            private static void Postfix(List<string> lines, Dialogue __result)
            {
                if (!IsCurrentClientLiang)
                    return;

                foreach (var entry in Lines)
                {
                    if (ReferenceEquals(entry.Field.GetValue(null), lines))
                    {
                        SetText(__result, entry.Key);
                        return;
                    }
                }
            }
        }

        [HarmonyPatch]
        private static class UtilityDialoguePatch
        {
            private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>
            {
                [nameof(StoreClientDialogList.CreateOutOfBudgetDialogue)] = "out_of_budget_dialogue",
                [nameof(StoreClientDialogList.CreateNoContrabandDialog)] = "no_contraband_dialogue",
                [nameof(StoreClientDialogList.CreateDefaultAllDoneDialog)] = "all_done_dialogue",
                [nameof(StoreClientDialogList.CreateShutterDownClose)] = "shutter_down_dialogue",
                [nameof(StoreClientDialogList.CreateAfterBuyingFromClientDialog)] = "after_buying_dialogue",
                [nameof(StoreClientDialogList.CreateAfterSellingToClientDialog)] = "after_selling_dialogue",
                [nameof(StoreClientDialogList.ClientAlarmInitialReactionDialog)] = "alarm_reaction_dialogue"
            };

            private static IEnumerable<MethodBase> TargetMethods()
            {
                return Keys.Keys.Select(name => AccessTools.Method(typeof(StoreClientDialogList), name));
            }

            private static void Postfix(MethodBase __originalMethod, Dialogue __result)
            {
                if (!IsCurrentClientLiang)
                    return;

                string key = Keys[__originalMethod.Name];
                SetText(__result, key, PlayerStore.Instance.currentClientInstance.GetClientBlueprint().GetBudget());
                if (__result.nextDialogue != null)
                    SetText(__result.nextDialogue, key.Replace("_dialogue", "_followup_dialogue"));
            }
        }

        [HarmonyPatch(typeof(AugHelper), nameof(AugHelper.GetRandomClientReactionUnholster))]
        private static class GunDialoguePatch
        {
            private static void Postfix(Dialogue __result)
            {
                if (!IsCurrentClientLiang)
                    return;

                SetText(__result, "gunpoint_dialogue");
                SetText(__result.nextDialogue, "gunpoint_followup_dialogue");
            }
        }
    }
}
