using HarmonyLib;
using RefinedChemistry.NPC;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(AugHelper), nameof(AugHelper.GetRandomClientReactionUnholster))]
    internal static class DrLiangGunDialoguePatch
    {
        private static void Postfix(Dialogue __result)
        {
            if (!DrLiangDialogue.IsCurrentClientLiang)
                return;

            DrLiangDialogue.SetText(__result, "gunpoint_dialogue");
            DrLiangDialogue.SetText(__result.nextDialogue, "gunpoint_followup_dialogue");
        }
    }
}
