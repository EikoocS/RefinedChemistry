using System.Linq;
using UnityEngine;

namespace RefinedChemistry.NPC
{
    internal static class DrLiang
    {
        private const string PortraitKey = "refined_chemistry__dr_liang";
        private static Sprite portrait;

        internal static string NameKey => DrLiangData.TrustMax > 20 ? "npc.dr_liang.name" : "npc.dr_liang.name_unknown";
        internal static string DisplayName => LocalizationHelper.Format(NameKey);

        internal static void Enable()
        {
            StoreClientListDict.storeClientDict[DrLiangToolSupplier.ClientId] = DrLiangToolSupplier.Create;
            StoreClientListDict.storeClientDict[DrLiangMaterialSupplier.ClientId] = DrLiangMaterialSupplier.Create;
            StoreClientListDict.storeClientDict[DrLiangBuyer.ClientId] = DrLiangBuyer.Create;
            StoreClientListDict.storeClientDict[DrLiangStory.Trust20ClientId] = DrLiangStory.CreateTrust20Client;
        }

        internal static void Disable()
        {
            StoreClientListDict.storeClientDict.Remove(DrLiangToolSupplier.ClientId);
            StoreClientListDict.storeClientDict.Remove(DrLiangMaterialSupplier.ClientId);
            StoreClientListDict.storeClientDict.Remove(DrLiangBuyer.ClientId);
            StoreClientListDict.storeClientDict.Remove(DrLiangStory.Trust20ClientId);
            DrLiangMaterialSupplier.Disable();
        }

        internal static void RefreshDisplayNames()
        {
            foreach (StoreClient client in PlayerStore.Instance.storeClientManager.clientStack.Where(IsLiang))
                client.displayName = DisplayName;
            StoreClient current = PlayerStore.Instance.currentClientInstance?.GetClientBlueprint();
            if (IsLiang(current))
                current.displayName = DisplayName;
        }

        internal static bool IsLiang(StoreClient client)
        {
            return client != null && (client.identifier == DrLiangStory.Trust0ClientId ||
                client.identifier == DrLiangToolSupplier.ClientId || client.identifier == DrLiangMaterialSupplier.ClientId ||
                client.identifier == DrLiangBuyer.ClientId || client.identifier == DrLiangStory.Trust20ClientId);
        }

        private static void RegisterPortrait()
        {
            if (portrait == null)
            {
                Texture2D texture = RefinedChemistry.getLoadedMod().Textures["dr_liang_portrait"];
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                Sprite template = SpriteDict.Instance.GetSprite("mentor");
                Vector2 pivot = template.pivot / template.rect.size;
                portrait = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                    pivot, template.pixelsPerUnit);
                portrait.name = PortraitKey;
            }
            SpriteDict.Instance.spriteDictionary[PortraitKey] = portrait;
        }

        internal static StoreClient CreateClient(string identifier)
        {
            RegisterPortrait();
            var client = new StoreClient
            {
                identifier = identifier,
                displayName = DisplayName,
                spriteName = PortraitKey
            };
            StoreClientFactionSetup.InitLowerLevelCitizen(client);
            DrLiangDialogue.Configure(client);
            return client;
        }
    }
}
