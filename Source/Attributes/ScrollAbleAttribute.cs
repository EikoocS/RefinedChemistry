using System;

namespace RefinedChemistry.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class ScrollAbleAttribute : Attribute
    {
        public string ItemId { get; }
        public string MethodName { get; }
        public string TooltipMethodName { get; }

        public ScrollAbleAttribute(string itemId, string methodName, string tooltipMethodName)
        {
            ItemId = itemId;
            MethodName = methodName;
            TooltipMethodName = tooltipMethodName;
        }
    }
}
