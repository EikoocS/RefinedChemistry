using RefinedChemistry.Attributes;
using System.Collections.Generic;
using HarmonyLib;
using RefinedChemistry.Item;
using UnityEngine;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(ItemSelectHandler))]
    [LocalizableAttributes("item", LabelerFactory.ItemId)]
    internal static partial class LabelerFactoryUsePatch
    {
        private static GameItem lastSource;
        private static string lastGrade;

        [HarmonyPrefix, HarmonyPatch("OnEventPress")]
        private static bool PressPrefix(ItemSelectHandler __instance, GameItem ___currentItem,
            ISet<KeyCode> keysPressed)
        {
            if (___currentItem?.identifier != LabelerFactory.ItemId)
                return true;
            if (keysPressed.Contains(KeyCode.Mouse1))
            {
                __instance.EndSelect();
                return false;
            }
            if (!keysPressed.Contains(__instance.key))
                return true;
            ItemMouseDoubleClickHandler.current?.ConsumePress();
            return false;
        }

        [HarmonyPostfix, HarmonyPatch("OnEventUpdate")]
        private static void UpdatePostfix(ItemSelectHandler __instance, GameItem ___currentItem,
            GameItem ___lastItem, RichTextBuilder ___builder, bool __result)
        {
            if (!__result || ___currentItem?.identifier != LabelerFactory.ItemId ||
                (StoreUIManager.Instance != null && StoreUIManager.Instance.hoveringStoreUI))
                return;

            var source = ___currentItem;
            string grade = LabelerFactory.GetGrade(source).identifier;
            bool refresh = source != lastSource || grade != lastGrade;
            lastSource = source;
            lastGrade = grade;

            if (Input.GetMouseButton(0) || Input.GetKey(__instance.key))
            {
                var target = RenderHandler.RaycastElement<GameItem>(Input.mousePosition);
                if (target != null && target != source && source.MayTarget(target) && source.CanTarget(target))
                {
                    ItemBehaviourManager.current.Target(source, target);
                    if (source.MayTarget(target) && source.CanTarget(target))
                    {
                        source.Target(target);
                        refresh = true;
                    }
                }
            }

            if (refresh)
                ModReflect.Call(__instance, "RefreshHighlights", source);

            if (___lastItem != null)
            {
                var feature = ChemicalProductHelper.GetChemicalFeature(___lastItem);
                if (feature == null || !LabelerHelper.IsEditableLabelFeature(feature))
                    return;
            }

            var builder = new RichTextBuilder(60);
            builder.AddLine(LabelerFactory.GetSelectionText(source));
            if (___lastItem != null)
            {
                if (LabelerFactory.HasSelectedLabel(source, ___lastItem))
                    builder.AddLine(Localize("same_label"),
                        ignoreEmpty: true, RenderHandler.ColorPalette.LightGray);
                else
                    builder.Add(___builder);
            }
            OverlayHandler.current.ShowTooltip(Input.mousePosition, __instance.tooltipTranslation, builder.Build());
        }
    }
}
