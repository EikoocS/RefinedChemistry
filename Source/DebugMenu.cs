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
            builder.AddButton("Queue Liang First Visit", () =>
                DrLiang.QueueFirstVisit(PlayerStore.Instance.storeClientManager));
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
