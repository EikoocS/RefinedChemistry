using System.Linq;
using RefinedChemistry.Item;
using UnityEngine;

namespace RefinedChemistry
{
    internal static class DrLiang
    {
        private const string FirstVisitClientId = "dr_liang_first_visit";
        internal const string ToolsVisitClientId = "dr_liang_tools_visit";
        internal const string MaterialsVisitClientId = "dr_liang_materials_visit";
        internal const string BuyerVisitClientId = "dr_liang_buyer_visit";
        internal const string NameRevealVisitClientId = "dr_liang_name_reveal_visit";
        private const string PortraitKey = "refined_chemistry__dr_liang";
        private const string ChannelMarkupId = "refined_chemistry__channel_markup";
        private static readonly string[] ToolsStock =
        {
            ImitationPipette.ItemId, AutoCylinder.ItemId, Flask.ItemId, Flask.ItemId,
            "chem_module", "chem_module", "evaporator"
        };
        private static readonly string[] MaterialsStock = { "common_chemical", ExperimentalChemical.ItemId };
        private static Sprite portrait;

        internal static string NameKey => ModSaveData.DrLiangNameKnown ? "dr_liang_name" : "dr_liang_name_unknow";
        internal static string DisplayName => LocalizationHelper.Format(NameKey);

        internal static void Enable()
        {
            StoreClientListDict.storeClientDict[ToolsVisitClientId] = CreateToolsVisitClient;
            StoreClientListDict.storeClientDict[MaterialsVisitClientId] = CreateMaterialsVisitClient;
            StoreClientListDict.storeClientDict[BuyerVisitClientId] = CreateBuyerVisitClient;
            StoreClientListDict.storeClientDict[NameRevealVisitClientId] = CreateNameRevealVisitClient;
        }

        internal static void Disable()
        {
            StoreClientListDict.storeClientDict.Remove(ToolsVisitClientId);
            StoreClientListDict.storeClientDict.Remove(MaterialsVisitClientId);
            StoreClientListDict.storeClientDict.Remove(BuyerVisitClientId);
            StoreClientListDict.storeClientDict.Remove(NameRevealVisitClientId);
            if (PlayerStore.HasInstance)
                ItemFeatureList.SpecialFeature.Remove(ChannelMarkupId);
        }

        internal static void OnUpgradeUnlocked(string upgradeId)
        {
            if (upgradeId == "RETIRED_CHEMIST")
                QueueFirstVisitIfEligible(PlayerStore.Instance.storeClientManager);
        }

        internal static void QueueFirstVisitIfEligible(StoreClientManager manager)
        {
            int day = StoreStation.Instance.dayCounter;
            if (!NetworkUpgrade.IsUnlocked("RETIRED_CHEMIST") || day <= 5 || day % 7 != 3 ||
                ModSaveData.DrLiangIntroCompleted || ModSaveData.DrLiangRemoved ||
                manager.clientStack.Any(client => client.identifier == FirstVisitClientId) ||
                PlayerStore.Instance.currentClientInstance?.GetClientBlueprint().identifier == FirstVisitClientId)
                return;

            QueueFirstVisit(manager);
        }

        internal static void QueueFirstVisit(StoreClientManager manager)
        {
            manager.AddNextClient(CreateFirstVisitClient());
        }

        internal static void QueueWeeklyBuyerIfEligible(StoreClientManager manager)
        {
            if (StoreStation.GetDayCounter() % 7 != 3 || !ModSaveData.DrLiangIntroCompleted ||
                ModSaveData.DrLiangRemoved || StoreClientManager.IsClientOnCooldown(BuyerVisitClientId) ||
                PlayerStore.Instance.futurStoreClientIdQueue.Contains(NameRevealVisitClientId) ||
                manager.clientStack.Any(client => client.identifier == NameRevealVisitClientId))
                return;

            manager.TryAddClient(CreateBuyerVisitClient());
        }

