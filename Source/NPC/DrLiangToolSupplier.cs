using RefinedChemistry.Attributes;
using RefinedChemistry.Item;
using System.Collections.Generic;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangToolSupplier
    {
        internal const string ClientId = "dr_liang_tool_supplier";

        internal static StoreClient Create()
        {
            StoreClient client = DrLiang.CreateClient(ClientId);
            client.clientIntent = StoreClient.ClientIntent.SELL;
            client.clientItemFeatureBuying.Add("premiumBuy15");
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
            float trust = DrLiangData.Trust;
            var ownedItems = EmporiumEntry.Instance.GetAllOwnedItems();
            bool hasPipette = ownedItems.Exists(item => item.identifier == Pipette.ItemId);
            if (trust >= 20 && !hasPipette)
                yield return Pipette.ItemId;

            if (!hasPipette)
                yield return ImitationPipette.ItemId;
            yield return AdjustableCylinder.ItemId;
            if (trust >= 20)
            {
                if (!ownedItems.Exists(item => item.identifier == ChemFactory.ItemId))
                    yield return ChemFactory.ItemId;
                yield return ChemicalTank.ItemId;
                if (!ownedItems.Exists(item => item.identifier == LabelerFactory.ItemId))
                    yield return LabelerFactory.ItemId;
                if (!ownedItems.Exists(item => item.identifier == AdvancedChemScanner.ItemId))
                    yield return AdvancedChemScanner.ItemId;
            }
            yield return Flask.ItemId;
            yield return Flask.ItemId;
        }

        internal static void RequestVisit(StorePhoneClient contact)
        {
            PlayerStore.Instance.QueueFuturClient(ClientId, 1);
            contact.UseService();
        }
    }
}
