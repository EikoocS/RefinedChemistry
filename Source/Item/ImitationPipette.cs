using System;

namespace RefinedChemistry.Item
{
    [WaterContainer(ItemId)]
    [ScrollAble(ItemId, nameof(OnScroll), nameof(GetCapacityText))]
    internal class ImitationPipette : Pipette
    {
        new internal const string ItemId = "imitation_pipette";
        protected override string Id => ItemId;
        internal override int basePrice => 100;
        internal override GameItem Create()
        {
            var item = base.Create();

            DurabilityHelper.InitItemWithDurability(item, 100);
            var durability = item.FindItemFeatureByID(DurabilityHelper.DURABILITY_TAG);
            durability.SetFeatureDiscovered(true);
            durability.SetExposed(true);

            item.canThisTargetItemFunc = CanThisTargetItemFunc(item.canThisTargetItemFunc);
            item.canItemTargetThisFunc = CanItemTargetThisFunc;
            item.onThisTargetItemFunc = OnThisTargetItemFunc(item.onThisTargetItemFunc);
            return item;
        }

        new internal static string GetCapacityText(GameItem item)
        {
            string text = Pipette.GetCapacityText(item);
            text += "\n" + DurabilityHelper.GetRealDisplayString(item);
            return text;
        }

        private Func<GameItem, GameItem, bool> CanThisTargetItemFunc(Func<GameItem, GameItem, bool> superCanTarget)
        {
            return (source, target) => superCanTarget(source, target) &&
                (DurabilityHelper.GetRealDurability(source) > 0 ||
                    (!WaterHelper.IsEmpty(source) && !WaterHelper.IsFaucet(target)));
        }

        private bool CanItemTargetThisFunc(GameItem source,GameItem target)
        {
            return DurabilityHelper.GetRealDurability(target) > 0 ||
                !source.IsTag(WaterHelper.LIQUID_CONTAINER_TAG) ||
                (source.IsTag(WaterHelper.LIQUID_CONTAINER_DROPPER_TAG) && WaterHelper.IsEmpty(source));
        }

        private Action<GameItem, GameItem> OnThisTargetItemFunc(Action<GameItem, GameItem> superUse)
        {
            return (source, target) =>
            {
                int volume = WaterHelper.GetTotalVolume(source);
                superUse(source, target);
                if (WaterHelper.GetTotalVolume(source) < volume)
                {
                    DurabilityHelper.ChangeDurability(source, -1);
                }
            };
        }
    }
}
