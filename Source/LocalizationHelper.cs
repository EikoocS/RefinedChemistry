using System;
using System.Collections.Generic;
using System.Linq;

namespace RefinedChemistry
{
    internal static class LocalizationHelper
    {
        public static string Format(string key, params object[] args)
        {
            return ModHelper.GetLocalized(RefinedChemistry.Instance.Manifest.ID, key, args);
        }

        public static string Item(string itemId, string property, params object[] args) =>
            Format("item." + itemId + "." + property, args);

        public static List<string> List(string prefix, params object[] args)
        {
            string locale = LocHelper.GetCurrentLocaleCode();
            return RefinedChemistry.getLoadedMod().Localization
                .Where(table => string.Equals(table.Key, locale, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(table.Key, "en", StringComparison.OrdinalIgnoreCase))
                .SelectMany(table => table.Value.Keys)
                .Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(key => key, StringComparer.Ordinal)
                .Select(key => Format(key, args))
                .ToList();
        }
    }
}
