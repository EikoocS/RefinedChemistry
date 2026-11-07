using RefinedChemistry.Attributes;
using System.Linq;
using RefinedChemistry.Item;
using UnityEngine;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiang
    {
        private const string Trust0ClientId = "dr_liang_trust0";
        internal const string Trust20ClientId = "dr_liang_trust20";
        private const string PortraitKey = "refined_chemistry__dr_liang";
        private static Sprite portrait;

        internal static string NameKey => DrLiangData.TrustMax > 20 ? "npc.dr_liang.name" : "npc.dr_liang.name_unknown";
        internal static string DisplayName => LocalizationHelper.Format(NameKey);

        internal static void Enable()
        {
            StoreClientListDict.storeClientDict[DrLiangToolSupplier.ClientId] = DrLiangToolSupplier.Create;
            StoreClientListDict.storeClientDict[DrLiangMaterialSupplier.ClientId] = DrLiangMaterialSupplier.Create;
            StoreClientListDict.storeClientDict[DrLiangBuyer.ClientId] = DrLiangBuyer.Create;
            StoreClientListDict.storeClientDict[Trust20ClientId] = CreateTrust20Client;
        }

        internal static void Disable()
        {
            StoreClientListDict.storeClientDict.Remove(DrLiangToolSupplier.ClientId);
            StoreClientListDict.storeClientDict.Remove(DrLiangMaterialSupplier.ClientId);
            StoreClientListDict.storeClientDict.Remove(DrLiangBuyer.ClientId);
            StoreClientListDict.storeClientDict.Remove(Trust20ClientId);
            DrLiangMaterialSupplier.Disable();
        }

        internal static void QueueTrust0IfEligible(StoreClientManager manager)
        {
            int day = StoreStation.Instance.dayCounter;
            if (!NetworkUpgrade.IsUnlocked("RETIRED_CHEMIST") || day <= 5 || day % 7 != 3 ||
                DrLiangData.TrustMax != 0 || DrLiangData.Trust < 0 ||
                DrLiangData.State == DrLiangData.NpcState.REMOVED ||
                manager.clientStack.Any(client => client.identifier == Trust0ClientId) ||
                PlayerStore.Instance.currentClientInstance?.GetClientBlueprint().identifier == Trust0ClientId)
                return;

            QueueTrust0(manager);
        }

        internal static void QueueTrust0(StoreClientManager manager)
        {
            manager.AddNextClient(CreateTrust0Client());
        }

        internal static void QueueTrust20IfEligible(StoreClientManager manager)
        {
            if (DrLiangData.State == DrLiangData.NpcState.REMOVED ||
                DrLiangData.TrustMax != 20 || DrLiangData.Trust < 20 ||
                PlayerStore.Instance.futurStoreClientIdQueue.Contains(Trust20ClientId) ||
                manager.clientStack.Any(client => client.identifier == Trust20ClientId) ||
                PlayerStore.Instance.currentClientInstance?.GetClientBlueprint().identifier == Trust20ClientId)
                return;

            manager.AddClient(CreateTrust20Client());
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
            return client != null && (client.identifier == Trust0ClientId ||
                client.identifier == DrLiangToolSupplier.ClientId || client.identifier == DrLiangMaterialSupplier.ClientId ||
                client.identifier == DrLiangBuyer.ClientId || client.identifier == Trust20ClientId);
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

        private static StoreClient CreateTrust0Client()
        {
            StoreClient client = CreateClient(Trust0ClientId);
            client.clientIntent = StoreClient.ClientIntent.DIALOGUE;
            client.dismissable = false;
            Dialogue last = DrLiangDialogue.SetLines(client.mainDialogue, client.displayName,
                "dialogue.trust0");

            last.SetEndAction(() =>
            {
                PlayerStore.Instance.AddDirectSellingItemToTable(DrLiangCard.ItemId, isOwend: true);
                PlayerStore.Instance.AddDirectSellingItemToTable(ImitationPipette.ItemId, isOwend: true);
                DrLiangData.TrustMax = 20;
                PlayerStore.Instance.DismissCurrentClient(callNextWhenPossible: false);
            });
            client.CompleteClientCreation();
            return client;
        }

        private static StoreClient CreateTrust20Client()
        {
            StoreClient client = CreateClient(Trust20ClientId);
            client.clientIntent = StoreClient.ClientIntent.DIALOGUE;
            client.dismissable = false;
            Dialogue introduction = DrLiangDialogue.SetLines(client.mainDialogue, "CLIENT",
                "dialogue.trust20", Localize("name"));
            Dialogue gift = introduction.NextDialogue();
            gift.SetStartAction(() =>
            {
                DrLiangData.TrustMax = 40;
                RefreshDisplayNames();
            });
            DrLiangDialogue.SetLines(gift, "CLIENT", "dialogue.trust20.gift")
                .SetEndAction(() =>
                {
                    PlayerStore.Instance.AddDirectSellingItemToTable(UtoMedicalBox.ItemId, isOwend: true);
                    PlayerStore.Instance.AddDirectSellingItemToTable(UtoMedicalCard.ItemId, isOwend: true);
                    PlayerStore.Instance.DismissCurrentClient(callNextWhenPossible: false);
                });
            client.CompleteClientCreation();
            return client;
        }
    }
}
