using RefinedChemistry.Item;
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
            ModHook.OnModItemDirectoryInit += RegisterItems;
            ModHook.OnNetworkUpgradeUnlocked += ChemistSupplies.OnUpgradeUnlocked;
            ModHook.OnCreateTooltipLate += ChemFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate += LabelerFactory.CreateTooltip;
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnNetworkUpgradeUnlocked -= ChemistSupplies.OnUpgradeUnlocked;
            ModHook.OnCreateTooltipLate -= ChemFactory.CreateTooltip;
            ModHook.OnCreateTooltipLate -= LabelerFactory.CreateTooltip;
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
            directory.Add(ChemFactory.ItemId, ChemFactory.Create);
            directory.Add(LabelerFactory.ItemId, LabelerFactory.Create);
            directory.Add(ExperimentalChemical.ItemId, ExperimentalChemical.Create);
            directory.Add(UtoMedicalBox.ItemId, UtoMedicalBox.Create);
            directory.Add(UtoMedicalBox.UnlockedItemId, UtoMedicalBox.CreateUnlocked);
            directory.Add(UtoMedicalCard.ItemId, UtoMedicalCard.Create);
            directory.Add(UtoMedicalModule.ItemId, UtoMedicalModule.Create);
            Flask.RegisterUpgradeMaterials(directory);
        }
    }
}
