using System;

namespace GuiFramework
{
    /// <summary>
    /// UI事件系统
    /// </summary>
    public static class UIEventSystem
    {
        // 面板事件
        public static event Action<string> OnPanelOpened;           // 面板打开
        public static event Action<string> OnPanelClosed;           // 面板关闭
        public static event Action<string> OnPanelFocus;           // 面板获得焦点
        public static event Action<string> OnPanelLostFocus;       // 面板失去焦点
        
        // 堆栈事件
        public static event Action<string> OnPanelPushed;          // 面板入栈
        public static event Action<string> OnPanelPopped;          // 面板出栈
        public static event Action OnStackChanged;                 // 堆栈变化

        public static void TriggerPanelOpened(string panelName) => OnPanelOpened?.Invoke(panelName);
        public static void TriggerPanelClosed(string panelName) => OnPanelClosed?.Invoke(panelName);
        public static void TriggerPanelFocus(string panelName) => OnPanelFocus?.Invoke(panelName);
        public static void TriggerPanelLostFocus(string panelName) => OnPanelLostFocus?.Invoke(panelName);
        public static void TriggerPanelPushed(string panelName) => OnPanelPushed?.Invoke(panelName);
        public static void TriggerPanelPopped(string panelName) => OnPanelPopped?.Invoke(panelName);
        public static void TriggerStackChanged() => OnStackChanged?.Invoke();
        
        // 清理所有事件（在场景切换时调用）
        public static void ClearAllEvents()
        {
            OnPanelOpened = null;
            OnPanelClosed = null;
            OnPanelFocus = null;
            OnPanelLostFocus = null;
            OnPanelPushed = null;
            OnPanelPopped = null;
            OnStackChanged = null;
        }
    }
}