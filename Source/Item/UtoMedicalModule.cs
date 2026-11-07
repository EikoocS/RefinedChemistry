using RefinedChemistry.Attributes;

namespace RefinedChemistry.Item
{
    [LocalizableAttributes("item", ItemId)]
    internal static partial class UtoMedicalModule
    {
        internal const string ItemId = "uto_medical_module";

        internal static GameItem Create()
        {
            GameItem item = ItemDirectory.CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId);
            item.name = Localize("name");
            item.shortDescription = Localize("desc");
            item.unitValue = 250;
            ContrabandHelper.InitContrabandItem(item, 2);
            item.ModifyTag(MachineDesequencer.DESEQUENCER_TARGET_ID_TAG,
                state => state.Enable().SetString(UtoMedicalCard.ItemId));
            MachineProgressHelper.InitProgressSourceItem(item, 100);
            item.mayThisTargetItemFunc = CanSwap;
            item.canThisTargetItemFunc = CanSwap;
            item.onThisTargetItemFunc = (source, target) =>
            {
                if (CanSwap(source, target))
                    GraphUtils.SwapItemsInPlace(source, target);
            };
            AudioHelper.InitSmallModule(item);
            return item;
        }

        private static bool CanSwap(GameItem source, GameItem target) =>
            source != null && target != null && source != target &&
            source.IsTag(MachineDesequencer.DESEQUENCER_TARGET_ID_TAG) &&
            target.IsTag(MachineDesequencer.DESEQUENCER_TARGET_ID_TAG) &&
            !source.IsGameItemType("MACHINE") && !target.IsGameItemType("MACHINE") &&
            !target.MayStack(source) && source.parentInventory != null &&
            target.parentInventory != null && source.parentInventory != target.parentInventory;
    }
}
