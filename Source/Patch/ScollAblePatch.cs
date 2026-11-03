using HarmonyLib;
using UnityEngine;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(ItemSelectHandler), "OnEventUpdate")]
    internal static class ScollAblePatch
    {
        private static readonly int[] ScrollSteps = [1,1,1,3,4,5,10,50];
        private static GameItem lastItem;
        private static float lastScrollTime;
        private static int lastDirection;
        private static int stepIndex;

        private static void Postfix(ItemSelectHandler __instance, GameItem ___currentItem,
            GameItem ___lastItem, RichTextBuilder ___builder, bool __result)
        {
            var currentItemId = ___currentItem?.identifier;
            if (!__result 
                || !ItemHelper.isScrollAble(currentItemId) 
                || (StoreUIManager.Instance != null && StoreUIManager.Instance.hoveringStoreUI))
            {
                lastItem = null;
                return;
            }

            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                float now = Time.unscaledTime;
                int direction = scroll > 0 ? 1 : -1;
                bool continuous = ReferenceEquals(___currentItem, lastItem) &&
                    direction == lastDirection &&
                    now - lastScrollTime <= 0.3f;
                stepIndex = continuous
                    ? Mathf.Min(stepIndex + 1, ScrollSteps.Length - 1)
                    : 0;
                lastItem = ___currentItem;
                lastScrollTime = now;
                lastDirection = direction;

                var handler = ItemHelper.getScrollHandler(currentItemId);
                handler(___currentItem, direction * ScrollSteps[stepIndex]);
                if (__instance.IsEquipped)
                {
                    Traverse.Create(__instance).Method("ShowEquipCursor", ___currentItem).GetValue();
                }
            }

            var builder = new RichTextBuilder(60);
            builder.AddLine(ItemHelper.getScrollTooltip(___currentItem));
            if (___lastItem != null)
            {
                builder.Add(___builder);
            }
            OverlayHandler.current.ShowTooltip(
                Input.mousePosition, __instance.tooltipTranslation, builder.Build());
        }
    }
}
