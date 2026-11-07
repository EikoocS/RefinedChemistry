using RefinedChemistry.Attributes;
using System.Linq;

namespace RefinedChemistry.NPC
{
    [LocalizableAttributes("npc", "dr_liang")]
    internal static partial class DrLiangDialogue
    {
        internal static bool IsCurrentClientLiang => PlayerStore.HasInstance &&
            DrLiang.IsLiang(PlayerStore.Instance.currentClientInstance?.GetClientBlueprint());

        internal static void SetText(Dialogue dialogue, string key, params object[] args)
        {
            dialogue.SetText(dialogue.title, Localize(key, args));
        }

        internal static Dialogue SetLines(Dialogue dialogue, string title, string key, params object[] args)
        {
            string[] lines = Localize(key, args).Split('\n');
            Dialogue last = dialogue.SetText(title, lines[0]);
            foreach (string line in lines.Skip(1))
                last = last.NextDialogue().SetText(title, line);
            return last;
        }

        internal static void Configure(StoreClient client)
        {
            client.customOnArrestDialogue = StoreClientDialogList.ClientArrestDialog();
            SetText(client.customOnArrestDialogue, "arrest_dialogue");
        }

        internal static void ConfigureTrade(StoreClient client)
        {
            SetText(client.acceptDealDialogue, "deal_accepted_dialogue");
        }
    }
}
