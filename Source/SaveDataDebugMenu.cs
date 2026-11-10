using RefinedChemistry.NPC;
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
                builder.AddLabel($"{DrLiangData.StateKey}: {DrLiangData.State}");
                CustomUIElement stateDisplay = builder.Last;
                foreach (DrLiangData.NpcState state in Enum.GetValues(typeof(DrLiangData.NpcState)))
                {
                    builder.AddButton(state.ToString(), () =>
                    {
                        DrLiangData.State = state;
                        stateDisplay.SetText($"{DrLiangData.StateKey}: {state}");
                    });
                }
                builder.AddLabel($"Effective Trust: {DrLiangData.Trust:0.###}");
                CustomUIElement effectiveTrust = builder.Last;
                Action refresh = () => effectiveTrust.SetText($"Effective Trust: {DrLiangData.Trust:0.###}");
                AddNumber(builder, DrLiangData.TrustMaxKey,
                    () => DrLiangData.TrustMax,
                    value =>
                    {
                        DrLiangData.TrustMax = Mathf.RoundToInt(value);
                        DrLiang.RefreshDisplayNames();
                    }, refresh);
                AddNumber(builder, DrLiangData.TrustRawKey,
                    () => DrLiangData.TrustRaw,
                    value => DrLiangData.TrustRaw = value, refresh);
                AddNumber(builder, DrLiangData.ClinicVolunteerTrustCorrectionKey,
                    () => DrLiangData.ClinicVolunteerTrustCorrection,
                    value => DrLiangData.ClinicVolunteerTrustCorrection = Mathf.RoundToInt(value), refresh);
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
