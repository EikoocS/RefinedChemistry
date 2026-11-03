using RefinedChemistry.Item;
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
            foreach (var type in typeof(ItemHelper).Assembly.GetTypes())
            {
                var tag = type.GetCustomAttribute<WaterContainerAttribute>();
                if (tag != null)
                    waterContainer.Add(tag.ItemId);

                var scroll = type.GetCustomAttribute<ScrollAbleAttribute>();
                if (scroll != null)
                {
                    var method = type.GetMethod(
                        scroll.MethodName,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
                        binder: null,
                        types: [ typeof(GameItem), typeof(int)],
                        modifiers: null)
                        ?? throw new MissingMethodException(type.FullName, scroll.MethodName);

                    scrollHandlers.Add(
                        scroll.ItemId,
                        (Action<GameItem, int>)method.CreateDelegate(typeof(Action<GameItem, int>)));

                    var tooltipMethod = type.GetMethod(
                        scroll.TooltipMethodName,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
                        binder: null,
                        types: [typeof(GameItem)],
                        modifiers: null)
                        ?? throw new MissingMethodException(type.FullName, scroll.TooltipMethodName);

                    scrollTooltips.Add(
                        scroll.ItemId,
                        (Func<GameItem, string>)tooltipMethod.CreateDelegate(typeof(Func<GameItem, string>)));
                }
            }
        }

        public static GameItem Create(string ItemId, string spriteKey = null)
        {
            var manifest = RefinedChemistry.Instance.Manifest;
            var item = ItemDirectory
                .CreateEmptyItem(ItemId)
                .SetSpriteAndShapeFromMod(manifest.ID, spriteKey ?? ItemId);
            item.name = LocalizationHelper.Format(ItemId + "_name");
            item.shortDescription = LocalizationHelper.Format(ItemId + "_desc");
            item.flavorText = LocalizationHelper.Format(ItemId + "_flavor");
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
