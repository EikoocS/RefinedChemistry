using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch]
    internal static class DrLiangBargainDialoguePatch
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
            if (!DrLiangDialogue.IsCurrentClientLiang)
                return;

            foreach (var entry in Lines)
            {
                if (ReferenceEquals(entry.Field.GetValue(null), lines))
                {
                    DrLiangDialogue.SetText(__result, entry.Key);
                    return;
                }
            }
        }
    }
}
