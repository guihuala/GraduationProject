using System.Collections.Generic;
using EUFarmworker.Tool.DoubleTileTool.Script.Data;
using EUFarmworker.Tool.DoubleTileTool.Script.NoiseGenerator;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace EUFarmworker.Tool.DoubleTileTool.EditorWindow
{
    public class DoubleTileToolEditorWindow : UnityEditor.EditorWindow
    {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField] private DoubleTileViewConfig ViewConfig;

        private string[] _selectNames =
        {
            "基础设置",
            "瓦片设置",
        };
        [MenuItem("EUTool/DoubleTileTool")]
        public static void ShowExample()
        {
            DoubleTileToolEditorWindow wnd = GetWindow<DoubleTileToolEditorWindow>();
            wnd.titleContent = new GUIContent("DoubleTileToolEditorWindow");
            wnd.minSize = new Vector2(1080, 720);
            wnd.maxSize = new Vector2(1080, 720);
            wnd.titleContent = new GUIContent("DoubleTileTool");
        }
        private ObjectField _viewConfig;
        private ObjectField _noiseGeneratorConfig;
        private ListView _selectionListView;
        private Button _tilePathButton;
        private Button _scriptPathButton;
        private TextField _scriptPath;
        private TextField _tilePath;
        private Foldout _tileTypes;
        private Button _addTileTypeButton;
        #region 配置数据文件为空时影响显示的组件

        private VisualElement _scriptPathShow;
        private VisualElement _tilePathShow;
        private VisualElement _noiseConfigShow;
        private VisualElement _tileTypesShow;
        #endregion
        private void GetUI(VisualElement root)
        {
            _selectionListView = root.Q<ListView>("Left");
            _viewConfig = root.Q<ObjectField>("ViewConfig");
            _noiseGeneratorConfig = root.Q<ObjectField>("NoiseConfig");
            _tilePathButton = root.Q<Button>("TilePathButton");
            _scriptPathButton = root.Q<Button>("ScriptPathButton");
            _scriptPath = root.Q<TextField>("ScriptPath");
            _tilePath = root.Q<TextField>("TilePath");
            _tileTypes = root.Q<Foldout>("TileTypes");
            _addTileTypeButton = root.Q<Button>("AddTileTypeButton");
        
            _scriptPathShow = root.Q<VisualElement>("ScriptPathShow");
            _tilePathShow = root.Q<VisualElement>("TilePathShow");
            _noiseConfigShow = root.Q<VisualElement>("NoiseConfigShow");
            _tileTypesShow = root.Q<VisualElement>("TileTypesShow");
        }
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;//获取当前的视觉元素
            m_VisualTreeAsset.CloneTree(root);//复制到视觉树
            GetUI(root);
            #region 选择菜单列表
            _selectionListView.itemsSource = _selectNames; //组件选项的信息
            _selectionListView.fixedItemHeight = 50; //选项组件高度
            _selectionListView.makeItem = SelectionListItem;//生成的组件绑定
            _selectionListView.bindItem = SelectionBindItem;//绑定信息
            _selectionListView.selectedIndex = 0; //默认选中的选项
            _selectionListView.selectedIndicesChanged += selectedIndexChanged;//绑定选择事件

            #endregion

            #region 配置文件

        
            _viewConfig.objectType = typeof(DoubleTileScriptableObject);
            _viewConfig.value = ViewConfig.ConfigData;
            _viewConfig.RegisterValueChangedCallback(ConfigChange);
            ConfigChangeShowVisual(ViewConfig.ConfigData);
        
            _noiseGeneratorConfig.objectType = typeof(DoubleTileNoiseGeneratorBase);
            _noiseGeneratorConfig.value = ViewConfig?.ConfigData?.DoubleTileNoiseGenerator;
            _noiseGeneratorConfig.RegisterValueChangedCallback(NoiseConfigChange);

            #endregion

            #region 路径
            //脚本路径
            _scriptPath.value = ViewConfig?.ConfigData?.ScriptPath ?? "";
            _scriptPath.RegisterValueChangedCallback(ScriptPathChange);
            _scriptPathButton.clickable.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", "", "");
                if(path.Equals("")) return;
                _scriptPath.value = path;
            };
            //瓦片路径
            _tilePath.value = ViewConfig?.ConfigData?.TilePath ?? "";
            _tilePath.RegisterValueChangedCallback(TilePathChange);
            _tilePathButton.clickable.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("选择文件夹", "", "");
                if(path.Equals("")) return;
                _tilePath.value = path;
            };
            #endregion

            #region 类型配置列表
            _addTileTypeButton.clickable.clicked += () =>//添加类型按钮
            {
                if(!ViewConfig.ConfigData) return;
                var ls = new TileTypesItem();
                var lsindex = ViewConfig.ConfigData.TileNames;
                ls.Index = lsindex.Count;
                lsindex.Add($"瓦片{lsindex.Count.ToString()}");
                ls.Name = lsindex[lsindex.Count - 1];
                RegisterValueChange(ls);
                RemoveTileTypeButtonEvent(ls);
                var lsData = new DoubleTileData();
                lsData.TileName = ls.Name;
                ViewConfig.ConfigData.TileDatas.Add(ls.Name,lsData);
                _typesItems.Add(ls);
                _tileTypes.Add(ls);
            };
            TypeConfigInit();
            #endregion
        }


        #region 选择菜单列表具体逻辑

        private void selectedIndexChanged(IEnumerable<int> selectedIndices) //选项方法
        {
            foreach (var index in selectedIndices)
            {
                if (index == 0)
                {
                    Setting();
                }
                else if (index == 1)
                {
                    TileSetting();
                }
            }
        }

        private void Setting() //选项逻辑
        {
            rootVisualElement.Q<VisualElement>("TileSetting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.None,
            };
            rootVisualElement.Q<VisualElement>("Setting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.Flex,
            };
        }

        private void TileSetting() //选项逻辑
        {
            rootVisualElement.Q<VisualElement>("TileSetting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.Flex,
            };
            rootVisualElement.Q<VisualElement>("Setting").style.display = new StyleEnum<DisplayStyle>()
            {
                value = DisplayStyle.None,
            };
        }

        private void SelectionBindItem(VisualElement arg1, int arg2) //选择的组件信息
        {
            var item = arg1 as Label;
            item.text = _selectNames[arg2];
        }

        private VisualElement SelectionListItem() //选择的组件
        {
            var item = new Label();
            item.style.unityTextAlign = TextAnchor.MiddleCenter;
            return item;
        }

        #endregion

        #region 配置文件改变事件

        private void ConfigChange(ChangeEvent<Object> evt)//主文件
        {
            var ls =evt.newValue as DoubleTileScriptableObject;
            ViewConfig.ConfigData = ls;
            TypeConfigInit();
            if (!ViewConfig.ConfigData)
            {
                _noiseGeneratorConfig.value = null;
                _scriptPath.value = "";
                _tilePath.value = "";
                ConfigChangeShowVisual(false);
                return;
            }
            ConfigChangeShowVisual(true);
            _noiseGeneratorConfig.value = ViewConfig.ConfigData?.DoubleTileNoiseGenerator;
            _scriptPath.value = ViewConfig.ConfigData?.ScriptPath;
            _tilePath.value = ViewConfig.ConfigData?.TilePath;
        }

        private void ConfigChangeShowVisual(bool show)
        {
            _scriptPathShow.style.display = new StyleEnum<DisplayStyle>(){value = show? DisplayStyle.Flex:DisplayStyle.None};
            _tilePathShow.style.display = new StyleEnum<DisplayStyle>(){value = show? DisplayStyle.Flex:DisplayStyle.None};
            _noiseConfigShow.style.display = new StyleEnum<DisplayStyle>(){value = show? DisplayStyle.Flex:DisplayStyle.None};
            _tileTypesShow.style.display = new StyleEnum<DisplayStyle>(){value = show? DisplayStyle.Flex:DisplayStyle.None};
        }
        private void NoiseConfigChange(ChangeEvent<Object> evt)//噪声逻辑
        {
            if(!ViewConfig.ConfigData) return;
            var ls = evt.newValue as DoubleTileNoiseGeneratorBase;
            ViewConfig.ConfigData.DoubleTileNoiseGenerator = ls;
            _noiseGeneratorConfig.value = ls;
        }

        private void ScriptPathChange(ChangeEvent<string> evt)
        {
            if(!ViewConfig.ConfigData) return;
            ViewConfig.ConfigData.ScriptPath = evt.newValue;
        }

        private void TilePathChange(ChangeEvent<string> evt)
        {
            if(!ViewConfig.ConfigData) return;
            ViewConfig.ConfigData.TilePath = evt.newValue;
        }
        #endregion

        private readonly List<TileTypesItem> _typesItems = new();
        #region 类型配置列表

        private void TypeConfigInit()
        {
            foreach (var type in _typesItems)
            {
                _tileTypes.Remove(type);
            }
            _typesItems.Clear();
            if (!ViewConfig.ConfigData)
            {
                return;
            }

            for (int i = 0; i < ViewConfig.ConfigData.TileNames.Count; i++)
            {
                var ls = new TileTypesItem();
                ls.Index = i;
                ls.Name = ViewConfig.ConfigData.TileNames[i];
                RegisterValueChange(ls);
                RemoveTileTypeButtonEvent(ls);
                _tileTypes.Add(ls);
                _typesItems.Add(ls);
            }

        }
        private void RegisterValueChange(TileTypesItem ls)
        {
            ls.RegisterValueChangedCallback(v =>
            {
                var ls2 = ls;
                var Name = ViewConfig.ConfigData.TileNames[ls2.Index];
                if(!ViewConfig.ConfigData) return;
                if (ViewConfig.ConfigData.TileDatas.ContainsKey(v.newValue) && Name != v.newValue)
                {
                    ls2.style.backgroundColor = Color.red;
                    return;
                }
                ls2.style.backgroundColor = new StyleColor(new Color(0,0,0,0));
                var lsData = ViewConfig.ConfigData.TileDatas[Name];
                lsData.TileName =  v.newValue;
                ViewConfig.ConfigData.TileDatas.Remove(Name);
                ViewConfig.ConfigData.TileDatas.Add(v.newValue, lsData);
                ViewConfig.ConfigData.TileNames[ls2.Index] =  v.newValue;
            });
        }
        private void RemoveTileTypeButtonEvent(TileTypesItem ls)//删除类型按钮
        {
            ls.AddButtonClick(() =>
            {
                int index = ls.Index;
                var ls2 = ViewConfig.ConfigData.TileNames;
                var ls3 = _typesItems;
                for (int j = index; j < ls2.Count; j++)
                {
                    ls3[j].Index = j - 1;
                }
                ls3.RemoveAt(index);
                ViewConfig.ConfigData.TileDatas.Remove(ls2[index]);
                ls2.RemoveAt(index);
                _tileTypes.RemoveAt(index);
            });
        }

        #endregion
    }
}