namespace RefinedChemistry.Item
{
    internal static class MedicalTechItems
    {
        internal const string BoxId = "box_medical_tech";
        internal const string UnlockedBoxId = "box_medical_tech_u";
        internal const string CardId = "uto_card";
        internal const string ModuleId = "uto_module";
        internal const string LockId = "LOCK_ID_UTO";

        internal static void Register(ModItemDirectory directory)
        {
            directory.Add(BoxId, CreateBox);
            directory.Add(UnlockedBoxId, CreateUnlockedBox);
            directory.Add(CardId, CreateCard);
            directory.Add(ModuleId, CreateModule);
        }

        private static GameItem CreateBox()
        {
            GameItem item = CreateFromVanilla(BoxId, "med_box");
            item.name += " (" + LocHelper.GetLocalizedMechanic("mech_lock_sealed") + ")";
            item.ModifyTag(LockHelper.LOCK_ID, state => state.SetString(LockId));
            LockHelper.LockUpContainer(item);
            return item;
        }

        private static GameItem CreateUnlockedBox()
        {
            GameItem item = CreateBox();
            item.identifier = UnlockedBoxId;
            LockHelper.UnlockContainer(item, force: true);
            SetText(item, UnlockedBoxId);
            return item;
        }

        private static GameItem CreateCard()
        {
            GameItem item = CreateFromVanilla(CardId, "med_keycard");
            item.onItemTargetItemFunc = (source, target) => LockHelper.TryOpenLock(source, target, LockId);
            return item;
        }

        private static GameItem CreateModule()
        {
            GameItem item = CreateFromVanilla(ModuleId, "crypto_module_med");
            item.ModifyTag(MachineDesequencer.DESEQUENCER_TARGET_ID_TAG, state => state.SetString(CardId));
            return item;
        }

        private static GameItem CreateFromVanilla(string itemId, string vanillaId)
        {
            GameItem item = DirectoryMaster.GetFactory<GameItem>(vanillaId)();
            item.identifier = itemId;
            item.SetSpriteAndShapeFromMod(RefinedChemistry.Instance.Manifest.ID, itemId);
            SetText(item, itemId);
            return item;
        }

        private static void SetText(GameItem item, string itemId)
        {
            item.name = LocalizationHelper.Format(itemId + "_name");
            item.shortDescription = LocalizationHelper.Format(itemId + "_desc");
            item.flavorText = LocalizationHelper.Format(itemId + "_flavor");
        }
    }
}
