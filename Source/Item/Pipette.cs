namespace RefinedChemistry.Item
{
    internal static class Pipette
    {
        internal const string ItemId = "pipette";
        internal const string ImitationItemId = "imitation_pipette";

        internal static GameItem Create()
        {
            var item = LiquidContainerItemHelper.Create(ItemId, "pipette", 10, 750, isDropper: true);
            AudioHelper.InitPlasticBottle(item);
            return item;
        }

        internal static GameItem CreateImitation()
        {
            var item = LiquidContainerItemHelper.Create(ImitationItemId, "pipette_flavor", 10, 100,
                isDropper: true, liquidSpritePrefix: "pipette");
            AudioHelper.InitPlasticBottle(item);

            DurabilityHelper.InitItemWithDurability(item, 100);
            var durability = item.FindItemFeatureByID(DurabilityHelper.DURABILITY_TAG);
            durability.SetFeatureDiscovered(true);
            durability.SetExposed(true);

            var canTarget = item.canThisTargetItemFunc;
            item.canThisTargetItemFunc = (source, target) => canTarget(source, target) &&
                (DurabilityHelper.GetRealDurability(source) > 0 ||
                    (!WaterHelper.IsEmpty(source) && !WaterHelper.IsFaucet(target)));
            item.canItemTargetThisFunc = (source, target) =>
                DurabilityHelper.GetRealDurability(target) > 0 ||
                !source.IsTag(WaterHelper.LIQUID_CONTAINER_TAG) ||
                (source.IsTag(WaterHelper.LIQUID_CONTAINER_DROPPER_TAG) && WaterHelper.IsEmpty(source));

            var use = item.onThisTargetItemFunc;
            item.onThisTargetItemFunc = (source, target) =>
            {
                int volume = WaterHelper.GetTotalVolume(source);
                use(source, target);
                if (WaterHelper.GetTotalVolume(source) < volume)
                {
                    DurabilityHelper.ChangeDurability(source, -1);
                }
            };
            return item;
        }

        internal static bool IsPipette(GameItem item)
        {
            return item != null && (item.identifier == ItemId || item.identifier == ImitationItemId);
        }

        internal static string GetCapacityText(GameItem item)
        {
            float capacityMl = WaterHelper.GetTotalCapacity(item) / 1000f;
            float totalMl = WaterHelper.GetTotalVolume(item) / 1000f;
            string text = LocalizationHelper.Format("pipette_capacity", capacityMl, totalMl);
            if (item.IsTag(DurabilityHelper.DURABILITY_TAG))
            {
                text += "\n" + DurabilityHelper.GetRealDisplayString(item);
            }
            return text;
        }

    }
}
