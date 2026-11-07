using RefinedChemistry.Attributes;
using RefinedChemistry.Machinery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RefinedChemistry.Item
{
    [LocalizableAttributes("item", ItemId)]
    internal sealed partial class ChemFactory
    {
        internal const string ItemId = "chem_factory";
        private const string PurityTag = "CHEM_FACTORY_PURITY";
        private const string QuantityTag = "CHEM_FACTORY_QUANTITY";
        private static readonly int[] GradePurities =
        [
            ChemicalProductHelper.PURE_GRADE,
            ChemicalProductHelper.PHARMA_GRADE,
            ChemicalProductHelper.NEAR_PHARMA_GRADE,
            ChemicalProductHelper.STREET_STANDARD_GRADE,
            ChemicalProductHelper.CUT_PRODUCT_GRADE,
            ChemicalProductHelper.JUNK_GRADE,
            ChemicalProductHelper.STARCH_GRADE
        ];

        private readonly GameGridInventory modules = new(6, 6);
        private readonly GameSlotInventory source = CreateSlot();
        private readonly GameSlotInventory auxiliary = CreateSlot();
        private readonly GameGridInventory output = new(10, 10);
        private readonly MachineryControl purity = new(CustomUIManager.ElementKind.Dropdown);
        private readonly List<int> purityOptions = new();
        private readonly MachineryControl quantity = new(CustomUIManager.ElementKind.Input, 28);
        private readonly MachineryText fixedPurity = Label();
        private readonly MachineryText recipeLabel = Label();
        private readonly MachineryText consumption = Label();
        private readonly MachineryText maximumPurity = Label();
        private readonly MachineryText maximumQuantity = Label();
        private readonly MachineryButton configure = new(NativeText("btn_configure"));
        private readonly MachineryButton produce = new(NativeText("btn_output"));
        private readonly GameItem machine;
        private readonly PixelWindow window;
        private ChemFactoryPlan plan;
        private (string Inputs, double? Target, int Quantity) planKey;
        private string shapeRecipe;
        private GridShape productShape;
        private int outputCapacity = -1;
        private bool executing;
        private bool hadContents;
        private bool editablePurity;
        private bool refreshLock;
        private bool displayDirty = true;

        internal static GameItem Create() => new ChemFactory().machine;

        internal static void CreateTooltip(RichTextBuilder builder, GameItem item)
        {
            if (item.identifier != ItemId)
                return;
            int loss = MachineChemFinisher.GetLoss(item);
            if (loss > 0)
                builder.AddLine(LocHelper.GetLocalizedMechanic("mech_chem_finisher_contaminant", loss),
                    ignoreEmpty: true, RenderHandler.ColorPalette.LightRed);
        }

        private ChemFactory()
        {
            quantity.input.contentType = TMP_InputField.ContentType.IntegerNumber;
            quantity.input.characterLimit = 3;
            quantity.input.SetTextWithoutNotify("1");
            var purityRow = MachineryStack.Horizontal(Label(Localize("purity")),
                purity, fixedPurity, Label("/"), maximumPurity);
            var quantityRow = MachineryStack.Horizontal(Label(Localize("quantity")),
                quantity, Label("/"), maximumQuantity);
            var controls = MachineryStack.Vertical(configure, recipeLabel,
                purityRow, quantityRow, consumption, produce);
            window = MachineHelper.CreateMachineryWindow(true,
                MachineHelper.Panel(NativeText("label_module"), modules, "module"),
                new MachineryPanel(Localize("inputs"), MachineryStack.Vertical(
                    MachineHelper.Panel(Localize("source"), source, "source"),
                    MachineHelper.Panel(Localize("auxiliary"), auxiliary, "auxiliary")), "inputs"),
                MachineHelper.Panel(NativeText("label_output"), output, "output"),
                new MachineryPanel(NativeText("label_control"), controls, "control"));
            machine = ItemDirectory.CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, ItemId + "_0")
                .SetContentWindow(window);
            machine.name = Localize("name");
            machine.shortDescription = Localize("desc");
            machine.unitValue = 300;
            machine.SetGameItemType("MACHINE");
            machine.EnableTag("CONTAINER_TAG");
            AudioHelper.InitMetalMachine(machine);
            machine.getItemSpriteModifierForItemFunc = GetContentsSprites;
            SpriteAnimationHelper.InitSpriteAnimation(machine,
                ["chem_factory_1", "chem_factory_2", "chem_factory_3",
                 "chem_factory_4", "chem_factory_5", "chem_factory_6"], 100);
            MachineBlender.SetupChemModuleGrid(modules, machine);
            output.mayInventoryAddItemFunc = ContainerHelper.CanContainerStoredItem;
            configure.SetCallback(() => ChemFinisherUIManager.Instance.OpenUI(machine));
            produce.SetCallback(Execute);
            purity.dropdown.onValueChanged.AddListener(index =>
            {
                SavePurity(GradePurities[purityOptions[index]]);
                RefreshDisplay();
            });
            quantity.input.onValueChanged.AddListener(value => EditQuantity(value, false));
            quantity.input.onEndEdit.AddListener(value => EditQuantity(value, true));
            foreach (GameInventory inventory in new GameInventory[] { modules, source, auxiliary, output })
            {
                inventory.onSlotAddItemEndFunc += (_, _, _) => InventoryChanged(inventory);
                inventory.onSlotRemoveItemEndFunc += (_, _, _) => InventoryChanged(inventory);
            }
            machine.onOneSecondItemFunc = (_, _, _) => UpdateContents();
            window.handler.gameObject.AddComponent<MachineryRefresh>().refresh = Refresh;
            Refresh();
        }

        private ChemFinisherRecipe Recipe => ChemFinisherRecipe.GetRecipeById(
            machine.GetTagReadonly(MachineChemFinisher.SELECTED_CHEM_ID_TAG).GetString());

        private double UpdatePurityOptions(double maximum)
        {
            var available = new List<int>();
            for (int i = 0; i < GradePurities.Length; i++)
                if (GradePurities[i] <= maximum)
                    available.Add(i);
            if (!purityOptions.SequenceEqual(available))
            {
                purity.dropdown.Hide();
                purityOptions.Clear();
                purityOptions.AddRange(available);
                purity.dropdown.ClearOptions();
                purity.dropdown.AddOptions(available.Select(index =>
                    ItemConditionList.conditionChemicalPurity[index].display + " (" + GradePurities[index] + "%)").ToList());
            }
            double target = machine.GetTagReadonly(PurityTag).GetDouble();
            int selected = machine.IsTag(PurityTag) ? available.FindIndex(index => GradePurities[index] == target) : -1;
            if (selected < 0)
                selected = 0;
            target = GradePurities[available[selected]];
            SavePurity(target);
            purity.dropdown.SetValueWithoutNotify(selected);
            purity.dropdown.RefreshShownValue();
            return target;
        }

        private void EditQuantity(string value, bool finished)
        {
            if (int.TryParse(value, out int count) || finished)
            {
                int maximum = plan?.MaximumCount ?? 0;
                count = finished && count <= 0 ? Math.Min(1, maximum) : Math.Clamp(count, 0, maximum);
                SaveQuantity(count);
                quantity.input.SetTextWithoutNotify(count.ToString(CultureInfo.InvariantCulture));
            }
            RefreshDisplay();
        }

        private void SavePurity(double target)
        {
            if (!machine.IsTag(PurityTag) || machine.GetTagReadonly(PurityTag).GetDouble() != target)
                machine.ModifyTag(PurityTag, state => state.Enable().SetDouble(target));
        }

        private void SaveQuantity(int count)
        {
            if (machine.GetTagReadonly(QuantityTag).GetInt() != count)
                machine.ModifyTag(QuantityTag, state => state.Enable().SetInt(count));
        }

        private void RefreshDisplay()
        {
            displayDirty = true;
            Refresh();
        }

        private void Refresh()
        {
            if (executing || refreshLock)
                return;
            refreshLock = true;
            try
            {
                UpdateContents();
                var recipe = Recipe;
                displayDirty |= UpdatePlan(recipe);
                if (!displayDirty)
                    return;
                displayDirty = false;
                RenderControls(recipe);
            }
            finally
            {
                refreshLock = false;
            }
        }

        private void UpdateContents()
        {
            if (executing)
                return;
            if (!machine.GetTagReadonly(SpriteAnimationHelper.SPRITE_ANIMATION_PLAYING_BOOL).GetBool())
                SpriteAnimationHelper.SetSpriteAnimationPlaying(machine, true);
            bool hasContents = source.childItem != null && !WaterHelper.IsEmpty(source.childItem);
            if (hasContents == hadContents)
                return;
            hadContents = hasContents;
            machine.Validate();
        }

        private void RenderControls(ChemFinisherRecipe recipe)
        {
            bool layoutChanged = purity.handler.gameObject.activeSelf != editablePurity;
            purity.handler.gameObject.SetActive(editablePurity);
            fixedPurity.handler.gameObject.SetActive(!editablePurity);
            layoutChanged |= SetText(recipeLabel, recipe?.displayName ?? NativeText("no_target"));
            string maxText = plan == null ? "—" : Percent(plan.MaximumPurity);
            layoutChanged |= SetText(fixedPurity, maxText);
            layoutChanged |= SetText(maximumPurity, maxText + "%");

            int maxCount = plan?.MaximumCount ?? 0;
            int count = plan?.Products.Count ?? 0;
            if (!quantity.input.isFocused || int.TryParse(quantity.input.text, out int typed) && typed > maxCount)
                quantity.input.SetTextWithoutNotify(count.ToString(CultureInfo.InvariantCulture));
            layoutChanged |= SetText(maximumQuantity, maxCount.ToString(CultureInfo.InvariantCulture));

            if (count > 0)
            {
                layoutChanged |= SetText(consumption, Localize("consumption",
                    plan.SourceVolume / 1000.0, plan.AuxiliaryVolume / 1000.0));
            }
            else
            {
                layoutChanged |= SetText(consumption, "");
            }
            bool validQuantity = int.TryParse(quantity.input.text, out int requested) && requested > 0 && requested <= maxCount;
            produce.SetDisabled(!validQuantity || count == 0);
            if (layoutChanged)
                window.Validate();
        }

        private void InventoryChanged(GameInventory inventory)
        {
            if (inventory == output)
                outputCapacity = -1;
            Refresh();
        }

        private bool UpdatePlan(ChemFinisherRecipe recipe)
        {
            if (recipe == null || source.childItem == null || WaterHelper.IsEmpty(source.childItem) ||
                recipe.isRequireDry && WaterHelper.IsWet(source.childItem))
            {
                bool changed = plan != null || planKey.Inputs != recipe?.identifier;
                plan = null;
                editablePurity = false;
                planKey = (recipe?.identifier, 0, 0);
                return changed;
            }
            int[] sourceParts = WaterHelper.GetLiquidParts(source.childItem);
            int[] auxiliaryParts = auxiliary.childItem == null ? new int[Liquid.Liquids.Count] :
                WaterHelper.GetLiquidParts(auxiliary.childItem);
            int loss = MachineChemFinisher.GetLoss(machine);
            int capacity = OutputCapacity(recipe);
            bool auxiliaryWet = recipe.isRequireDry && auxiliary.childItem != null && WaterHelper.IsWet(auxiliary.childItem);
            if (auxiliaryWet)
                Array.Clear(auxiliaryParts, 0, auxiliaryParts.Length);
            bool canEdit = auxiliaryParts.Sum() > 0;
            displayDirty |= editablePurity != canEdit;
            editablePurity = canEdit;
            int active = Liquid.Liquids.FindIndex(liquid => liquid.identifier == recipe.activeIngredientId);
            double? target = editablePurity ? UpdatePurityOptions(
                ChemFactoryPlan.GetMaximumPurity(sourceParts, recipe.requiredVolume, active, loss)) : null;
            int quantity = machine.IsTag(QuantityTag) ? Math.Max(1, machine.GetTagReadonly(QuantityTag).GetInt()) : 1;
            var key = (Inputs: recipe.identifier + ":" + loss + ":" + capacity + ":" + auxiliaryWet +
                ":" + string.Join(",", sourceParts) + ":" + string.Join(",", auxiliaryParts), Target: target, Quantity: quantity);
            if (key == planKey)
                return false;
            plan = ChemFactoryPlan.Calculate(sourceParts, auxiliaryParts, recipe.requiredVolume,
                active,
                Liquid.Liquids.FindIndex(liquid => liquid.identifier == "physical_contaminant"),
                loss, target, capacity, quantity);
            if (plan.MaximumCount > 0)
            {
                quantity = plan.Products.Count;
                SaveQuantity(quantity);
            }
            planKey = (key.Inputs, target, quantity);
            return true;
        }

        private void Execute()
        {
            Refresh();
            if (executing || plan == null || !int.TryParse(quantity.input.text, out int count) ||
                count < 1 || count != plan.Products.Count)
                return;
            var recipe = Recipe;
            executing = true;
            configure.SetDisabled(true);
            produce.SetDisabled(true);
            purity.dropdown.interactable = quantity.input.interactable = false;
            source.LockInv();
            auxiliary.LockInv();
            modules.LockInv();
            OverlayHandler.current.StartCoroutine(Produce(recipe.identifier, plan));
        }

        private IEnumerator Produce(string recipeId, ChemFactoryPlan batch)
        {
            var sourceItem = source.childItem;
            var auxiliaryItem = auxiliary.childItem;
            int[] sourceParts = WaterHelper.GetLiquidParts(sourceItem);
            int[] auxiliaryParts = auxiliaryItem == null ? new int[Liquid.Liquids.Count] :
                WaterHelper.GetLiquidParts(auxiliaryItem);
            var products = new List<GameItem>();
            bool committed = false;
            try
            {
                double deadline = Time.realtimeSinceStartupAsDouble + 0.004;
                foreach (var portion in batch.Products)
                {
                    if (((GameItemElement)machine).IsDestroyed())
                        yield break;
                    var product = DirectoryMaster.Item(recipeId);
                    products.Add(product);
                    SetComposition(product, portion);
                    ChemicalProductHelper.GradeProduct(product);
                    if (Time.realtimeSinceStartupAsDouble >= deadline)
                    {
                        yield return null;
                        deadline = Time.realtimeSinceStartupAsDouble + 0.004;
                    }
                }
                if (((GameItemElement)machine).IsDestroyed() ||
                    !sourceParts.SequenceEqual(WaterHelper.GetLiquidParts(sourceItem)) ||
                    auxiliaryItem != null && !auxiliaryParts.SequenceEqual(WaterHelper.GetLiquidParts(auxiliaryItem)))
                    yield break;
                foreach (var product in products)
                {
                    var slot = output.TryFindOneValidInventorySlot(product);
                    if (slot == null || slot.TryAcceptOnce(1) != 1)
                        yield break;
                }
                Consume(sourceItem, sourceParts, batch.SourceParts);
                if (batch.AuxiliaryVolume > 0)
                    Consume(auxiliaryItem, auxiliaryParts, batch.AuxiliaryParts);
                committed = true;
                foreach (var product in products)
                    AchievementTracker.Instance.CheckLabPure(product);
            }
            finally
            {
                if (!committed)
                    foreach (var product in products)
                        product.Destroy();
                source.UnlockInv();
                auxiliary.UnlockInv();
                modules.UnlockInv();
                executing = false;
                outputCapacity = -1;
                planKey = default;
                if (!((GameItemElement)machine).IsDestroyed())
                {
                    configure.SetDisabled(false);
                    purity.dropdown.interactable = quantity.input.interactable = true;
                    Refresh();
                }
            }
        }

        private static void Consume(GameItem item, int[] remaining, int[] used)
        {
            if (!used.Any(amount => amount > 0))
                return;
            for (int i = 0; i < remaining.Length; i++)
                remaining[i] -= used[i];
            SetComposition(item, remaining);
            WaterFeatureHelper.OnLiquidContainerPourOutOf(item);
            WaterHelper.UpdateWaterBottleSound(item);
        }

        private static void SetComposition(GameItem item, int[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                int amount = parts[i];
                item.ModifyTag(Liquid.Liquids[i].currentPart,
                    state => state.Enable().SetInt(amount), skipValidate: true);
            }
            item.ValidateShapeState();
        }

        private int OutputCapacity(ChemFinisherRecipe recipe)
        {
            if (shapeRecipe != recipe.identifier)
            {
                var product = DirectoryMaster.Item(recipe.identifier);
                productShape = product.shape.Clone();
                product.Destroy();
                shapeRecipe = recipe.identifier;
                outputCapacity = -1;
            }
            if (outputCapacity < 0)
                outputCapacity = ChemFactoryOutput.Capacity(output, productShape);
            return outputCapacity;
        }

        private List<ItemSpriteModifier> GetContentsSprites(GameItem item)
        {
            if (source.childItem == null || WaterHelper.IsEmpty(source.childItem))
                return null;
            return new List<ItemSpriteModifier>
            {
                new(RefinedChemistry.GetSprite(ItemId + "_b"), Vector2.zero)
            };
        }

        private static GameSlotInventory CreateSlot()
        {
            var slot = new GameSlotInventory();
            slot.mayInventoryAddItemFunc = (item, _) =>
                GeneralHelper.IsItemOwned(item) && item.IsTag("LIQUID_CONTAINER_TAG");
            slot.slotPriorityFunc = (_, _, _) => 5;
            slot.SetBackgroundFadeSprite("Items/water_container4", "xl");
            return slot;
        }

        private static string Percent(double value) =>
            (Math.Floor(value * 1000) / 1000).ToString("0.###", CultureInfo.InvariantCulture);

        private static MachineryText Label(string text = null) =>
            new MachineryText(text, uppercase: false).SetFillWidth(false);

        private static bool SetText(MachineryText label, string text)
        {
            if (label.GetText() == text)
                return false;
            label.SetText(text);
            return true;
        }

        private static string NativeText(string key) =>
            LocHelper.GetLocalizedMechanic("mech_chem_finisher_" + key);
    }
}
