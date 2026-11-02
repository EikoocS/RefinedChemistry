using HarmonyLib;
using UnityEngine;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(ItemSelectHandler), "OnEventUpdate")]
    internal static class LiquidContainerScrollPatch
    {
        private const float ContinuousScrollInterval = 0.3f;
        private static readonly int[] BeakerSteps = { 1, 2, 5, 10 };
        private static GameItem lastScrolledItem;
        private static float lastScrollTime;
        private static int lastScrollDirection;
        private static int stepIndex;

        private static void Postfix(ItemSelectHandler __instance, GameItem ___currentItem,
            GameItem ___lastItem, RichTextBuilder ___builder, bool __result)
        {
            bool isBeaker = ___currentItem?.identifier == Beaker.ItemId;
            if (!__result || (!isBeaker && !Pipette.IsPipette(___currentItem)) ||
                (StoreUIManager.Instance != null && StoreUIManager.Instance.hoveringStoreUI))
            {
                lastScrolledItem = null;
                return;
            }

            if (!isBeaker)
            {
                lastScrolledItem = null;
            }
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0)
            {
                int direction = scroll > 0 ? 1 : -1;
                int step = isBeaker ? GetBeakerStep(___currentItem, direction) : 1;
                LiquidContainerItemHelper.AdjustCapacity(___currentItem, direction * step,
                    isBeaker ? 10 : 1, isBeaker ? 100 : 10);
                if (isBeaker && __instance.IsEquipped)
                {
                    Traverse.Create(__instance).Method("ShowEquipCursor", ___currentItem).GetValue();
                }
            }

            var builder = new RichTextBuilder(60);
            builder.AddLine(isBeaker
                ? Beaker.GetCapacityText(___currentItem)
                : Pipette.GetCapacityText(___currentItem));
            if (___lastItem != null)
            {
                builder.Add(___builder);
            }
            OverlayHandler.current.ShowTooltip(Input.mousePosition, __instance.tooltipTranslation, builder.Build());
        }

        private static int GetBeakerStep(GameItem item, int direction)
        {
            float now = Time.unscaledTime;
            bool continuous = lastScrolledItem == item && lastScrollDirection == direction &&
                now - lastScrollTime <= ContinuousScrollInterval;
            stepIndex = continuous ? Mathf.Min(stepIndex + 1, BeakerSteps.Length - 1) : 0;
            lastScrolledItem = item;
            lastScrollDirection = direction;
            lastScrollTime = now;
            return BeakerSteps[stepIndex];
        }
    }

    [HarmonyPatch(typeof(SpriteHelper), nameof(SpriteHelper.UpdateWaterContainerSprite))]
    internal static class LiquidContainerSpritePatch
    {
        private static void Postfix(GameItem gameItem)
        {
            if (Pipette.IsPipette(gameItem) || gameItem.identifier == Beaker.ItemId ||
                gameItem.identifier == Flask.ItemId)
            {
                ((GameItemElement)gameItem).HandleInnateModSprite();
            }
        }
    }
}
