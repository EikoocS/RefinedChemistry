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
            GameItem item = ItemDirectory.CreateEmptyItem(UnlockedItemId)
                .SetContentWindow(window)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, UnlockedItemId);
            item.name = Localize("name");
            item.flavorText = Localize("flavor");
            item.unitValue = 15;
            ContainerHelper.InitContainerItem(inventory, item);
            return item;
        }

        internal static GameItem CreateLocked()
        {
            GameItem item = Create();
            item.identifier = ItemId;
            item.SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId);
            item.name += " (" + LocHelper.GetLocalizedMechanic("mech_lock_sealed") + ")";
            item.shortDescription = Localize("desc");
            item.unitValue = 200;
            item.DisableTag(GeneralHelper.IMPORTANT_TAG);
            LockHelper.InitLockedContainerItem(item, LockId, (GameInventory)item.contentWindow.child);
            LockHelper.LockUpContainer(item);
            return item;
        }
    }
}
