using HarmonyLib;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(StorePhoneClient), nameof(StorePhoneClient.GetLocalizedDisplayName))]
    internal static class DrLiangPhone
    {
        private const long Number = 2324;
        private const int CooldownDays = 7;
        private const string DialogId = "refined_chemistry__dr_liang";

        internal static void Enable()
        {
            PhoneClientList.DialogFuncs[DialogId] = CreateDialogue;
            ModHook.OnGameLoadedEarly += RegisterContact;
            ModHook.OnNewGameStarted += OnNewGameStarted;
            if (PlayerStore.HasInstance)
                RegisterContact();
        }

        internal static void Disable()
        {
            ModHook.OnGameLoadedEarly -= RegisterContact;
            ModHook.OnNewGameStarted -= OnNewGameStarted;
            PhoneClientList.DialogFuncs.Remove(DialogId);
            if (PlayerStore.HasInstance)
                PlayerStore.Instance.PhoneClientDict.Remove(Number);
        }

        private static void OnNewGameStarted(string scenarioId) => RegisterContact();

        private static void RegisterContact()
        {
            PlayerStore.Instance.PhoneClientDict.TryAdd(Number, new StorePhoneClient
            {
                locID = DrLiang.NameKey,
                dialogFuncId = DialogId,
                phoneState = StorePhoneClient.PhoneState.Regular
            });
            PlayerStore.Instance.PhoneClientDict[Number].cooldownDuration = CooldownDays;
        }

        private static bool Prefix(StorePhoneClient __instance, ref string __result)
        {
            if (__instance.dialogFuncId != DialogId)
                return true;

            __result = DrLiang.DisplayName;
            return false;
        }

        private static Dialogue CreateDialogue(StorePhoneClient contact)
        {
            string name = contact.GetLocalizedDisplayName();
            if (contact.phoneState == StorePhoneClient.PhoneState.Cooldown)
                return PhoneDialogList.GenericCooldown(name);

            var menu = new Dialogue().SetText(name, LocalizationHelper.Format("dr_liang_phone_greeting"));
            menu.AddChoice(DirectoryMaster.Item("$"), "", "",
                CreateRequestDialogue(name, "dr_liang_phone_tools"),
                () => RequestVisit(contact, DrLiang.ToolsVisitClientId));
            menu.AddChoice(DirectoryMaster.Item("$"), "", "",
                CreateRequestDialogue(name, "dr_liang_phone_materials"),
                () => RequestVisit(contact, DrLiang.MaterialsVisitClientId));

            var cancel = new Dialogue().SetText(DialoguePhoneUIManager.PLAYER,
                LocHelper.GetLocalizedDialoguePhone("end_call"));
            cancel.NextDialogue()
                .SetText(name, LocHelper.GetLocalizedDialoguePhone("goodbye"))
                .SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            menu.AddChoice(DirectoryMaster.Item("$"), "", "", cancel);
            return menu;
        }

        private static void RequestVisit(StorePhoneClient contact, string clientId)
        {
            PlayerStore.Instance.QueueFuturClient(clientId, 1);
            contact.UseService();
        }

        private static Dialogue CreateRequestDialogue(string name, string key)
        {
            var choice = new Dialogue().SetText(DialoguePhoneUIManager.PLAYER, LocalizationHelper.Format(key));
            choice.NextDialogue()
                .SetText(name, LocalizationHelper.Format("dr_liang_phone_visit_reply"))
                .SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            return choice;
        }

        [HarmonyPatch(typeof(PhoneUIManager))]
        private static class RemovedPhonePatch
        {
            [HarmonyPrefix, HarmonyPatch(nameof(PhoneUIManager.HandleCall))]
            private static bool HandleCall(PhoneUIManager __instance, long currentNumber, ref long ___currentNumber)
            {
                if (currentNumber != Number || !ModSaveData.DrLiangRemoved)
                    return true;

                ___currentNumber = 0;
                __instance.phoneClientDrop.Play();
                __instance.statusDisplay.text = LocHelper.GetLocalizedUI("ui_phone_no_answer");
                return false;
            }

            [HarmonyPostfix, HarmonyPatch(nameof(PhoneUIManager.WillAnswerCall))]
            private static void WillAnswerCall(long number, ref bool __result)
            {
                if (number == Number && ModSaveData.DrLiangRemoved)
                    __result = false;
            }
        }
    }
}
