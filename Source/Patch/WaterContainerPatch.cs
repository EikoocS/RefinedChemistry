using HarmonyLib;

namespace RefinedChemistry.Source.Patch
{
    [HarmonyPatch(typeof(SpriteHelper), nameof(SpriteHelper.UpdateWaterContainerSprite))]
    internal static class ScollAbleSpritePatch
    {
        private static void Postfix(GameItem gameItem)
        {
            if (ItemHelper.isWaterContainer(gameItem.identifier))
            {
                ((GameItemElement)gameItem).HandleInnateModSprite();
            }
        }
    }
}
