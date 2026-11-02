using HarmonyLib;

namespace RefinedChemistry
{
    internal static class ChemicalScanText
    {
        internal static bool TryBuildResult(GameItem item, out string text)
        {
            text = null;
            if (!LiquidCompositionText.TryBuildPercentage(item, out string composition,
                out Liquid mainLiquid, out int mainPurity))
            {
                return false;
            }

            text = mainLiquid.identifier == Liquid.LIQUID_WATER
                ? WaterFeatureHelper.GetWaterDisplayName(mainPurity) + "\n" + composition
                : composition;
            if (item.IsTag("CHEMICAL_PRODUCT_TAG"))
            {
                text = ChemicalProductHelper.GetChemicalFeature(item).realCondition.display + "\n" + text;
            }

            return true;
        }

        internal static void SetLastScanResult(GameItem item, string result)
        {
            if (!item.IsTag("LIQUID_CONTAINER_TAG"))
            {
                return;
            }

            string text = "[" + LocHelper.GetLocalizedMechanic("mech_last_scan") + "]: " + result;
            TagState state = item.GetTagReadonly("LAST_SCAN_RESULT_TAG");
            if (!state.IsEnabled() || !text.Equals(state.GetString()))
            {
                item.ModifyTag("LAST_SCAN_RESULT_TAG", tag => tag.Enable().SetString(text),
                    skipValidate: true);
                item.SyncModifiedState();
            }
        }
    }

    [HarmonyPatch(typeof(ChemScannerUIManager))]
    internal static class ChemicalScannerTextPatch
    {
        [HarmonyPostfix, HarmonyPatch(nameof(ChemScannerUIManager.RefreshUI))]
        private static void RefreshPostfix(ChemScannerUIManager __instance)
        {
            GameItem item = __instance.currentScannedItem;
            if (!ChemicalScanText.TryBuildResult(item, out string result))
            {
                return;
            }

            __instance.testedTMP.text = LocHelper.GetLocalizedMechanic("mech_tooltip_tested") + "\n" + result;
            ChemicalScanText.SetLastScanResult(item, result);
        }

        [HarmonyPostfix, HarmonyPatch(nameof(ChemScannerUIManager.OnMatch))]
        private static void MatchPostfix(ChemScannerUIManager __instance)
        {
            __instance.RefreshUI();
        }
    }
}
