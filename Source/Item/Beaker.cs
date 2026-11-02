using UnityEngine;

namespace RefinedChemistry.Item
{
    internal static class Beaker
    {
        internal const string ItemId = "beaker";

        internal static GameItem Create()
        {
            var item = LiquidContainerItemHelper.Create(ItemId, "beaker", 100, 150);
            item.SetShape(new GridShapeBuilder().SetData([ 1, 1, 1, 1 ], 2));
            AudioHelper.InitGlass(item);
            return item;
        }

        internal static string GetCapacityText(GameItem item)
        {
            float capacityMl = WaterHelper.GetTotalCapacityML(item) / 1000f;
            float totalMl = WaterHelper.GetTotalVolume(item) / 1000f;
            float freeMl = Mathf.Max(0, WaterHelper.GetFreeCapacity(item)) / 1000f;
            string text = LocalizationHelper.Format("beaker_capacity", capacityMl, totalMl, freeMl);
            return text;
        }
    }
}
