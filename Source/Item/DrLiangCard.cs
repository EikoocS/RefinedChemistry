namespace RefinedChemistry.Item
{
    internal static class DrLiangCard
    {
        internal const string ItemId = "dr_liang_card";

        internal static GameItem Create()
        {
            GameItem item = ItemHelper.Create(ItemId);
            item.unitValue = 0;
            item.SetGameItemType("DOCUMENT");
            item.EnableTag("IMPORTANT_TAG");
            AudioHelper.InitCartonCard(item);
            item.canActivateSlotItemFunc = (current, inventory, marker) => true;
            item.mayActivateSlotItemFunc = (current, inventory, marker) => true;
            item.onActivateSlotItemFunc = (current, inventory, marker) =>
                ModHelper.OpenPaper(RefinedChemistry.Instance.Manifest.ID, ItemId);
            return item;
        }
    }
}
