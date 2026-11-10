using System.Collections.Generic;

namespace RefinedChemistry.Item
{
    internal static class UtoMedicalBoxLoot
    {
        internal static GameItem Create()
        {
            GameItem box = DirectoryMaster.Item(UtoMedicalBox.ItemId);
            var inventory = (GameGridInventory)box.contentWindow.child;
            inventory.mayInventoryAddItemFunc = null;

            AddSupplies(inventory);
            AddReagents(inventory);
            if (RNG.Roll(50))
                TryAdd(inventory, DirectoryMaster.Item(Flask.ItemId));
            AddMedicalItems(inventory);
            FillSequence(inventory, ["bandage_item", "hemostatic_bandage_item", "rubbing_alcohol"], true);
            FillSequence(inventory, ["caffeine_pill", "smelling_salt", "phagimycin_pill"]);

            LockHelper.LockUpContainer(box);
            return box;
        }

        private static void AddSupplies(GameGridInventory inventory)
        {
            int count = RNG.Roll(15, 85) ? 2 : 1;
            for (int i = 0; i < count; i++)
                TryAdd(inventory, DirectoryMaster.Item(ExperimentalChemical.ItemId));
            if (RNG.Roll(10))
                TryAdd(inventory, DirectoryMaster.Item("blue_blood_bag"));
            if (RNG.Roll(5))
            {
                for (int i = 0; i < 2; i++)
                    TryAdd(inventory, PreBuiltItemHelper.CreateRealGenuineInjector());
            }
        }

        private static void AddReagents(GameGridInventory inventory)
        {
            var liquids = new List<string> { "base", "acid", "reducing_agent", "lipobond_agent" };
            int count = RNG.Roll(30) ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                int index = RNG.GetRandomInt(0, liquids.Count - 1);
                string liquid = liquids[index];
                liquids.RemoveAt(index);
                GameItem flask = DirectoryMaster.Item(Flask.ItemId);
                WaterHelper.AddLiquid(flask, liquid, liquid == "lipobond_agent" ? 300000 : 500000);
                PreBuildChemHelper.SetImpurity(flask, 100, 500); // 万分比：1%–5% 杂质。
                TryAdd(inventory, flask);
            }
        }

        private static void AddMedicalItems(GameGridInventory inventory)
        {
            if (RNG.Roll(25))
                TryAdd(inventory, DirectoryMaster.Item("surgery_tool"));
            if (RNG.Roll(20))
                TryAdd(inventory, DirectoryMaster.Item("black_injector"));
            if (RNG.Roll(30))
                TryAdd(inventory, DirectoryMaster.Item("salve"));

            int roll = RNG.GetRandomInt(0, 99);
            if (roll < 50)
            {
                int count = roll < 20 ? 2 : 1;
                for (int i = 0; i < count; i++)
                    TryAdd(inventory, DirectoryMaster.Item("pink_injector"));
            }
            else
                TryAdd(inventory, DirectoryMaster.Item("blood_bag"));
        }

        private static void FillSequence(GameGridInventory inventory, string[] itemIds, bool leaveSingleCells = false)
        {
            bool added;
            do
            {
                added = false;
                foreach (string itemId in itemIds)
                {
                    if (leaveSingleCells && !HasAdjacentFreeCells(inventory))
                        return;

                    GameItem item = DirectoryMaster.Item(itemId);
                    if (itemId == "caffeine_pill" && RNG.Roll(15))
                    {
                        ChemFinisherRecipe.SetCompositionByPurity(item, ChemicalProductHelper.PURE_GRADE, 100, itemId);
                        ChemicalProductHelper.GradeProduct(item);
                    }
                    added |= TryAdd(inventory, item);
                }
            } while (added);
        }

        private static bool HasAdjacentFreeCells(GameGridInventory inventory)
        {
            GridShape shape = inventory.inventoryShape;
            var items = inventory.childItems;
            bool IsFree(int x, int y) => x < shape.width && y < shape.height &&
                shape.Get(x, y) == 0 && !items.Exists(item => item.modifiedShape.Intersects(x, y));

            for (int y = 0; y < shape.height; y++)
                for (int x = 0; x < shape.width; x++)
                    if (IsFree(x, y) && (IsFree(x + 1, y) || IsFree(x, y + 1)))
                        return true;
            return false;
        }

        private static bool TryAdd(GameGridInventory inventory, GameItem item)
        {
            if (GraphUtils.TryAcceptAll(inventory, item) == 0)
                return true;
            item.Destroy();
            return false;
        }
    }
}