        internal static void QueueNameRevealVisitIfEligible(StoreClientManager manager)
        {
            if (ModSaveData.DrLiangRemoved ||
                ModSaveData.DrLiangTrustLimit != 20 || DrLiangTrust.Value < 20 ||
                PlayerStore.Instance.futurStoreClientIdQueue.Contains(NameRevealVisitClientId) ||
                manager.clientStack.Any(client => client.identifier == NameRevealVisitClientId) ||
                PlayerStore.Instance.currentClientInstance?.GetClientBlueprint().identifier == NameRevealVisitClientId)
                return;

            manager.AddClient(CreateNameRevealVisitClient());
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
            return client != null && (client.identifier == FirstVisitClientId ||
                client.identifier == ToolsVisitClientId || client.identifier == MaterialsVisitClientId ||
                client.identifier == BuyerVisitClientId || client.identifier == NameRevealVisitClientId);
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

        private static StoreClient CreateClient(string identifier)
        {
            RegisterPortrait();
            var client = new StoreClient
            {
                identifier = identifier,
                displayName = DisplayName,
                spriteName = PortraitKey
            };
            StoreClientFactionSetup.InitLowerLevelCitizen(client);
            client.onItemBought += DrLiangTrust.OnItemBoughtByLiang;
            client.onItemSold += DrLiangTrust.OnItemSoldByLiang;
            DrLiangDialogue.Configure(client);
            return client;
        }

        private static StoreClient CreateFirstVisitClient()
        {
            StoreClient client = CreateClient(FirstVisitClientId);
            client.clientIntent = StoreClient.ClientIntent.DIALOGUE;
            client.dismissable = false;
            Dialogue last = DrLiangDialogue.SetLines(client.mainDialogue, client.displayName,
                "dr_liang_first_visit_dialogue");

            last.SetEndAction(() =>
            {
                PlayerStore.Instance.AddDirectSellingItemToTable(DrLiangCard.ItemId, isOwend: true);
                PlayerStore.Instance.AddDirectSellingItemToTable(ImitationPipette.ItemId, isOwend: true);
                ModSaveData.DrLiangIntroCompleted = true;
                PlayerStore.Instance.DismissCurrentClient(callNextWhenPossible: false);
            });
            client.CompleteClientCreation();
            return client;
        }

        private static StoreClient CreateToolsVisitClient()
        {
            return CreateSupplyClient(ToolsVisitClientId, ToolsStock, "premiumBuy15");
        }

        private static StoreClient CreateNameRevealVisitClient()
        {
            StoreClient client = CreateClient(NameRevealVisitClientId);
            client.clientIntent = StoreClient.ClientIntent.DIALOGUE;
            client.dismissable = false;
            Dialogue introduction = DrLiangDialogue.SetLines(client.mainDialogue, "CLIENT",
                "dr_liang_name_reveal_dialogue", LocalizationHelper.Format("dr_liang_name"));
            Dialogue gift = introduction.NextDialogue();
            gift.SetStartAction(() =>
            {
                ModSaveData.DrLiangNameKnown = true;
                RefreshDisplayNames();
            });
            DrLiangDialogue.SetLines(gift, "CLIENT", "dr_liang_name_reveal_gift_dialogue")
                .SetEndAction(() =>
                {
                    PlayerStore.Instance.AddDirectSellingItemToTable(MedicalTechItems.BoxId, isOwend: true);
                    PlayerStore.Instance.AddDirectSellingItemToTable(MedicalTechItems.CardId, isOwend: true);
                    PlayerStore.Instance.DismissCurrentClient(callNextWhenPossible: false);
                    ModSaveData.DrLiangTrustLimit = 40;
                });
            client.CompleteClientCreation();
            return client;
        }

        private static StoreClient CreateBuyerVisitClient()
        {
            StoreClient client = CreateClient(BuyerVisitClientId);
            client.clientIntent = StoreClient.ClientIntent.BUY;
            client.IsPersonalConsumption = false;
            client.cooldown = 7;
            client.SetBudget(200 + RNG.GetRandomInt(0, 50) + StoreStation.GetDayCounter() / 4);
            client.clientBuyingTagList.Add("MEDICAL");
            client.AcceptingContrabandOverride = true;
            client.CanExposeChem = true;
            client.onItemAddedToWeighted += DrLiangBuyer.ApplyDiscount;
            client.AddBasicDialog();
            DrLiangDialogue.ConfigureBuyer(client);
            client.CompleteClientCreation();
            return client;
        }

        private static StoreClient CreateMaterialsVisitClient()
        {
            ItemFeatureList.SpecialFeature[ChannelMarkupId] = CreateChannelMarkup;
            return CreateSupplyClient(MaterialsVisitClientId, MaterialsStock, ChannelMarkupId);
        }

        private static StoreClient CreateSupplyClient(string identifier, string[] stock, string markupId)
        {
            StoreClient client = CreateClient(identifier);
            client.clientIntent = StoreClient.ClientIntent.SELL;
            client.clientItemFeatureBuying.Add(markupId);
            client.mainDialogue.SetText(client.displayName, LocalizationHelper.Format("dr_liang_sell_dialogue"))
                .SetEndAction(() =>
                {
                    foreach (string itemId in stock)
                        PlayerStore.Instance.AddDirectSellingItemToTable(itemId);
                });
            client.AddBasicDialog();
            DrLiangDialogue.ConfigureTrade(client);
            client.CompleteClientCreation();
            return client;
        }

        private static ItemFeature CreateChannelMarkup()
        {
            ItemFeature feature = ItemFeatureList.PremiumBuy15();
            feature.identifier = ChannelMarkupId;
            feature.category = ChannelMarkupId;
            string name = LocalizationHelper.Format("dr_liang_channel_markup");
            feature.SetPublicDisplay(name);
            feature.SetActualDisplay(name);
            feature.SetPreExposedValueMod(Mathf.RoundToInt(Difficulty.Rules.retiredSupplierMarkup * (10f / 15f)));
            return feature;
        }
    }
}
