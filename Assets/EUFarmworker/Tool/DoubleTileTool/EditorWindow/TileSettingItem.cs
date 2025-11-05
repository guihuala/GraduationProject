using UnityEngine;
using UnityEngine.UIElements;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class TileSettingItem : VisualElement
    {
        private TemplateContainer _container;
        public new class UxmlFactory : UxmlFactory<TileSettingItem> {}

        public TileSettingItem()
        {
            _container = Resources.Load<VisualTreeAsset>("DoubleTileTool/TileSettingItem").Instantiate();
            hierarchy.Add(_container);
        }
    }
}
