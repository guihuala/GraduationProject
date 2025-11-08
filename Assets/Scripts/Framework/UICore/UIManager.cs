// UIManager.cs (完整优化版)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

namespace GuiFramework
{
    public class UIManager : SingletonPersistent<UIManager>
    {
        // 基础字典
        private Dictionary<string, string> _panelPathDict;
        private Dictionary<string, GameObject> _uiPrefabDict;
        private Dictionary<string, BasePanel> _panelDict;
        private Transform _uiRoot;
        private GameObject _confirmPanelPrefab;

        // 面板堆栈管理
        private Stack<PanelStackItem> _panelStack = new Stack<PanelStackItem>();
        
        [System.Serializable]
        private class PanelStackItem
        {
            public string panelName;
            public BasePanel panelInstance;
            public bool isModal; // 是否为模态面板
            public BasePanel parentPanel; // 父面板
            public int stackDepth; // 堆栈深度

            public PanelStackItem(string name, BasePanel panel, bool modal = false, BasePanel parent = null, int depth = 0)
            {
                panelName = name;
                panelInstance = panel;
                isModal = modal;
                parentPanel = parent;
                stackDepth = depth;
            }
        }

        public Transform UIRoot
        {
            get
            {
                if (_uiRoot == null)
                {
                    _uiRoot = GameObject.Find("Canvas").transform;
                    if (_uiRoot == null)
                    {
                        Debug.LogError("Canvas未找到！请确保场景中有名为'Canvas'的GameObject");
                    }
                }
                return _uiRoot;
            }
        }

        public UIDatas uiDatas;
        public ConfirmData confirmData;

        // 堆栈信息属性
        public int StackCount => _panelStack.Count;
        public string TopPanelName => _panelStack.Count > 0 ? _panelStack.Peek().panelName : null;
        public BasePanel TopPanel => _panelStack.Count > 0 ? _panelStack.Peek().panelInstance : null;
        public bool HasModalPanel => _panelStack.Any(item => item.isModal);
        public bool IsStackEmpty => _panelStack.Count == 0;

        protected override void Awake()
        {
            base.Awake();
            InitDicts();
            LoadConfirmPanelPrefab();
        }

        private void InitDicts()
        {
            _panelPathDict = new Dictionary<string, string>();
            if (uiDatas != null && uiDatas.uiDataList != null)
            {
                foreach (var data in uiDatas.uiDataList)
                {
                    if (!_panelPathDict.ContainsKey(data.uiName))
                    {
                        _panelPathDict.Add(data.uiName, data.uiPath);
                    }
                    else
                    {
                        Debug.LogWarning($"重复的面板名称: {data.uiName}");
                    }
                }
            }
            else
            {
                Debug.LogError("UIDatas未配置！");
            }

            _uiPrefabDict = new Dictionary<string, GameObject>();
            _panelDict = new Dictionary<string, BasePanel>();
        }

        private void LoadConfirmPanelPrefab()
        {
            if (confirmData != null && !string.IsNullOrEmpty(confirmData.confirmPath))
            {
                _confirmPanelPrefab = Resources.Load<GameObject>(confirmData.confirmPath);
                if (_confirmPanelPrefab == null)
                {
                    Debug.LogError($"确认面板预制体未找到：{confirmData.confirmPath}");
                }
                else
                {
                    Debug.Log("确认面板预制体加载成功");
                }
            }
            else
            {
                Debug.LogError("确认面板配置数据未设置");
            }
        }

        // ========== 基础面板功能 ==========

        /// <summary>
        /// 打开UI面板（不加入堆栈）
        /// </summary>
        public BasePanel OpenPanel(string name)
        {
            return OpenPanelInternal(name, false, false, null);
        }

        /// <summary>
        /// 打开UI面板并推入堆栈
        /// </summary>
        /// <param name="name">面板名称</param>
        /// <param name="isModal">是否为模态面板</param>
        /// <param name="parentPanel">父面板</param>
        /// <returns>打开的UI面板脚本</returns>
        public BasePanel OpenPanel(string name, bool isModal, BasePanel parentPanel = null)
        {
            return OpenPanelInternal(name, true, isModal, parentPanel);
        }

