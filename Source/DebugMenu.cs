using RefinedChemistry.NPC;
using RefinedChemistry.Item;
using UnityEngine.UI;

namespace RefinedChemistry
{
    internal static class DebugMenu
    {
        internal const string WindowId = "refined_chemistry__debug";

        internal static void Open()
        {
            CustomUIManager manager = CustomUIManager.Instance;
            CustomUIWindow window = manager.GetWindow(WindowId);
            if (window != null && window.IsAlive)
            {
                CustomUIWindow parent = manager.GetWindow(CustomUIDebugPanel.WINDOW_ID);
                CopyLayout(parent, window);
                parent.Hide();
                window.Show();
                return;
            }

            var builder = CreateWindow(WindowId, "RefinedChemistry", CustomUIDebugPanel.WINDOW_ID);
            builder.AddButton("Queue Liang Trust 0 Visit", () =>
                DrLiangStory.QueueTrust0(PlayerStore.Instance.storeClientManager));
            builder.AddButton("Queue Liang Trust 20 Visit", () =>
                DrLiangStory.QueueTrust20(PlayerStore.Instance.storeClientManager));
            builder.AddButton("Queue Liang Tool Supplier", () =>
                PlayerStore.Instance.storeClientManager.AddNextClient(DrLiangToolSupplier.Create()));
            builder.AddButton("Queue Liang Material Supplier", () =>
                PlayerStore.Instance.storeClientManager.AddNextClient(DrLiangMaterialSupplier.Create()));
            builder.AddButton("Queue Liang Buyer", () =>
                PlayerStore.Instance.storeClientManager.AddNextClient(DrLiangBuyer.Create()));
            builder.AddButton("Spawn Filled Locked Medical Crate", () =>
                PlayerStore.Instance.AddDirectSellingItemToTable(UtoMedicalBoxLoot.Create(), isOwend: true));
            builder.AddButton("Save Data", SaveDataDebugMenu.Open);
            CustomUIDebugPanel.SetBackgroundAlpha(builder.Show(), 0.8f);
        }

        internal static CustomUIBuilder CreateWindow(string id, string title, string parentId)
        {
            CustomUIManager manager = CustomUIManager.Instance;
            var builder = manager.CreateWindow(id, title, "rear")
                .SetDraggable(true)
                .SetCloseOnEscape();
            CustomUIWindow window = builder.Handle;
            CustomUIWindow parent = manager.GetWindow(parentId);
            CopyLayout(parent, window);
            parent.Hide();
            builder.OnClose(() =>
            {
                CustomUIWindow previous = manager.GetWindow(parentId);
                if (previous != null)
                {
                    CopyLayout(window, previous);
                    previous.Show();
                }
            });
            return builder;
        }

        private static void CopyLayout(CustomUIWindow source, CustomUIWindow target)
        {
            var size = source.Rect.sizeDelta;
            target.SetSize(size.x, size.y);
            target.SetPosition(source.Rect.anchoredPosition);
            var sourceLayout = source.root.GetComponent<VerticalLayoutGroup>();
            var targetLayout = target.root.GetComponent<VerticalLayoutGroup>();
            if (sourceLayout != null && targetLayout != null)
                targetLayout.padding = sourceLayout.padding;
        }
    }
}
