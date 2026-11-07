using RefinedChemistry.Attributes;

namespace RefinedChemistry.Item
{
    [LocalizableAttributes("item", ItemId)]
    internal static partial class UtoMedicalCard
    {
        internal const string ItemId = "uto_medical_card";

        internal static GameItem Create()
        {
            GameItem item = ItemDirectory.CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId);
            item.name = Localize("name");
            item.flavorText = Localize("flavor");
            item.unitValue = 75;
            ContrabandHelper.InitContrabandItem(item, 1);
            item.SetGameItemType("ACCESS_CARD");
            AudioHelper.InitPlasticCard(item);
            item.maySelectSlotItemFunc = (_, _, _) => true;
            item.canSelectSlotItemFunc = item.maySelectSlotItemFunc;
            item.mayItemTargetItemFunc = (source, target) =>
                GeneralHelper.IsItemOwned(source) && GeneralHelper.IsItemOwned(target) &&
                LockHelper.IsLockedContainer(target);
            item.canItemTargetItemFunc = item.mayItemTargetItemFunc;
            item.onItemTargetItemFunc = (source, target) => LockHelper.TryOpenLock(source, target, UtoMedicalBox.LockId);
            return item;
        }
    }
}