        /// <summary>
        /// 打开UI面板并推入堆栈（非模态）
        /// </summary>
        public BasePanel OpenPanel(string name, BasePanel parentPanel = null)
        {
            return OpenPanelInternal(name, true, false, parentPanel);
        }

        private BasePanel OpenPanelInternal(string name, bool addToStack, bool isModal = false, BasePanel parentPanel = null)
        {
            // 检查UIRoot
            if (UIRoot == null)
            {
                Debug.LogError("无法找到UIRoot！");
                return null;
            }

            BasePanel panel = null;

            // 检查面板是否已经打开
            if (_panelDict.TryGetValue(name, out panel))
            {
                Debug.LogWarning($"面板 {name} 已经打开");
                
                // 如果已经在堆栈中，将其移到栈顶
                if (addToStack)
                {
                    BringPanelToTop(name);
                }
                return panel;
            }

            // 检查面板路径
            if (!_panelPathDict.TryGetValue(name, out string path))
            {
                Debug.LogWarning($"面板 {name} 的路径不存在");
                return null;
            }

            // 加载面板预制体
            if (!_uiPrefabDict.TryGetValue(name, out GameObject panelPrefab))
            {
                panelPrefab = Resources.Load<GameObject>(path);
                if (panelPrefab == null)
                {
                    Debug.LogError($"面板 {name} 的预制体未找到：{path}");
                    return null;
                }
                _uiPrefabDict.Add(name, panelPrefab);
                Debug.Log($"面板预制体加载成功: {name}");
            }

            // 实例化面板
            GameObject panelObj = Instantiate(panelPrefab, UIRoot, false);
            if (panelObj == null)
            {
                Debug.LogError($"面板 {name} 实例化失败");
                return null;
            }

            panel = panelObj.GetComponent<BasePanel>();
            if (panel == null)
            {
                Debug.LogError($"面板 {name} 的脚本未挂载或未继承 BasePanel");
                Destroy(panelObj);
                return null;
            }

            // 设置父子关系
            if (parentPanel != null)
            {
                panel.SetParentPanel(parentPanel);
            }

            // 打开面板
            panel.OpenPanel(name);
            _panelDict.Add(name, panel);

            // 推入堆栈
            if (addToStack)
            {
                PushPanelToStack(name, panel, isModal, parentPanel);
            }

            return panel;
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public bool ClosePanel(string name)
        {
            if (!_panelDict.TryGetValue(name, out BasePanel panel))
            {
                Debug.LogWarning($"面板 {name} 当前未打开，无法关闭");
                return false;
            }

            // 从堆栈中移除
            RemovePanelFromStack(name);

            _panelDict.Remove(name);
            panel.ClosePanel();

            return true;
        }

        /// <summary>
        /// 立即关闭UI面板（无动画）
        /// </summary>
        public bool ClosePanelImmediate(string name)
        {
            if (!_panelDict.TryGetValue(name, out BasePanel panel))
            {
                Debug.LogWarning($"面板 {name} 当前未打开，无法关闭");
                return false;
            }

            // 从堆栈中移除
            RemovePanelFromStack(name);

            _panelDict.Remove(name);
            panel.ClosePanelImmediate();

            return true;
        }

        /// <summary>
        /// 关闭所有面板
        /// </summary>
        public void CloseAllPanels()
        {
            var panelNames = _panelDict.Keys.ToList();
            foreach (var name in panelNames)
            {
                ClosePanelImmediate(name);
            }
            _panelStack.Clear();
            UIEventSystem.TriggerStackChanged();
        }

        /// <summary>
        /// 获取面板实例
        /// </summary>
        public T GetPanel<T>(string name) where T : BasePanel
        {
            if (_panelDict.TryGetValue(name, out BasePanel panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 检查面板是否打开
        /// </summary>
        public bool IsPanelOpen(string name)
        {
            return _panelDict.ContainsKey(name);
        }

        // ========== 堆栈管理功能 ==========

        /// <summary>
        /// 将面板推入堆栈
        /// </summary>
        private void PushPanelToStack(string name, BasePanel panel, bool isModal, BasePanel parentPanel)
        {
            // 当前栈顶面板失去焦点
            if (_panelStack.Count > 0)
            {
                var topItem = _panelStack.Peek();
                topItem.panelInstance?.OnPanelLostFocus();
            }

            var newItem = new PanelStackItem(name, panel, isModal, parentPanel, _panelStack.Count + 1);
            _panelStack.Push(newItem);
            
            // 新面板获得焦点
            panel.OnPanelFocus();
            
            // 触发事件
            UIEventSystem.TriggerPanelPushed(name);
            UIEventSystem.TriggerStackChanged();

            Debug.Log($"面板推入堆栈: {name}, 当前堆栈深度: {_panelStack.Count}");
        }

        /// <summary>
        /// 从堆栈中弹出顶部面板
        /// </summary>
        public bool PopPanel()
        {
            if (_panelStack.Count == 0)
            {
                Debug.LogWarning("堆栈为空，无法弹出面板");
                return false;
            }

            var topItem = _panelStack.Pop();
            string poppedName = topItem.panelName;
            
            Debug.Log($"弹出面板: {poppedName}");

            // 关闭面板
            if (_panelDict.ContainsKey(poppedName))
            {
                _panelDict.Remove(poppedName);
                topItem.panelInstance.ClosePanel();
            }

            // 新的栈顶面板获得焦点
            if (_panelStack.Count > 0)
            {
                var newTopItem = _panelStack.Peek();
                newTopItem.panelInstance?.OnPanelFocus();
            }

            // 触发事件
            UIEventSystem.TriggerPanelPopped(poppedName);
            UIEventSystem.TriggerStackChanged();

            return true;
        }

        /// <summary>
        /// 弹出到指定面板（关闭所有在它上面的面板）
        /// </summary>
        public bool PopToPanel(string targetPanelName)
        {
            if (!_panelStack.Any(item => item.panelName == targetPanelName))
            {
                Debug.LogWarning($"面板 {targetPanelName} 不在堆栈中");
                return false;
            }

            bool found = false;
            int popCount = 0;

            while (_panelStack.Count > 0 && !found)
            {
                var topItem = _panelStack.Peek();
                if (topItem.panelName == targetPanelName)
                {
                    found = true;
                    Debug.Log($"弹出到面板: {targetPanelName}, 共关闭了 {popCount} 个面板");
                }
                else
                {
                    PopPanel();
                    popCount++;
                }
            }

            return found;
        }

        /// <summary>
        /// 弹出所有面板直到堆栈为空
        /// </summary>
        public void PopAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                PopPanel();
            }
        }

        /// <summary>
        /// 将指定面板移到栈顶
        /// </summary>
        public bool BringPanelToTop(string panelName)
        {
            if (!_panelStack.Any(item => item.panelName == panelName))
            {
                Debug.LogWarning($"面板 {panelName} 不在堆栈中");
                return false;
            }

            var tempStack = new Stack<PanelStackItem>();
            PanelStackItem targetItem = null;

            // 查找目标面板
            while (_panelStack.Count > 0)
            {
                var item = _panelStack.Pop();
                if (item.panelName == panelName)
                {
                    targetItem = item;
                    break;
                }
                else
                {
                    tempStack.Push(item);
                }
            }

            if (targetItem == null) return false;

            // 恢复其他面板
            while (tempStack.Count > 0)
            {
                _panelStack.Push(tempStack.Pop());
            }

            // 将目标面板推回栈顶
            _panelStack.Push(targetItem);

            // 更新焦点
            UpdateStackFocus();

            UIEventSystem.TriggerStackChanged();
            Debug.Log($"面板移到栈顶: {panelName}");
            return true;
        }

        /// <summary>
        /// 从堆栈中移除面板（但不关闭）
        /// </summary>
        private void RemovePanelFromStack(string panelName)
        {
            var tempStack = new Stack<PanelStackItem>();
            bool found = false;

            while (_panelStack.Count > 0)
            {
                var item = _panelStack.Pop();
                if (item.panelName == panelName)
                {
                    found = true;
                    break;
                }
                tempStack.Push(item);
            }

            // 恢复其他面板
            while (tempStack.Count > 0)
            {
                _panelStack.Push(tempStack.Pop());
            }

            if (found)
            {
                UpdateStackFocus();
                UIEventSystem.TriggerStackChanged();
                Debug.Log($"从堆栈移除面板: {panelName}");
            }
        }

        /// <summary>
        /// 更新堆栈焦点
        /// </summary>
        private void UpdateStackFocus()
        {
            if (_panelStack.Count > 0)
            {
                var topItem = _panelStack.Peek();
                topItem.panelInstance?.OnPanelFocus();
            }
        }

        /// <summary>
        /// 获取堆栈信息（用于调试）
        /// </summary>
        public string GetStackInfo()
        {
            var stackArray = _panelStack.ToArray();
            System.Array.Reverse(stackArray);
            
            string info = $"堆栈深度: {stackArray.Length}\n";
            for (int i = 0; i < stackArray.Length; i++)
            {
                var item = stackArray[i];
                info += $"{i + 1}. {item.panelName} {(item.isModal ? "[模态]" : "")} (深度:{item.stackDepth})\n";
            }
            return info;
        }

        /// <summary>
        /// 打印堆栈信息到控制台
        /// </summary>
        public void PrintStackInfo()
        {
            Debug.Log(GetStackInfo());
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
        public void ShowConfirm(string title, string message, string confirmText = "确定", string cancelText = "取消",
            System.Action<bool> callback = null)
        {
            StartCoroutine(ShowConfirmCoroutine(title, message, confirmText, cancelText, callback));
        }

        private IEnumerator ShowConfirmCoroutine(string title, string message, string confirmText, string cancelText,
            System.Action<bool> callback)
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
            ShowConfirm(title, message, "确定", "", (result) => { callback?.Invoke(); });
        }

        /// <summary>
        /// 显示确认对话框（同步方式，使用协程等待结果）
        /// </summary>
        public System.Collections.IEnumerator ShowConfirmAsync(string title, string message, string confirmText = "确定",
            string cancelText = "取消")
        {
            bool? result = null;

            ShowConfirm(title, message, confirmText, cancelText, (r) => { result = r; });

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

        // ========== 资源管理 ==========

        /// <summary>
        /// 清理未使用的面板预制体
        /// </summary>
        public void CleanupUnusedPrefabs()
        {
            var unusedKeys = _uiPrefabDict.Keys.Except(_panelDict.Keys).ToList();
            foreach (var key in unusedKeys)
            {
                _uiPrefabDict.Remove(key);
            }
            Resources.UnloadUnusedAssets();
            Debug.Log($"清理了 {unusedKeys.Count} 个未使用的面板预制体");
        }

        /// <summary>
        /// 预加载面板预制体
        /// </summary>
        public void PreloadPanel(string name)
        {
            if (_uiPrefabDict.ContainsKey(name))
            {
                Debug.Log($"面板 {name} 已经预加载");
                return;
            }

            if (_panelPathDict.TryGetValue(name, out string path))
            {
                var prefab = Resources.Load<GameObject>(path);
                if (prefab != null)
                {
                    _uiPrefabDict.Add(name, prefab);
                    Debug.Log($"面板预加载成功: {name}");
                }
                else
                {
                    Debug.LogError($"面板预加载失败: {name}, 路径: {path}");
                }
            }
            else
            {
                Debug.LogError($"面板路径不存在: {name}");
            }
        }

        protected virtual void OnDestroy()
        {
            // 清理事件系统
            UIEventSystem.ClearAllEvents();
        }

        // ========== 编辑器调试方法 ==========
        
        #if UNITY_EDITOR
        [ContextMenu("打印堆栈信息")]
        private void DebugPrintStackInfo()
        {
            PrintStackInfo();
        }

        [ContextMenu("清理未使用资源")]
        private void DebugCleanupUnused()
        {
            CleanupUnusedPrefabs();
        }
        #endif
    }
}