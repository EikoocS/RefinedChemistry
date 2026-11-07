using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.TryAddDirectSellingItemToTable),
        new[] { typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(int) })]
    internal static class ChemistSupplies
    {
        private static readonly string[] StarterItemIds =
        {
            Flask.ItemId, Flask.ItemId, ImitationPipette.ItemId, AdjustableCylinder.ItemId
        };

        private static bool Prefix(PlayerStore __instance, ref string identifier)
        {
            if (__instance.currentClientInstance?.GetClientBlueprint().identifier != "retired_chemist")
            {
                return true;
            }
            switch (identifier)
            {
                case "blender":
                    if (RNG.Roll(25))
                        identifier = Diluter.ItemId;
                    break;
                case "chem_finisher":
                    identifier = ChemFactory.ItemId;
                    break;
                case "1ml_dropper":
                    identifier = ImitationPipette.ItemId;
                    break;
                case "5ml_dropper":
                    identifier = Pipette.ItemId;
                    break;
                case "25ml_cylinder":
                case "50ml_cylinder":
                case "100ml_cylinder":
                    return false;
            }
            return true;
        }

        private static void Postfix(PlayerStore __instance, string identifier)
        {
            if (__instance.currentClientInstance?.GetClientBlueprint().identifier == "retired_chemist" &&
                identifier == ChemFactory.ItemId)
            {
                __instance.AddDirectSellingItemToTable(AdjustableCylinder.ItemId);
                __instance.AddDirectSellingItemToTable(LabelerFactory.ItemId);
            }
        }

        internal static void OnUpgradeUnlocked(string upgradeId)
        {
            if (upgradeId != "RETIRED_CHEMIST")
            {
                return;
            }
            foreach (string id in StarterItemIds)
            {
                GraphUtils.EmporiumTryAdd(EmporiumEntry.Instance.backInvinvElement,
                    DirectoryMaster.Item(id));
            }
        }
    }
}
