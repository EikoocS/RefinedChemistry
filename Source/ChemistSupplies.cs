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
            Flask.ItemId, Flask.ItemId, Pipette.ImitationItemId, Beaker.ItemId
        };

        private static void Prefix(PlayerStore __instance, ref string identifier)
        {
            if (__instance.currentClientInstance?.GetClientBlueprint().identifier != "retired_chemist")
            {
                return;
            }
            switch (identifier)
            {
                case "blender":
                    if (RNG.Roll(25))
                        identifier = Diluter.ItemId;
                    break;
                case "1ml_dropper":
                    if (RNG.Roll(75))
                        identifier = Pipette.ImitationItemId;
                    break;
                case "5ml_dropper":
                    if (RNG.Roll(10))
                        identifier = Pipette.ItemId;
                    break;
                case "25ml_cylinder":
                    if (RNG.Roll(75))
                        identifier = Beaker.ItemId;
                    break;
                case "50ml_cylinder":
                case "100ml_cylinder":
                    if (RNG.Roll(50))
                        identifier = Flask.ItemId;
                    break;
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
