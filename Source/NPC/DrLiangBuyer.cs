using RefinedChemistry.Attributes;
using System.Linq;
using UnityEngine;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangBuyer
    {
        internal const string ClientId = "dr_liang_buyer";
        private static readonly int[] Discounts = { -10, -17, -35, -50, -75 };

        internal static StoreClient Create()
        {
            StoreClient client = DrLiang.CreateClient(ClientId);
            client.clientIntent = StoreClient.ClientIntent.BUY;
            client.IsPersonalConsumption = false;
            client.cooldown = 7;
            client.SetBudget(200 + RNG.GetRandomInt(0, 50) + StoreStation.GetDayCounter() / 4);
            client.clientBuyingTagList.Add("MEDICAL");
            client.AcceptingContrabandOverride = true;
            client.CanExposeChem = true;
            client.onItemAddedToWeighted += ApplyDiscount;
            client.onItemBought += OnItemBought;
            client.AddBasicDialog();
            ConfigureDialogue(client);
            client.CompleteClientCreation();
            return client;
        }

        internal static void QueueIfEligible(StoreClientManager manager)
        {
            if (StoreStation.GetDayCounter() % 7 != 3 || DrLiangData.TrustMax <= 0 ||
                DrLiangData.State == DrLiangData.NpcState.REMOVED || StoreClientManager.IsClientOnCooldown(ClientId) ||
                PlayerStore.Instance.futurStoreClientIdQueue.Contains(DrLiangStory.Trust20ClientId) ||
                manager.clientStack.Any(client => client.identifier == DrLiangStory.Trust20ClientId))
                return;

            manager.TryAddClient(Create());
        }

        private static void ConfigureDialogue(StoreClient client)
        {
            DrLiangDialogue.SetLines(client.mainDialogue, client.displayName, "buy_dialogue", client.GetBudget());
            DrLiangDialogue.SetText(client.acceptDealDialogue, "buy_deal_accepted_dialogue");
            DrLiangDialogue.SetText(client.allDoneDialogue, "buy_done_dialogue");
            client.placeRightItemWhenSellingToDialogue = new Dialogue().SetText(client.displayName,
                Localize("buy_right_item_dialogue"));
            client.placedWrongItemWhenSellingToDialogue = new Dialogue().SetText(client.displayName,
                Localize("buy_wrong_item_dialogue"));
        }

        private static void OnItemBought(GameItem item, int price)
        {
            if (!item.IsTag("MEDICAL"))
                return;

            float trustValue = price + 2f * GetVoluntaryDiscount(item);
            DrLiangData.AddTradeTrust(trustValue / (ContrabandHelper.IsContraband(item) ? 40f : 10f));
        }

        private static float GetVoluntaryDiscount(GameItem item)
        {
            ItemFeature discount = item.FindItemFeatureByID("bargainDiscount");
            if (discount == null || discount.isDisabled)
                return 0;

            int quotedPrice = (int)item.GetNegociatedValue();
            discount.isDisabled = true;
            try
            {
                int normalPrice = BargainUIManager.GetPreBargainValue(item, (int)item.GetNegociatedValue());
                return Mathf.Max(0, normalPrice - quotedPrice);
            }
            finally
            {
                discount.isDisabled = false;
            }
        }

        private static void ApplyDiscount(GameItem item)
        {
            if (!item.IsTag("MEDICAL"))
                return;

            int level = ContrabandHelper.GetContrabandLevel(item);
            ItemFeature feature = level switch
            {
                1 => ItemFeatureList.DiscountedLowContraband(),
                2 => ItemFeatureList.DiscountedMidContraband(),
                3 => ItemFeatureList.DiscountedHighContraband(),
                4 => ItemFeatureList.DiscountedCriticalContraband(),
                _ => ItemFeatureList.DiscountedBuy25()
            };
            if (level == 0)
            {
                feature.identifier = "refined_chemistry__dr_liang_sincerity";
                feature.category = feature.identifier;
                string name = Localize("sincerity");
                feature.SetPublicDisplay(name);
                feature.SetActualDisplay(name);
            }
            feature.SetPreExposedValueMod(Discounts[level]);
            item.AddItemFeature(feature);
        }
    }
}
