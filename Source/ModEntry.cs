using RefinedChemistry.Item;

namespace RefinedChemistry
{
    public class ModEntry : IMod
    {
        public static ModEntry Instance { get; private set; }
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
        }

        public void OnDisable()
        {
            ModHook.OnModItemDirectoryInit -= RegisterItems;
            ModHook.OnNetworkUpgradeUnlocked -= ChemistSupplies.OnUpgradeUnlocked;
        }

        private void RegisterItems(ModItemDirectory directory)
        {
            directory.Add(Pipette.ItemId, Pipette.Create);
            directory.Add(Pipette.ImitationItemId, Pipette.CreateImitation);
            directory.Add(Beaker.ItemId, Beaker.Create);
            directory.Add(Flask.ItemId, Flask.Create);
            directory.Add(Diluter.ItemId, Diluter.Create);
            Flask.RegisterUpgradeMaterials(directory);
        }
    }
}
