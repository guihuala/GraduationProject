using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace GuiFramework
{
    /// <summary>
    /// UI面板的基类
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePanel : MonoBehaviour
    {
        protected bool hasRemoved = false;
        protected string panelName;
        protected CanvasGroup canvasGroup;
        
        // 父子关系管理
        protected BasePanel parentPanel;
        protected List<BasePanel> childPanels = new List<BasePanel>();
        
        // 面板层级信息
        public int PanelDepth { get; protected set; }
        public bool IsModal { get; protected set; } // 是否为模态面板（阻塞下层交互）

        [Header("动画设置")]
        [SerializeField] protected float fadeInDuration = 0.5f;
        [SerializeField] protected float fadeOutDuration = 0.5f;
        [SerializeField] protected Ease fadeInEase = Ease.OutQuad;
        [SerializeField] protected Ease fadeOutEase = Ease.InQuad;
        [SerializeField] protected bool scaleAnimation = true;
        [SerializeField] protected Vector2 scaleFrom = new Vector2(0.8f, 0.8f);
        [SerializeField] protected float scaleDuration = 0.3f;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        public virtual void OpenPanel(string name)
        {
            panelName = name;
            gameObject.SetActive(true);

            canvasGroup.alpha = 0;
            if (scaleAnimation)
            {
                transform.localScale = scaleFrom;
            }

            Sequence s = DOTween.Sequence();
            s.SetUpdate(UpdateType.Normal, true);

            s.Append(canvasGroup.DOFade(1, fadeInDuration)
                .SetEase(fadeInEase)
                .SetUpdate(UpdateType.Normal, true));

            if (scaleAnimation)
            {
                s.Join(transform.DOScale(Vector3.one, scaleDuration)
                    .SetEase(fadeInEase)
                    .SetUpdate(UpdateType.Normal, true));
            }
            
            // 触发打开事件
            UIEventSystem.TriggerPanelOpened(panelName);
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public virtual void ClosePanel()
        {
            if (hasRemoved) return;
            hasRemoved = true;

            // 先关闭所有子面板
            CloseAllChildPanels();

            // 触发关闭事件
            UIEventSystem.TriggerPanelClosed(panelName);

            Sequence s = DOTween.Sequence();
            s.SetUpdate(UpdateType.Normal, true);

            s.Append(canvasGroup.DOFade(0, fadeOutDuration)
                .SetEase(fadeOutEase)
                .SetUpdate(UpdateType.Normal, true));

            if (scaleAnimation)
            {
                s.Join(transform.DOScale(scaleFrom, Mathf.Min(fadeOutDuration, scaleDuration))
                    .SetEase(fadeOutEase)
                    .SetUpdate(UpdateType.Normal, true));
            }

            s.OnComplete(() =>
            {
                if (gameObject != null)
                {
                    Destroy(gameObject);
                }
            });
        }

        /// <summary>
        /// 立即关闭面板（无动画）
        /// </summary>
        public virtual void ClosePanelImmediate()
        {
            if (hasRemoved) return;
            hasRemoved = true;

            CloseAllChildPanels();
            UIEventSystem.TriggerPanelClosed(panelName);
            
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 设置父面板
        /// </summary>
        public virtual void SetParentPanel(BasePanel parent)
        {
            this.parentPanel = parent;
            if (parent != null)
            {
                parent.AddChildPanel(this);
                UpdateDepth();
            }
        }

        /// <summary>
        /// 添加子面板
        /// </summary>
        public virtual void AddChildPanel(BasePanel child)
        {
            if (!childPanels.Contains(child))
            {
                childPanels.Add(child);
                child.SetParentPanel(this);
            }
        }

        /// <summary>
        /// 移除子面板
        /// </summary>
        public virtual void RemoveChildPanel(BasePanel child)
        {
            if (childPanels.Contains(child))
            {
                childPanels.Remove(child);
                child.parentPanel = null;
            }
        }

        /// <summary>
        /// 关闭所有子面板
        /// </summary>
        public virtual void CloseAllChildPanels()
        {
            for (int i = childPanels.Count - 1; i >= 0; i--)
            {
                var child = childPanels[i];
                if (child != null)
                {
                    child.ClosePanelImmediate();
                }
            }
            childPanels.Clear();
        }

        /// <summary>
        /// 更新面板深度（用于父子层级）
        /// </summary>
        protected virtual void UpdateDepth()
        {
            if (parentPanel != null)
            {
                PanelDepth = parentPanel.PanelDepth + 1;
                // 可以在这里设置Canvas的sorting order等
            }
            else
            {
                PanelDepth = 0;
            }

            // 更新所有子面板的深度
            foreach (var child in childPanels)
            {
                child.UpdateDepth();
            }
        }

        /// <summary>
        /// 设置面板交互状态
        /// </summary>
        public virtual void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }

        /// <summary>
        /// 面板获得焦点（当成为栈顶面板时调用）
        /// </summary>
        public virtual void OnPanelFocus()
        {
            SetInteractable(true);
            UIEventSystem.TriggerPanelFocus(panelName);
        }

        /// <summary>
        /// 面板失去焦点（当被其他面板覆盖时调用）
        /// </summary>
        public virtual void OnPanelLostFocus()
        {
            // 模态面板保持交互，非模态面板禁用交互
            if (!IsModal)
            {
                SetInteractable(false);
            }
            UIEventSystem.TriggerPanelLostFocus(panelName);
        }

        protected virtual void OnDestroy()
        {
            // 从父面板中移除自己
            parentPanel?.RemoveChildPanel(this);
        }
    }
}