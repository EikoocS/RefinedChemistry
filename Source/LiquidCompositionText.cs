namespace RefinedChemistry
{
    internal static class LiquidCompositionText
    {
        internal static bool TryBuildPercentage(GameItem item, out string composition)
        {
            return TryBuildPercentage(item, out composition, out _, out _);
        }

        internal static bool TryBuildPercentage(GameItem item, out string composition,
            out Liquid mainLiquid, out int mainPurity)
        {
            composition = null;
            mainLiquid = null;
            mainPurity = 0;
            if (item == null || (!item.IsTag("LIQUID_CONTAINER_TAG") && !item.IsTag("CHEM_ITEM_TAG")))
            {
                return false;
            }

            int mainAmount = 0;
            int total = 0;
            foreach (Liquid liquid in Liquid.Liquids)
            {
                int amount = item.GetTagReadonly(liquid.currentPart).GetInt();
                if (amount <= 0)
                {
                    continue;
                }

                total += amount;
                if (amount > mainAmount)
                {
                    mainLiquid = liquid;
                    mainAmount = amount;
                }
            }

            if (total <= 0)
            {
                return false;
            }

            mainPurity = (int)((long)mainAmount * 10000 / total);
            composition = mainLiquid.displayName + " " + (mainAmount * 100.0 / total).ToString("0.#") + "%";
            return true;
        }
    }
}
