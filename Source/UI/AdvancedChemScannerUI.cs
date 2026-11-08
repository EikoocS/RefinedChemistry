using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RefinedChemistry.Item;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RefinedChemistry.UI
{
    internal sealed class AdvancedChemScannerUI : MonoBehaviour
    {
        private static readonly HashSet<string> primaryIngredients = new()
        {
            "dream_cap_mush",
            "dream_cap_extract",
            "caffeine",
            "coffee_mush",
            "nightmare_extract",
            "nightmare_cap_mush",
            "fanta",
            "methylol",
            "nomore"
        };

        private static readonly HashSet<string> impurities = new()
        {
            "heavy_metal",
            "organic_waste",
            "microplastic",
            "microbe",
            "chemical_contaminant",
            "physical_contaminant",
            "plant_matter",
            "protein",
            "debris"
        };

        private const float Scale = 2;
        private GameItem target;
        private TextMeshProUGUI itemName;
        private TextMeshProUGUI result;
        private TextMeshProUGUI label;
        private TextMeshProUGUI details;
        private Button matchButton;
        private Button confrontButton;
        private ChemScannerEffect euphoriaEffect;
        private ChemScannerEffect toxicityEffect;

        private bool IsChemical => target.IsTag("CHEM_ITEM_TAG") || target.IsTag("CHEMICAL_PRODUCT_TAG");

        internal static void CreateTooltip(RichTextBuilder builder, GameItem item)
        {
            if (!item.IsTag("LIQUID_CONTAINER_TAG"))
                return;
            if (!EmporiumEntry.Instance.GetAllItems().Any(scanner => scanner.identifier == AdvancedChemScanner.ItemId &&
                GeneralHelper.IsItemOwned(scanner) && scanner.IsTag(GeneralHelper.IS_ITEM_TOGGLE_ON_TAG)))
                return;

            var parts = GetParts(item);
            long total = parts.Sum(part => (long)part.Amount);
            foreach (string line in GetScanResult(item, parts, total).Split('\n'))
                builder.AddLine(line);
        }

        internal static void Show(GameItem target)
        {
            var texture = RefinedChemistry.getLoadedMod().Textures["advanced_chem_scanner_background"];
            texture.filterMode = FilterMode.Point;
            var background = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), 100);
            var prefab = new GameObject("AdvancedChemScanner", typeof(RectTransform),
                typeof(Image), typeof(CustomUIWindowFrame));
            prefab.SetActive(false);
            prefab.GetComponent<Image>().sprite = background;
            prefab.GetComponent<CustomUIWindowFrame>().contentRoot = (RectTransform)prefab.transform;

            var builder = CustomUIManager.Instance.CreateWindow("refined_chemistry.advanced_chem_scanner",
                layerId: CustomUIManager.LAYER_PAPER, windowPrefabOverride: prefab)
                .SetSize(texture.width * Scale, texture.height * Scale)
                .SetCloseOnEscape()
                .SetDraggable(true)
                .Center();
            Object.Destroy(prefab);
            var window = builder.Handle;
            builder.OnClose(() =>
            {
                Object.Destroy(background);
                EmporiumAudioPlayer.Instance.audioButton4.Play();
            });

            var view = window.root.AddComponent<AdvancedChemScannerUI>();
            view.target = target;
            view.itemName = AddLabel(builder, 58, 26, TextAlignmentOptions.Center);
            view.result = AddLabel(builder, 87, 26, TextAlignmentOptions.Bottom);
            view.result.margin = new Vector4(0, 0, 0, 2);
            view.label = AddLabel(builder, 114, 17, TextAlignmentOptions.Center);
            view.label.fontSize = 12;
            view.details = AddLabel(builder, 135, 64, TextAlignmentOptions.TopLeft);
            view.details.margin = new Vector4(6 * Scale, 0, 6 * Scale, 0);
            view.details.richText = true;
            view.details.fontSizeMin = 3;
            view.details.fontSizeMax = 14;
            view.details.enableAutoSizing = true;
            view.euphoriaEffect = CreateEffect(ChemScannerUIManager.Instance.euphoriaEffect, window.Rect, 35.4f);
            view.toxicityEffect = CreateEffect(ChemScannerUIManager.Instance.toxicityEffect, window.Rect, 104.3f);
            view.matchButton = CreateActionButton(ChemScannerUIManager.Instance.matchButton, window.Rect,
                view.MatchLabel);
            view.confrontButton = CreateActionButton(ChemScannerUIManager.Instance.confrontButton, window.Rect,
                view.Confront);

            builder.AddCloseButton(window.Close);
            Place(builder.Last.Rect, 175, 8, 16, 16);
            window.Show();
            view.StartCoroutine(view.RefreshWhileOpen());
        }

        private IEnumerator RefreshWhileOpen()
        {
            while (gameObject.activeSelf)
            {
                Refresh();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void Refresh()
        {
            var parts = GetParts(target);
            long total = parts.Sum(part => (long)part.Amount);
            itemName.text = target.name;
            result.text = GetScanResult(target, parts, total);
            details.text = GetScanDetails(parts, total);
            RefreshLabelActions();
            RefreshEffects();
        }

        private void RefreshLabelActions()
        {
            if (IsChemical)
            {
                label.text = target.IsTag("CHEMICAL_PRODUCT_TAG")
                    ? LocHelper.GetLocalizedMechanic("mech_tooltip_labeled") + "\n" +
                        ChemicalProductHelper.GetChemicalFeature(target).fakeCondition.display
                    : string.Empty;
            }
            else
            {
                label.text = WaterHelper.IsEmpty(target)
                    ? string.Empty : WaterScannerUIManager.GetLabeledString(target);
            }

            bool canMatch = IsChemical ? ChemScannerUIManager.Instance.CanMatchChem(target)
                : WaterScannerUIManager.CanMatch(target);
            bool canConfront = IsChemical ? ChemScannerUIManager.CanConfrontChem(target)
                : WaterScannerUIManager.CanConfront(target);
            matchButton.gameObject.SetActive(canMatch);
            confrontButton.gameObject.SetActive(canConfront);
            bool both = canMatch && canConfront;
            Place((RectTransform)matchButton.transform, both ? 44 : 69, 265, both ? 60 : 73, 19);
            Place((RectTransform)confrontButton.transform, both ? 107 : 69, 265, both ? 60 : 73, 19);
        }

        private void RefreshEffects()
        {
            string euphoriaKey = ChemicalFeedbackHelper.GetEuphoriaLevel(target) switch
            {
                1 => "mech_scanner_euphoria_weak",
                2 => "mech_scanner_euphoria_mild",
                3 => "mech_scanner_euphoria_strong",
                4 => "mech_scanner_euphoria_extreme",
                _ => null
            };
            euphoriaEffect.gameObject.SetActive(euphoriaKey != null);
            if (euphoriaKey != null)
                euphoriaEffect.effectTMP.text = LocHelper.GetLocalizedMechanic(euphoriaKey);

            bool lethal = ChemicalFeedbackHelper.IsLethal(target);
            bool toxic = lethal || ChemicalFeedbackHelper.IsToxic(target);
            toxicityEffect.gameObject.SetActive(toxic);
            if (lethal)
                toxicityEffect.effectTMP.text = LocHelper.GetLocalizedMechanic("mech_scanner_toxicity_lethal") +
                    " (x" + ChemicalFeedbackHelper.GetLethalDose(target) + ")";
            else if (toxic)
                toxicityEffect.effectTMP.text = LocHelper.GetLocalizedMechanic("mech_scanner_toxicity_toxic");
        }

        private void MatchLabel()
        {
            if (target.IsTag("LIQUID_CONTAINER_TAG") && WaterScannerUIManager.CanMatch(target))
                WaterScannerUIManager.MatchWaterLabel(target, matchButton, label, result);
            else if (IsChemical && ChemScannerUIManager.Instance.CanMatchChem(target))
                ChemScannerUIManager.MatchChemLabel(target, matchButton, label, result);
            Refresh();
        }

        private void Confront()
        {
            if (InspectionUIManager.IsConfrontLocked())
                return;
            if (target.IsTag("LIQUID_CONTAINER_TAG") && WaterScannerUIManager.CanConfront(target))
                WaterScannerUIManager.ConfrontWater(target, confrontButton);
            else if (IsChemical && ChemScannerUIManager.CanConfrontChem(target))
                ChemScannerUIManager.ConFrontChem(target, confrontButton);
            Refresh();
        }

        private static List<(Liquid Liquid, int Amount, int Category)> GetParts(GameItem target) =>
            Liquid.Liquids.Select(liquid => (
                Liquid: liquid,
                Amount: target.GetTagReadonly(liquid.currentPart).GetInt(),
                Category: primaryIngredients.Contains(liquid.identifier) ? 0 :
                    impurities.Contains(liquid.identifier) ? 1 : 2))
                .Where(part => part.Amount > 0).ToList();

        private static string GetScanResult(GameItem target,
            List<(Liquid Liquid, int Amount, int Category)> parts, long total)
        {
            if (parts.Count == 0)
                return string.Empty;

            string activeIngredient = target.IsTag(ChemicalProductHelper.ACTIVATE_INGREDIENT_TAG)
                ? target.GetTagReadonly(ChemicalProductHelper.ACTIVATE_INGREDIENT_TAG).GetString()
                : null;
            var selected = parts.OrderByDescending(part => part.Liquid.identifier == activeIngredient)
                .ThenByDescending(part => part.Category == 0)
                .ThenByDescending(part => part.Amount).First();
            string detailLine = !target.IsTag("LIQUID_CONTAINER_TAG") && target.IsTag("CHEMICAL_PRODUCT_TAG")
                ? ChemicalProductHelper.GetChemicalFeature(target).realCondition.display
                : LocalizationHelper.Format("ui.advanced_chem_scanner.active_ingredient", FormatAmount(selected.Amount));
            return $"{selected.Amount * 100.0 / total:0.##}% {selected.Liquid.displayName}\n{detailLine}";
        }

        private static string GetScanDetails(List<(Liquid Liquid, int Amount, int Category)> parts, long total)
        {
            var orderedParts = parts.Any(part => part.Category == 0)
                ? parts.OrderBy(part => part.Category)
                : parts.OrderByDescending(part => part.Category);
            return string.Join("\n", orderedParts
                .ThenByDescending(part => part.Amount).Select(part =>
                {
                    string line = $"{part.Liquid.displayName}: {FormatAmount(part.Amount)} ({part.Amount * 100.0 / total:F2}%)";
                    return part.Category switch
                    {
                        0 => $"<color=#62A7D7>{line}</color>",
                        1 => $"<color=#DDC99B>{line}</color>",
                        _ => line
                    };
                }));
        }

        private static string FormatAmount(int amount) =>
            amount >= 1000 ? $"{amount / 1000.0:0.###}ml" : $"{amount}ul";

        private static Button CreateActionButton(Button template, RectTransform parent,
            UnityEngine.Events.UnityAction onClick)
        {
            var button = Object.Instantiate(template, parent);
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(onClick);
            button.gameObject.SetActive(false);
            return button;
        }

        private static ChemScannerEffect CreateEffect(ChemScannerEffect template, RectTransform parent, float x)
        {
            var effect = Object.Instantiate(template, parent);
            var rect = (RectTransform)effect.transform;
            Place(rect, x, 219.5f, template.GetComponent<RectTransform>().rect.width / Scale,
                template.GetComponent<RectTransform>().rect.height / Scale);
            effect.gameObject.SetActive(false);
            return effect;
        }

        private static TextMeshProUGUI AddLabel(CustomUIBuilder builder, float y, float height,
            TextAlignmentOptions alignment)
        {
            builder.AddLabel(string.Empty);
            var label = builder.Last;
            Place(label.Rect, 34, y, 143, height);
            label.text.fontSize = 16;
            label.text.enableAutoSizing = false;
            label.text.enableWordWrapping = false;
            label.text.alignment = alignment;
            label.text.color = new Color32(202, 216, 223, 255);
            label.text.raycastTarget = false;
            return label.text;
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y) * Scale;
            rect.sizeDelta = new Vector2(width, height) * Scale;
        }
    }
}
