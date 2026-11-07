using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch]
    internal static class DrLiangUtilityDialoguePatch
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
            if (!DrLiangDialogue.IsCurrentClientLiang)
                return;

            string key = Keys[__originalMethod.Name];
            DrLiangDialogue.SetText(__result, key, PlayerStore.Instance.currentClientInstance.GetClientBlueprint().GetBudget());
            if (__result.nextDialogue != null)
                DrLiangDialogue.SetText(__result.nextDialogue, key.Replace("_dialogue", "_followup_dialogue"));
        }
    }
}
