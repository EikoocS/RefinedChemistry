using RefinedChemistry.Attributes;
using RefinedChemistry.Item;
using System.Collections.Generic;
using UnityEngine;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangMaterialSupplier
    {
        internal const string ClientId = "dr_liang_material_supplier";
        private const string ChannelMarkupId = "refined_chemistry__channel_markup";

        internal static StoreClient Create()
        {
            ItemFeatureList.SpecialFeature[ChannelMarkupId] = CreateChannelMarkup;
            StoreClient client = DrLiang.CreateClient(ClientId);
            client.clientIntent = StoreClient.ClientIntent.SELL;
            client.clientItemFeatureBuying.Add(ChannelMarkupId);
            client.mainDialogue.SetText(client.displayName, Localize("sell_dialogue"))
                .SetEndAction(() =>
                {
                    foreach (string itemId in GetStock())
                        PlayerStore.Instance.AddDirectSellingItemToTable(itemId);
                });
            client.onItemSold += (_, price) => DrLiangData.AddTradeTrust(price / 20f);
            client.AddBasicDialog();
            DrLiangDialogue.ConfigureTrade(client);
            client.CompleteClientCreation();
            return client;
        }

        private static IEnumerable<string> GetStock()
        {
            yield return UtoMedicalCard.ItemId;
            if (DrLiangData.Trust < 20)
                yield return "common_chemical";
            yield return ExperimentalChemical.ItemId;
            if (DrLiangData.Trust >= 20)
            {
                yield return ExperimentalChemical.ItemId;
                yield return "restricted_chemical";
                if (RNG.Roll(50))
                    yield return "restricted_chemical";
            }
        }

        internal static void RequestVisit(StorePhoneClient contact)
        {
            PlayerStore.Instance.QueueFuturClient(ClientId, 1);
            contact.UseService();
        }

        internal static void Disable()
        {
            if (PlayerStore.HasInstance)
                ItemFeatureList.SpecialFeature.Remove(ChannelMarkupId);
        }

        private static ItemFeature CreateChannelMarkup()
        {
            ItemFeature feature = ItemFeatureList.PremiumBuy15();
            feature.identifier = ChannelMarkupId;
            feature.category = ChannelMarkupId;
            string name = Localize("channel_markup");
            feature.SetPublicDisplay(name);
            feature.SetActualDisplay(name);
            feature.SetPreExposedValueMod(Mathf.RoundToInt(Difficulty.Rules.retiredSupplierMarkup * (10f / 15f)));
            return feature;
        }
    }
}
