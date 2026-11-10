using HarmonyLib;
using RefinedChemistry.Item;
using System.Collections.Generic;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(PlayerStore), nameof(PlayerStore.SaveGame))]
    internal static class EnderFlaskSavePatch
    {
        private static void Prefix() => EnderFlaskLinks.Save();
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.DecodeNodes))]
    internal static class EnderFlaskLoadPatch
    {
        private static void Postfix(List<SaveItemNode> savedItems)
        {
            foreach (var node in savedItems)
                EnderFlaskLinks.Register(node.tempLink);
        }
    }

    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.Destroy))]
    internal static class EnderFlaskDestroyPatch
    {
        private static void Prefix(GameItemElement __instance) => EnderFlaskLinks.Remove(__instance);
    }

    [HarmonyPatch(typeof(EmporiumEntry), nameof(EmporiumEntry.GetAllItems))]
    internal static class EnderFlaskTickPatch
    {
        private static void Postfix(List<GameItem> __result) => EnderFlaskLinks.SelectTickOwners(__result);
    }

    [HarmonyPatch(typeof(SpriteHelper), nameof(SpriteHelper.UpdateWaterContainerSprite))]
    internal static class EnderFlaskSpritePatch
    {
        private static void Postfix(GameItem gameItem) => EnderFlaskLinks.Refresh(gameItem);
    }

    [HarmonyPatch(typeof(WaterHelper), nameof(WaterHelper.TransferLiquid))]
    internal static class EnderFlaskTransferPatch
    {
        private static bool Prefix(GameItem sourceContainer, GameItem targetContainer) =>
            !EnderFlaskLinks.ShareContents(sourceContainer, targetContainer);
    }
}
