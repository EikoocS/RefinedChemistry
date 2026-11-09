using RefinedChemistry.Attributes;
using RefinedChemistry.Item;
using System.Linq;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangStory
    {
        internal const string Trust0ClientId = "dr_liang_trust0";
        internal const string Trust20ClientId = "dr_liang_trust20";

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

            QueueTrust20(manager);
        }

        internal static void QueueTrust20(StoreClientManager manager)
        {
            manager.AddClient(CreateTrust20Client());
        }

        private static StoreClient CreateTrust0Client()
        {
            StoreClient client = DrLiang.CreateClient(Trust0ClientId);
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

        internal static StoreClient CreateTrust20Client()
        {
            StoreClient client = DrLiang.CreateClient(Trust20ClientId);
            client.clientIntent = StoreClient.ClientIntent.DIALOGUE;
            client.dismissable = false;
            Dialogue introduction = DrLiangDialogue.SetLines(client.mainDialogue, "CLIENT",
                "dialogue.trust20.introduction", Localize("name"));
            Dialogue gift = introduction.NextDialogue();
            gift.SetStartAction(() =>
            {
                DrLiangData.TrustMax = 40;
                DrLiang.RefreshDisplayNames();
            });
            DrLiangDialogue.SetLines(gift, "CLIENT", "dialogue.trust20.gift")
                .SetEndAction(() =>
                {
                    GameItem box = UtoMedicalBox.Create();
                    FileTrust20FillGift(box);
                    PlayerStore.Instance.AddDirectSellingItemToTable(box, isOwend: true);
                    PlayerStore.Instance.DismissCurrentClient(callNextWhenPossible: false);
                });
            client.CompleteClientCreation();
            return client;
        }

        private static void FileTrust20FillGift(GameItem box)
        {
            var inventory = (GameInventory)box.contentWindow.child;
            var mayAdd = inventory.mayInventoryAddItemFunc;
            inventory.mayInventoryAddItemFunc = null;
            string difficulty = Difficulty.Current.id;
            foreach (string itemId in new[] { Pipette.ItemId, LabelerFactory.ItemId, AdvancedChemScanner.ItemId })
            {
                if (difficulty == Difficulty.CHALLENGE && itemId == Pipette.ItemId)
                    continue;

                GameItem tool = DirectoryMaster.Item(itemId);
                if (difficulty == Difficulty.HARD)
                    ItemFeatureHelper.SetShabby(tool);
                else if (difficulty == Difficulty.CHALLENGE)
                    tool.EnableTag(TypeHelper.NOT_FOR_RESALE);
                GraphUtils.TryAcceptAll(box, tool);
            }

            while (true)
            {
                GameItem supplies = ExperimentalChemical.Create();
                if (GraphUtils.TryAcceptAll(box, supplies) == 0)
                    continue;

                supplies.Destroy();
                break;
            }
            inventory.mayInventoryAddItemFunc = mayAdd;
        }
    }
}
