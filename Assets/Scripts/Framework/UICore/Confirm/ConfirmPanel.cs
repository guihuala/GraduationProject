using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace GuiFramework
{
    public class ConfirmPanel : BasePanel
    {
        [Header("对话框组件")]
        public Text titleText;
        public Text messageText;
        public Button confirmButton;
        public Button cancelButton;
        public Text confirmButtonText;
        public Text cancelButtonText;

        private Action<bool> callback;

        protected override void Awake()
        {
            base.Awake();
            
            // 绑定按钮事件
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirm);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
        }

        /// <summary>
        /// 初始化对话框
        /// </summary>
        public void Initialize(string title, string message, string confirmText = "确定", string cancelText = "取消", Action<bool> resultCallback = null)
        {
            if (titleText != null)
                titleText.text = title;
            
            if (messageText != null)
                messageText.text = message;
            
            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;
            
            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;

            // 如果没有取消按钮文本，隐藏取消按钮
            if (cancelButton != null)
                cancelButton.gameObject.SetActive(!string.IsNullOrEmpty(cancelText));

            callback = resultCallback;
        }

        private void OnConfirm()
        {
            callback?.Invoke(true);
            ClosePanel();
        }

        private void OnCancel()
        {
            callback?.Invoke(false);
            ClosePanel();
        }

        // 重写关闭方法，确保正确清理
        public override void ClosePanel()
        {
            // 先清理回调，避免重复调用
            var tempCallback = callback;
            callback = null;

            base.ClosePanel();
            
            // 在面板完全关闭后调用回调
            tempCallback?.Invoke(false);
        }

        // 清理事件监听
        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.onClick.RemoveAllListeners();
            
            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();

            // 确保回调被调用（如果面板被直接销毁）
            if (callback != null)
            {
                callback.Invoke(false);
                callback = null;
            }
        }
    }
}