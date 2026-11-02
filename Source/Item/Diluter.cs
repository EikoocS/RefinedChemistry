using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace RefinedChemistry.Item
{
    internal sealed class Diluter
    {
        internal const string ItemId = "diluter";
        private const string SelectedTag = "DILUTER_SELECTED_LIQUID";
        private const string TargetTag = "DILUTER_TARGET_UL_PER_ML";

        private readonly GameSlotInventory source = CreateSlot();
        private readonly GameSlotInventory solvent = CreateSlot();
        private readonly GameSlotInventory output = CreateSlot();
        private readonly List<Liquid> options = new List<Liquid>();
        private readonly MachineryText preview = new MachineryText(uppercase: false);
        private readonly MachineryText warning = new MachineryText(uppercase: false)
            .SetColor(new Color(1f, 0.75f, 0.25f));
        private readonly MachineryButton confirm = new MachineryButton(Text("confirm"));
        private readonly DiluterControl selection;
        private readonly DiluterControl amount;
        private readonly GameItem machine;
        private readonly PixelWindow window;
        private DilutionRecipe displayedRecipe;

        internal static GameItem Create() => new Diluter().machine;

        private Diluter()
        {
            selection = new DiluterControl(CustomUIManager.ElementKind.Dropdown);
            amount = new DiluterControl(CustomUIManager.ElementKind.Input);
            amount.input.contentType = TMP_InputField.ContentType.IntegerNumber;
            amount.input.characterLimit = 4;
            amount.input.SetTextWithoutNotify("1");

            var inputs = MachineryStack.Vertical(
                MachineHelper.Panel(Text("source"), source, "source"),
                MachineHelper.Panel(Text("solvent"), solvent, "solvent"));
            var controls = MachineryStack.Vertical(
                new MachineryText(Text("substance"), uppercase: false), selection,
                new MachineryText(Text("target"), uppercase: false), amount, preview, warning, confirm);
            window = MachineHelper.CreateMachineryWindow(true,
                new MachineryPanel(Text("inputs"), inputs, "inputs"),
                new MachineryPanel(Text("control"), controls, "control"),
                MachineHelper.Panel(Text("output"), output, "output"));
            machine = ItemDirectory.CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(ModEntry.Instance.Manifest.ID, ItemId)
                .SetContentWindow(window);
            machine.name = Text("name");
            machine.shortDescription = Text("desc");
            machine.unitValue = 300;
            machine.SetGameItemType("MACHINE");
            machine.EnableTag("CONTAINER_TAG");
            AudioHelper.InitMetalMachine(machine);

            selection.dropdown.onValueChanged.AddListener(index =>
            {
                if (index >= 0 && index < options.Count)
                    machine.ModifyTag(SelectedTag, state => state.Enable().SetString(options[index].identifier));
                Refresh();
            });
            amount.input.onValueChanged.AddListener(value =>
            {
                int.TryParse(value, out int target);
                machine.ModifyTag(TargetTag, state => state.Enable().SetInt(target));
                Refresh();
            });
            confirm.SetCallback(Execute);
            foreach (var slot in new[] { source, solvent, output })
            {
                slot.onSlotAddItemEndFunc += (_, _, _) => Refresh();
                slot.onSlotRemoveItemEndFunc += (_, _, _) => Refresh();
            }
            window.handler.gameObject.AddComponent<DiluterRefresh>().refresh = Refresh;
            Refresh();
        }

        private void Refresh()
        {
            var available = new List<Liquid>();
            foreach (Liquid liquid in Liquid.Liquids)
                if (source.childItem != null && source.childItem.GetTagReadonly(liquid.currentPart).GetInt() > 0)
                    available.Add(liquid);
            bool changed = available.Count != options.Count;
            for (int i = 0; !changed && i < available.Count; i++)
                changed = available[i] != options[i];
            string selected = machine.GetTagReadonly(SelectedTag).GetString();
            int index = available.FindIndex(liquid => liquid.identifier == selected);
            if (index < 0 && available.Count > 0)
            {
                index = 0;
                machine.ModifyTag(SelectedTag, state => state.Enable().SetString(available[0].identifier));
            }
            if (changed || selection.dropdown.options.Count == 0)
            {
                selection.dropdown.Hide();
                options.Clear();
                options.AddRange(available);
                var captions = new List<string>();
                foreach (Liquid liquid in options)
                    captions.Add(liquid.displayName);
                if (captions.Count == 0)
                    captions.Add(Text("select"));
                selection.dropdown.ClearOptions();
                selection.dropdown.AddOptions(captions);
            }
            selection.dropdown.interactable = options.Count > 0;
            selection.dropdown.SetValueWithoutNotify(Math.Max(0, index));
            selection.dropdown.RefreshShownValue();
            if (!amount.input.isFocused)
            {
                int target = machine.IsTag(TargetTag) ? machine.GetTagReadonly(TargetTag).GetInt() : 1;
                amount.input.SetTextWithoutNotify(target.ToString());
            }

            string error = Calculate(out var recipe);
            displayedRecipe = recipe;
            string text = error == null ? Text("preview", recipe.SourceVolume / 1000.0,
                recipe.SolventVolume / 1000.0, recipe.TotalVolume / 1000.0) + "\n" +
                Text(recipe.IsExact ? "actual" : "nearest",
                    recipe.ActualConcentration.ToString("0.############")) : Text(error);
            bool approximate = error == null && !recipe.IsExact;
            if (output.childItem != null && !WaterHelper.IsEmpty(output.childItem))
            {
                var liquid = Liquid.Liquids.Find(value => value.identifier ==
                    machine.GetTagReadonly(SelectedTag).GetString());
                if (liquid != null)
                {
                    int volume = WaterHelper.GetTotalVolume(output.childItem);
                    int selectedVolume = output.childItem.GetTagReadonly(liquid.currentPart).GetInt();
                    decimal concentration = 1000m * selectedVolume / volume;
                    text += "\n" + Text("actual", concentration.ToString("0.############"));
                    if (int.TryParse(amount.input.text, out int target))
                        approximate = 1000L * selectedVolume != (long)target * volume;
                }
            }
            if (preview.GetText() != text)
                preview.SetText(text);
            string warningText = approximate ? Text("approximate") : "";
            if (warning.GetText() != warningText)
                warning.SetText(warningText);
            string caption = Text(approximate ? "confirm_approximate" : "confirm");
            if (confirm.text != caption)
                confirm.SetText(caption);
            confirm.SetDisabled(error != null);
            window.Validate();
        }

        private string Calculate(out DilutionRecipe recipe)
        {
            recipe = null;
            if (!int.TryParse(amount.input.text, out int target) || target < 1 || target > 1000)
                return "range";
            if (source.childItem == null || WaterHelper.IsEmpty(source.childItem))
                return "source_empty";
            if (output.childItem == null)
                return "output_missing";
            if (!WaterHelper.IsEmpty(output.childItem))
                return "output_not_empty";
            if (WaterHelper.GetFilterItem(output.childItem) != null)
                return "output_filter";
            string selected = machine.GetTagReadonly(SelectedTag).GetString();
            int index = Liquid.Liquids.FindIndex(liquid => liquid.identifier == selected);
            return DilutionRecipe.Calculate(Parts(source.childItem), Parts(solvent.childItem), index,
                target, WaterHelper.GetFreeCapacity(output.childItem), WaterHelper.SplitProportionally, out recipe);
        }

        private void Execute()
        {
            if (Calculate(out var recipe) != null || displayedRecipe == null ||
                recipe.TargetConcentration != displayedRecipe.TargetConcentration ||
                !recipe.SourceParts.SequenceEqual(displayedRecipe.SourceParts) ||
                !recipe.SolventParts.SequenceEqual(displayedRecipe.SolventParts))
            {
                Refresh();
                return;
            }
            Transfer(source.childItem, output.childItem, recipe.SourceParts);
            if (recipe.SolventVolume > 0)
                Transfer(solvent.childItem, output.childItem, recipe.SolventParts);
            AudioHelper.PourWater();
            Refresh();
        }

        private static void Transfer(GameItem from, GameItem to, int[] parts)
        {
            bool wasEmpty = WaterHelper.IsEmpty(to);
            for (int i = 0; i < parts.Length; i++)
            {
                int volume = parts[i];
                if (volume == 0)
                    continue;
                string tag = Liquid.Liquids[i].currentPart;
                int remaining = from.GetTagReadonly(tag).GetInt() - volume;
                int combined = to.GetTagReadonly(tag).GetInt() + volume;
                ChemicalHelper.HandleReceiveLiquidTemp(to, volume, from);
                to.ModifyTag(tag, state => state.Enable().SetInt(combined), skipValidate: true);
                from.ModifyTag(tag, state => state.Enable().SetInt(remaining), skipValidate: true);
            }
            WaterFeatureHelper.OnLiquidContainerPourInto(to, from, wasEmpty);
            WaterFeatureHelper.OnLiquidContainerPourOutOf(from);
            WaterHelper.UpdateWaterBottleSound(from);
            WaterHelper.UpdateWaterBottleSound(to);
        }

        private static int[] Parts(GameItem container)
        {
            var parts = new int[Liquid.Liquids.Count];
            if (container != null)
                for (int i = 0; i < parts.Length; i++)
                    parts[i] = container.GetTagReadonly(Liquid.Liquids[i].currentPart).GetInt();
            return parts;
        }

        private static GameSlotInventory CreateSlot()
        {
            var slot = new GameSlotInventory();
            slot.mayInventoryAddItemFunc = (item, _) =>
                GeneralHelper.IsItemOwned(item) && item.IsTag("LIQUID_CONTAINER_TAG");
            slot.SetBackgroundFadeSprite("Items/water_container4", "xl");
            return slot;
        }

        private static string Text(string key, params object[] args) =>
            LocalizationHelper.Format("diluter_" + key, args);
    }

    internal sealed class DiluterRefresh : MonoBehaviour
    {
        internal Action refresh;
        private float nextRefresh;

        private void OnEnable() => refresh?.Invoke();

        private void Update()
        {
            if (Time.unscaledTime < nextRefresh)
                return;
            nextRefresh = Time.unscaledTime + 0.25f;
            refresh?.Invoke();
        }
    }
}
