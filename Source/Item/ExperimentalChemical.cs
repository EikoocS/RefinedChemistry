using System;

namespace RefinedChemistry.Item
{
    internal static class ExperimentalChemical
    {
        internal const string ItemId = "experimental_chemical";

        internal static GameItem Create()
        {
            GameItem item = ItemHelper.Create(ItemId);
            item.SetGameItemType("CHEMICAL_SUPPLIES");
            item.SetValue(80);
            item.canActivateSlotItemFunc = (current, _, _) => GeneralHelper.IsItemOwned(current);
            item.mayActivateSlotItemFunc = (current, _, _) => GeneralHelper.IsItemOwned(current);
            item.onActivateSlotItemFunc = (current, inventory, _) =>
            {
                Action unpack = () =>
                {
                    current.Destroy();
                    GraphUtils.EmporiumTryAdd(inventory, CreateAlcoholBottle());
                    GraphUtils.EmporiumTryAdd(inventory, CreateAlcoholBottle());
                    GraphUtils.EmporiumTryAdd(inventory,
                        CreateReagentBottle("med_bottle_small_pink", "lipobond_agent", 150000));
                    GraphUtils.EmporiumTryAdd(inventory,
                        CreateReagentBottle("med_bottle_red", "acid", 250000));
                    GraphUtils.EmporiumTryAdd(inventory,
                        CreateReagentBottle("med_bottle_blue", "base", 250000));
                };

                if (NetworkUpgrade.IsUnlocked("RETIRED_CHEMIST"))
                {
                    unpack();
                    return;
                }

                GenericConfirmationUIManager.Instance.OpenPanel(
                    LocHelper.GetLocalizedMechanic("mech_chemical_supplies_unpack_warning"),
                    LocHelper.GetLocalizedMainMenu("yes"), LocHelper.GetLocalizedMainMenu("no"),
                    () =>
                    {
                        GenericConfirmationUIManager.Instance.ClosePanel();
                        unpack();
                    },
                    () => GenericConfirmationUIManager.Instance.ClosePanel());
            };
            return item;
        }

        private static GameItem CreateAlcoholBottle()
        {
            GameItem bottle = DirectoryMaster.Item("mini_bottle");
            bottle.name = LocHelper.GetLocalizedOrDefault("Item", "item_mini_alcohol_bottle_name", "Mini Alcohol Bottle");
            WaterHelper.AddLiquid(bottle, "alcohol", 225000);
            WaterHelper.AddLiquid(bottle, "water", 25000);
            return bottle;
        }

        private static GameItem CreateReagentBottle(string bottleId, string liquidId, int volume)
        {
            GameItem bottle = DirectoryMaster.Item(bottleId);
            WaterHelper.AddLiquid(bottle, liquidId, volume);
            PreBuildChemHelper.SetImpurity(bottle, 100, 400); // 万分比：1%–4%。
            AudioHelper.InitPlasticContainer(bottle);
            return bottle;
        }
    }
}
