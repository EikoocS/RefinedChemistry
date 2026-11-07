using RefinedChemistry.Attributes;

namespace RefinedChemistry.Item
{
    [LocalizableAttributes("item", ItemId)]
    internal static partial class UtoMedicalBox
    {
        internal const string ItemId = "uto_medical_box";
        internal const string UnlockedItemId = "uto_medical_box_u";
        internal const string LockId = "LOCK_ID_UTO_MEDICAL";

        internal static GameItem Create()
        {
            var (window, inventory) = DirectoryUtils.CreateInventoryWindow(7, 5);
            GameItem item = ItemDirectory.CreateEmptyItem(ItemId)
                .SetContentWindow(window)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId);
            item.name = Localize("name") + " (" + LocHelper.GetLocalizedMechanic("mech_lock_sealed") + ")";
            item.shortDescription = Localize("desc");
            item.flavorText = Localize("flavor");
            item.unitValue = 200;
            LockHelper.InitLockedContainerItem(item, LockId, inventory);
            LockHelper.LockUpContainer(item);
            return item;
        }

        internal static GameItem CreateUnlocked()
        {
            GameItem item = Create();
            item.identifier = UnlockedItemId;
            LockHelper.UnlockContainer(item, force: true);
            return item;
        }
    }
}
