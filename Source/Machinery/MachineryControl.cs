using TMPro;
using UnityEngine;

namespace RefinedChemistry.Machinery
{
    internal sealed class MachineryControl : PixelElement
    {
        internal TMP_InputField input { get; }
        internal TMP_Dropdown dropdown { get; }
        public bool canResize => false;
        public int widthPixels { get; }
        public int heightPixels => 18;
        public TreeNodeRender handler { get; }
        public RectTransform rectTransform => handler.rectTransform;
        public TreeNodeRender inner => handler;

        internal MachineryControl(CustomUIManager.ElementKind kind, int width = 130)
        {
            widthPixels = width;
            handler = TreeNodeRender.Instantiate(this, "MachineryControl");
            handler.image.enabled = false;
            var root = CustomUIManager.Instance.SpawnElement(kind, handler.transform, null, kind.ToString());
            CustomUIMachineryStyle.Reskin(root);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(widthPixels * 2, heightPixels * 2);
            rect.localScale = new Vector3(0.5f, 0.5f, 1);
            input = root.GetComponentInChildren<TMP_InputField>(true);
            dropdown = root.GetComponentInChildren<TMP_Dropdown>(true);
            CustomUIElement.Bind(root, kind.ToString());
            Validate();
        }

        public void Validate() => rectTransform.sizeDelta = new Vector2(widthPixels, heightPixels);
        public void ResizePixels(int width, int height) => Validate();
        public int WidthPixelsClamp(int width) => widthPixels;
        public int HeightPixelsClamp(int height) => heightPixels;
        public int XClamp(int x) => 0;
        public int YClamp(int y) => 0;
        public bool Attach(PixelElement element) => false;
        public bool Detach(PixelElement element) => false;
    }
}
