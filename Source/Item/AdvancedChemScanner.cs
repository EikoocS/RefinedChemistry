using RefinedChemistry.UI;

namespace RefinedChemistry.Item
{
    internal static class AdvancedChemScanner
    {
        internal const string ItemId = "advanced_chem_scanner";

        internal static GameItem Create()
        {
            var item = ItemHelper.Create(ItemId);
            item.SetGameItemType("TOOL");
            AudioHelper.InitScanner(item);
            item.EnableTag("CAN_USE_ON_NON_OWNED_TAG");
            item.maySelectSlotItemFunc = (_, _, _) => true;
            item.canSelectSlotItemFunc = (_, _, _) => true;
            item.mayThisTargetItemFunc = CanScan;
            item.canThisTargetItemFunc = CanScan;
            item.onThisTargetItemFunc = (_, target) => Scan(target);
            item.mayToggleSlotItemFunc = (source, _, _) => GeneralHelper.IsItemOwned(source);
            item.canToggleSlotItemFunc = item.mayToggleSlotItemFunc;
            item.onToggleSlotItemFunc = (source, _, _) =>
            {
                if (source.IsTag(GeneralHelper.IS_ITEM_TOGGLE_ON_TAG))
                    source.DisableTag(GeneralHelper.IS_ITEM_TOGGLE_ON_TAG);
                else
                    source.EnableTag(GeneralHelper.IS_ITEM_TOGGLE_ON_TAG);
                UpdateSprite(source);
                AudioManager.Instance.Play3("scanner_activate");
            };
            item.onLoaded += UpdateSprite;
            return item;
        }

        private static void UpdateSprite(GameItem item) =>
            item.SetSpriteQuick(RefinedChemistry.Instance.Manifest.ID,
                item.IsTag(GeneralHelper.IS_ITEM_TOGGLE_ON_TAG) ? ItemId + "_on" : ItemId);

        private static bool CanScan(GameItem source, GameItem target) =>
            target.IsTag("LIQUID_CONTAINER_TAG") || target.IsTag("CHEM_ITEM_TAG") ||
            target.IsTag("CHEMICAL_PRODUCT_TAG");

        private static void Scan(GameItem target)
        {
            AdvancedChemScannerUI.Show(target);
            AudioManager.Instance.Play3("scanner_activate");
        }
    }
}
