using RefinedChemistry.Attributes;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangPhone
    {
        internal const long Number = 2324;
        private const int CooldownDays = 7;
        internal const string DialogId = "refined_chemistry__dr_liang";

        internal static bool IsAvailable => DrLiangData.TrustMax > 0 &&
            DrLiangData.State != DrLiangData.NpcState.REMOVED;

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

        private static Dialogue CreateDialogue(StorePhoneClient contact)
        {
            string name = contact.GetLocalizedDisplayName();
            if (contact.phoneState == StorePhoneClient.PhoneState.Cooldown)
                return PhoneDialogList.GenericCooldown(name);

            var menu = new Dialogue().SetText(name, Localize("phone_greeting"));
            menu.AddChoice(DirectoryMaster.Item("$"), "", "",
                CreateRequestDialogue(name, "phone_tools"),
                () => DrLiangToolSupplier.RequestVisit(contact));
            menu.AddChoice(DirectoryMaster.Item("$"), "", "",
                CreateRequestDialogue(name, "phone_materials"),
                () => DrLiangMaterialSupplier.RequestVisit(contact));

            var cancel = new Dialogue().SetText(DialoguePhoneUIManager.PLAYER,
                LocHelper.GetLocalizedDialoguePhone("end_call"));
            cancel.NextDialogue()
                .SetText(name, LocHelper.GetLocalizedDialoguePhone("goodbye"))
                .SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            menu.AddChoice(DirectoryMaster.Item("$"), "", "", cancel);
            return menu;
        }

        private static Dialogue CreateRequestDialogue(string name, string key)
        {
            var choice = new Dialogue().SetText(DialoguePhoneUIManager.PLAYER, Localize(key));
            choice.NextDialogue()
                .SetText(name, Localize("phone_visit_reply"))
                .SetEndAction(() => PhoneUIManager.Instance.OnPhoneDialogEnd());
            return choice;
        }
    }
}
