using System;
using System.Globalization;
using UnityEngine;

namespace RefinedChemistry
{
    internal static class SaveDataDebugMenu
    {
        internal const string WindowId = "refined_chemistry__save_data_debug";

        internal static void Open()
        {
            var builder = DebugMenu.CreateWindow(WindowId, "RefinedChemistry Save Data", DebugMenu.WindowId);

            if (PlayerStore.HasInstance)
            {
                builder.AddLabel("Edits apply immediately and save with the game.");
                builder.BeginScrollFlexible();
                builder.AddToggle(ModSaveData.DrLiangIntroCompletedKey, ModSaveData.DrLiangIntroCompleted,
                    value => ModSaveData.DrLiangIntroCompleted = value);
                builder.AddToggle(ModSaveData.DrLiangRemovedKey, ModSaveData.DrLiangRemoved,
                    value => ModSaveData.DrLiangRemoved = value);
                builder.AddToggle(ModSaveData.DrLiangNameKnownKey, ModSaveData.DrLiangNameKnown, value =>
                {
                    ModSaveData.DrLiangNameKnown = value;
                    DrLiang.RefreshDisplayNames();
                });
                builder.AddLabel($"Effective Trust: {DrLiangTrust.Value:0.###}");
                CustomUIElement effectiveTrust = builder.Last;
                Action refresh = () => effectiveTrust.SetText($"Effective Trust: {DrLiangTrust.Value:0.###}");
                AddNumber(builder, ModSaveData.DrLiangTrustLimitKey,
                    () => ModSaveData.DrLiangTrustLimit,
                    value => ModSaveData.DrLiangTrustLimit = Mathf.RoundToInt(value), refresh);
                AddNumber(builder, ModSaveData.DrLiangTrustRawKey,
                    () => ModSaveData.DrLiangTrustRaw,
                    value => ModSaveData.DrLiangTrustRaw = value, refresh);
                AddNumber(builder, ModSaveData.DrLiangNeedleGiftBonusKey,
                    () => ModSaveData.DrLiangNeedleGiftBonus,
                    value => ModSaveData.DrLiangNeedleGiftBonus = Mathf.RoundToInt(value), refresh);
                AddNumber(builder, ModSaveData.DrLiangNeedleSalePenaltyKey,
                    () => ModSaveData.DrLiangNeedleSalePenalty,
                    value => ModSaveData.DrLiangNeedleSalePenalty = Mathf.RoundToInt(value), refresh);
                builder.End();
            }
            else
            {
                builder.AddLabel("No game loaded.");
            }

            CustomUIDebugPanel.SetBackgroundAlpha(builder.Show(), 0.8f);
        }

        private static void AddNumber(CustomUIBuilder builder, string key, Func<float> get,
            Action<float> set, Action refresh)
        {
            builder.AddLabel(key);
            CustomUIElement input = null;
            builder.AddInput(key, get().ToString("R", CultureInfo.InvariantCulture), text =>
            {
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    set(value);
                    refresh();
                }
                input.SetString(get().ToString("R", CultureInfo.InvariantCulture));
            });
            input = builder.Last;
        }
    }
}
