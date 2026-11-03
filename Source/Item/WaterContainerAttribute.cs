using System;

namespace RefinedChemistry.Item
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class WaterContainerAttribute : Attribute
    {
        public string ItemId { get; }
        public WaterContainerAttribute(string itemId) => ItemId = itemId;
    }
}
