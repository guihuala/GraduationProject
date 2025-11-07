using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GuiFramework
{
    public class UIManager : SingletonPersistent<UIManager>
    {
        // <面板名称, 面板预制体路径>
        private Dictionary<string, string> _panelPathDict;

        // 缓存的面板预制体 <面板名称, 面板预制体>
        private Dictionary<string, GameObject> _uiPrefabDict;

        // 当前已打开的面板实例 <面板名称, 面板实例>
        private Dictionary<string, BasePanel> _panelDict;

        // UI 面板的根节点
        private Transform _uiRoot;

        // 确认面板预制体缓存
        private GameObject _confirmPanelPrefab;

        public Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    _uiRoot = GameObject.Find("Canvas").transform;
                }

                return _uiRoot;
            }
        }

        public UIDatas uiDatas;
        public ConfirmData confirmData;

        protected override void Awake()
        {
            base.Awake();
            InitDicts();
            LoadConfirmPanelPrefab();
        }

        // 初始化字典
        private void InitDicts()
        {
            _panelPathDict = new Dictionary<string, string>();

            foreach (var data in uiDatas.uiDataList)
            {
                _panelPathDict.Add(data.uiName, data.uiPath);
            }

            _uiPrefabDict = new Dictionary<string, GameObject>();
            _panelDict = new Dictionary<string, BasePanel>();
        }

        // 加载确认面板预制体
        private void LoadConfirmPanelPrefab()
        {
            if (confirmData != null && !string.IsNullOrEmpty(confirmData.confirmPath))
            {
                _confirmPanelPrefab = Resources.Load<GameObject>(confirmData.confirmPath);
                if (_confirmPanelPrefab == null)
                {
                    Debug.LogError($"确认面板预制体未找到：{confirmData.confirmPath}");
                }
            }
            else
            {
                Debug.LogError("确认面板配置数据未设置");
            }
        }

        /// <summary>
        /// 打开UI面板，外部直接调用此方法
        /// </summary>
        /// <param name="name">面板名称</param>
        /// <returns>打开的UI面板脚本</returns>
        public BasePanel OpenPanel(string name)
        {
            BasePanel panel = null;

            // 检查面板是否已经打开
            if (_panelDict.TryGetValue(name, out panel))
            {
                Debug.LogWarning($"面板 {name} 已经打开");
                return null;
            }

            // 检查面板路径是否存在于路径字典中
            string path = "";
            if (!_panelPathDict.TryGetValue(name, out path))
            {
                Debug.LogWarning($"面板 {name} 的路径不存在");
                return null;
            }

            // 从缓存中获取面板预制体
            GameObject panelPrefab = null;
            if (!_uiPrefabDict.TryGetValue(name, out panelPrefab))
            {
                string prefabPath = path;

                panelPrefab = Resources.Load<GameObject>(prefabPath);

                if (panelPrefab == null)
                {
                    Debug.LogError($"面板 {name} 的预制体未找到：{prefabPath}");
                    return null;
                }

                _uiPrefabDict.Add(name, panelPrefab);
            }

            // 实例化面板并将其挂载到 UIRoot
            GameObject panelObj = Instantiate(panelPrefab, UIRoot, false);
            panel = panelObj.GetComponent<BasePanel>();

            if (panel == null)
            {
                Debug.LogError($"面板 {name} 的脚本未挂载或未继承 BasePanel");
                Destroy(panelObj);
                return null;
            }

            panel.OpenPanel(name);
            _panelDict.Add(name, panel);

            return panel;
        }

        /// <summary>
        /// 关闭UI面板，外部直接调用此方法
        /// </summary>
        /// <param name="name">面板名称</param>
        /// <returns>是否关闭成功</returns>
        public bool ClosePanel(string name)
        {
            BasePanel panel = null;

            // 检查面板是否已经打开，未打开则无法关闭
            if (!_panelDict.TryGetValue(name, out panel))
            {
                Debug.LogWarning($"面板 {name} 当前未打开，无法关闭");
                return false;
            }

            _panelDict.Remove(name);
            panel.ClosePanel();

            return true;
        }

        // ========== 确认面板功能 ==========

        /// <summary>
        /// 直接从路径打开确认面板
        /// </summary>
        private ConfirmPanel OpenConfirmPanel()
        {
            if (_confirmPanelPrefab == null)
            {
                Debug.LogError("确认面板预制体未加载");
                return null;
            }

            // 实例化确认面板
            GameObject panelObj = Instantiate(_confirmPanelPrefab, UIRoot, false);
            var confirmPanel = panelObj.GetComponent<ConfirmPanel>();

            if (confirmPanel == null)
            {
                Debug.LogError("确认面板脚本未挂载或未继承 ConfirmPanel");
                Destroy(panelObj);
                return null;
            }

            // 使用唯一名称打开面板，避免与普通面板冲突
            string uniqueName = $"{confirmData.confirmName}_{System.Guid.NewGuid()}";
            confirmPanel.OpenPanel(uniqueName);

            // 不添加到 _panelDict 中，由确认面板自己管理生命周期
            return confirmPanel;
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        /// <param name="title">对话框标题</param>
        /// <param name="message">对话框消息</param>
        /// <param name="confirmText">确认按钮文本</param>
        /// <param name="cancelText">取消按钮文本</param>
        /// <param name="callback">回调函数，参数为用户选择结果</param>
        public void ShowConfirm(string title, string message, string confirmText = "确定", string cancelText = "取消", System.Action<bool> callback = null)
        {
            StartCoroutine(ShowConfirmCoroutine(title, message, confirmText, cancelText, callback));
        }

        private IEnumerator ShowConfirmCoroutine(string title, string message, string confirmText, string cancelText, System.Action<bool> callback)
        {
            yield return null; // 等待一帧

            // 直接从路径打开确认面板
            var confirmPanel = OpenConfirmPanel();
            if (confirmPanel != null)
            {
                confirmPanel.Initialize(title, message, confirmText, cancelText, callback);
            }
            else
            {
                Debug.LogError("无法打开确认面板");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// 显示简单消息对话框（只有确定按钮）
        /// </summary>
        public void ShowMessage(string title, string message, System.Action callback = null)
        {
            ShowConfirm(title, message, "确定", "", (result) =>
            {
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 显示确认对话框（同步方式，使用协程等待结果）
        /// </summary>
        public System.Collections.IEnumerator ShowConfirmAsync(string title, string message, string confirmText = "确定", string cancelText = "取消")
        {
            bool? result = null;
            
            ShowConfirm(title, message, confirmText, cancelText, (r) =>
            {
                result = r;
            });

            // 等待用户做出选择
            while (result == null)
            {
                yield return null;
            }

            yield return result;
        }

        /// <summary>
        /// 重新加载确认面板预制体（用于热重载）
        /// </summary>
        public void ReloadConfirmPanel()
        {
            _confirmPanelPrefab = null;
            LoadConfirmPanelPrefab();
        }
    }
}