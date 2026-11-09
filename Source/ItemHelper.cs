using RefinedChemistry.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RefinedChemistry
{
    internal static class ItemHelper
    {
        private static readonly HashSet<string> waterContainer= new HashSet<string>();
        private static readonly Dictionary<string, Action<GameItem, int>> scrollHandlers = new Dictionary<string, Action<GameItem, int>>();
        private static readonly Dictionary<string, Func<GameItem, string>> scrollTooltips = new();

        static ItemHelper()
        {
            foreach (var type in ModReflect.Types(RefinedChemistry.Instance.Manifest.ID))
            {
                var tag = type.GetCustomAttribute<WaterContainerAttribute>();
                if (tag != null)
                    waterContainer.Add(tag.ItemId);

                var scroll = type.GetCustomAttribute<ScrollAbleAttribute>();
                if (scroll != null)
                {
                    scrollHandlers.Add(
                        scroll.ItemId,
                        ModReflect.StaticMethod<Action<GameItem, int>>(type, scroll.MethodName));

                    scrollTooltips.Add(
                        scroll.ItemId,
                        ModReflect.StaticMethod<Func<GameItem, string>>(type, scroll.TooltipMethodName));
                }
            }
        }

        public static GameItem Create(string ItemId, string spriteKey = null)
        {
            var manifest = RefinedChemistry.Instance.Manifest;
            var item = ItemDirectory
                .CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(manifest.ID, spriteKey ?? ItemId);
            item.name = LocalizationHelper.Item(ItemId, "name");
            item.shortDescription = LocalizationHelper.Item(ItemId, "desc");
            item.flavorText = LocalizationHelper.Item(ItemId, "flavor");
            return item;
        }

        public static bool isWaterContainer(string itemId) => waterContainer.Contains(itemId);

        public static bool isScrollAble(string itemId) => scrollHandlers.ContainsKey(itemId);
        
        public static Action<GameItem, int> getScrollHandler(string itemId)
        {
            return scrollHandlers[itemId];
        }

        public static string getScrollTooltip(GameItem item)
            => scrollTooltips[item.identifier](item);
    }
}
