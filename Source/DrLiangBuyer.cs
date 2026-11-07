using RefinedChemistry.Attributes;
using HarmonyLib;

namespace RefinedChemistry
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangBuyer
    {
        private static readonly int[] Discounts = { -10, -17, -35, -50, -75 };

        internal static void ApplyDiscount(GameItem item)
        {
            if (!item.IsTag("MEDICAL"))
                return;

            int level = ContrabandHelper.GetContrabandLevel(item);
            ItemFeature feature = level switch
            {
                1 => ItemFeatureList.DiscountedLowContraband(),
                2 => ItemFeatureList.DiscountedMidContraband(),
                3 => ItemFeatureList.DiscountedHighContraband(),
                4 => ItemFeatureList.DiscountedCriticalContraband(),
                _ => ItemFeatureList.DiscountedBuy25()
            };
            if (level == 0)
            {
                feature.identifier = "refined_chemistry__dr_liang_sincerity";
                feature.category = feature.identifier;
                string name = Localize("sincerity");
                feature.SetPublicDisplay(name);
                feature.SetActualDisplay(name);
            }
            feature.SetPreExposedValueMod(Discounts[level]);
            item.AddItemFeature(feature);
        }

        [HarmonyPatch(typeof(SecData), nameof(SecData.ApplyCrimeModifier),
            new[] { typeof(StoreClient), typeof(int) })]
        private static class EvidencePatch
        {
            private static void Postfix(StoreClient client, ref int __result)
            {
                if (client?.identifier == DrLiang.BuyerVisitClientId)
                    __result /= 2;
            }
        }
    }
}
