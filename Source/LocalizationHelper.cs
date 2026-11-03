using System;
using System.Collections.Generic;
using System.Text;

namespace RefinedChemistry
{
    internal static class LocalizationHelper
    {
        public static string Format(string key, params object[] args)
        {
            return ModHelper.GetLocalized(RefinedChemistry.Instance.Manifest.ID, key, args);
        }
    }
}
