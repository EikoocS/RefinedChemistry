using RefinedChemistry.Attributes;
using RefinedChemistry.Item;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangToolSupplier
    {
        internal const string ClientId = "dr_liang_tool_supplier";
        private static readonly string[] Stock =
        {
            ImitationPipette.ItemId, AdjustableCylinder.ItemId, Flask.ItemId, Flask.ItemId,
            "chem_module", "chem_module", "evaporator"
        };

        internal static StoreClient Create()
        {
            StoreClient client = DrLiang.CreateClient(ClientId);
            client.clientIntent = StoreClient.ClientIntent.SELL;
            client.clientItemFeatureBuying.Add("premiumBuy15");
            client.mainDialogue.SetText(client.displayName, Localize("sell_dialogue"))
                .SetEndAction(() =>
                {
                    foreach (string itemId in Stock)
                        PlayerStore.Instance.AddDirectSellingItemToTable(itemId);
                });
            client.onItemSold += (_, price) => DrLiangData.AddTradeTrust(price / 20f);
            client.AddBasicDialog();
            DrLiangDialogue.ConfigureTrade(client);
            client.CompleteClientCreation();
            return client;
        }

        internal static void RequestVisit(StorePhoneClient contact)
        {
            PlayerStore.Instance.QueueFuturClient(ClientId, 1);
            contact.UseService();
        }
    }
}
