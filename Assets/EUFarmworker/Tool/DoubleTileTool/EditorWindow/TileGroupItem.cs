using UnityEngine;
using UnityEngine.UIElements;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class TileGroupItem : VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<TileGroupItem> {}

        public TileGroupItem()
        {
            _container = Resources.Load<VisualTreeAsset>("DoubleTileTool/TileGroupItem").Instantiate();
            hierarchy.Add(_container);
        }
    }
}
