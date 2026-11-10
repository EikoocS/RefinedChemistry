using RefinedChemistry.NPC;
using RefinedChemistry.Item;
using RefinedChemistry.UI;
using UnityEngine;

namespace RefinedChemistry
{
    public class RefinedChemistry : IMod
    {
        public static RefinedChemistry Instance { get; private set; }
        public ModManifest Manifest { get; private set; }
        public ModLog Log { get; private set; }

        public void Init(ModManifest manifest)
        {
            Instance = this;
            Manifest = manifest;
            Log = new ModLog(manifest);
            Log.Log($"{manifest.Name} v{manifest.ModVersion} loaded.");
        }

        public void OnEnable()
        {
            DrLiang.Enable();
            DrLiangPhone.Enable();
            ModHelper.AddDebugFunction(Manifest.ID, CustomUIDebugPanel.CATEGORY_SPECIAL,
                "RefinedChemistry", DebugMenu.Open, tall: true);
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnGenerateCustomerVeryLate += DrLiangStory.QueueTrust0IfEligible;
            ModHook.OnGenerateCustomerVeryLate += DrLiangStory.QueueTrust20IfEligible;
            ModHook.OnGenerateCustomerVeryLate += DrLiangBuyer.QueueIfEligible;
            ModHook.OnPlaceInventorInventoryItemEarly += DrLiangToolSupplier.PlaceInventorStock;
            ModHook.OnCreateTooltipLate += ChemFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate += LabelerFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate += AdvancedChemScannerUI.CreateTooltip;
        }

        public void OnDisable()
        {
            DrLiangPhone.Disable();
            DrLiang.Disable();
            ModHelper.RemoveDebugFunctions(Manifest.ID);
            CustomUIManager.Instance?.CloseWindow(DebugMenu.WindowId);
            CustomUIManager.Instance?.CloseWindow(SaveDataDebugMenu.WindowId);
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnGenerateCustomerVeryLate -= DrLiangStory.QueueTrust0IfEligible;
            ModHook.OnGenerateCustomerVeryLate -= DrLiangStory.QueueTrust20IfEligible;
            ModHook.OnGenerateCustomerVeryLate -= DrLiangBuyer.QueueIfEligible;
            ModHook.OnPlaceInventorInventoryItemEarly -= DrLiangToolSupplier.PlaceInventorStock;
            ModHook.OnCreateTooltipLate -= ChemFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate -= LabelerFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate -= AdvancedChemScannerUI.CreateTooltip;
        }

        public static LoadedMod getLoadedMod()
        {
            return ModLoader.Mods.Find(m => m.Manifest.ID == Instance.Manifest.ID);
        }

        public static Sprite GetSprite(string key)
        {
            return getLoadedMod().Sprites[key];
        }

        private void RegisterItems(ModItemDirectory directory)
        {
            directory.Add(Pipette.ItemId, new Pipette().Create);
            directory.Add(ImitationPipette.ItemId, new ImitationPipette().Create);
            directory.Add(AdjustableCylinder.ItemId, AdjustableCylinder.Create);
            directory.Add(Flask.ItemId, Flask.Create);
            directory.Add(ChemicalTank.ItemId, ChemicalTank.Create);
            directory.Add(ChemFactory.ItemId, ChemFactory.Create);
            directory.Add(LabelerFactory.ItemId, LabelerFactory.Create);
            directory.Add(DrLiangCard.ItemId, DrLiangCard.Create);
            directory.Add(AdvancedChemScanner.ItemId, AdvancedChemScanner.Create);
            directory.Add(ExperimentalChemical.ItemId, ExperimentalChemical.Create);
            directory.Add(UtoMedicalBox.ItemId, UtoMedicalBox.CreateLocked);
            directory.Add(UtoMedicalBox.UnlockedItemId, UtoMedicalBox.Create);
            directory.Add(UtoMedicalCard.ItemId, UtoMedicalCard.Create);
            directory.Add(UtoMedicalModule.ItemId, UtoMedicalModule.Create);
            Flask.RegisterUpgradeMaterials(directory);
        }
    }
}
