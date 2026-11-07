using HarmonyLib;
using RefinedChemistry.Item;

namespace RefinedChemistry
{
    [HarmonyPatch(typeof(GameItem), nameof(GameItem.SetSpriteAndShape))]
    internal static class MedicalTechBoxSpritePatch
    {
        private static bool Prefix(GameItem __instance, string spriteAtlasPath, string spritePath,
            ref GameItem __result)
        {
            if ((__instance.identifier != MedicalTechItems.BoxId &&
                 __instance.identifier != MedicalTechItems.UnlockedBoxId) ||
                spriteAtlasPath != RefinedChemistry.Instance.Manifest.ID)
                return true;

            __result = __instance.SetSpriteAndShapeFromMod(spriteAtlasPath, spritePath);
            return false;
        }
    }
}
