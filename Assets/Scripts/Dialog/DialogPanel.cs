using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuiFramework;

namespace Dialog
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogPanel : BasePanel
    {
        [Header("Dialog UI")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private RectTransform choicesContainer;
        [SerializeField] private Button choicePrefab;
        [SerializeField] private float offsetY = 2f;

        private Transform _target;
        private Camera _mainCamera;

        // choice button pool (shared)
        private static Stack<Button> _choicePool = new Stack<Button>();

        // optional panel pool (shared) - stores inactive DialogPanel instances
        private static Stack<DialogPanel> _panelPool = new Stack<DialogPanel>();
        private static GameObject _fallbackPrefab;

        protected override void Awake()
        {
            base.Awake();
            _mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (_target != null && _mainCamera != null)
            {
                Vector3 targetPosition = _target.position + Vector3.up * offsetY;
                Vector3 screenPoint = _mainCamera.WorldToScreenPoint(targetPosition);
                var rt = transform as RectTransform;
                if (rt != null)
                {
                    rt.position = screenPoint;
                }
            }
        }

        public void SetTarget(Transform t)
        {
            _target = t;
        }

        public void ShowDialog(string speakerName, string text)
        {
            if (speakerNameText != null) speakerNameText.text = speakerName;
            if (dialogText != null) dialogText.text = text;
            gameObject.SetActive(true);
        }

        public void AddChoice(string choiceText, System.Action onChoiceSelected)
        {
            var btn = GetChoiceButton();
            btn.transform.SetParent(choicesContainer, false);
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = choiceText;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onChoiceSelected?.Invoke());
            btn.gameObject.SetActive(true);
        }

        public void ClearChoices()
        {
            if (choicesContainer == null) return;
            for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            {
                var child = choicesContainer.GetChild(i);
                var btn = child.GetComponent<Button>();
                if (btn != null)
                {
                    ReleaseChoiceButton(btn);
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }

        // ========== Choice button pooling ==========
        private Button GetChoiceButton()
        {
            if (_choicePool.Count > 0)
            {
                return _choicePool.Pop();
            }

            if (choicePrefab != null)
            {
                var go = Instantiate(choicePrefab.gameObject);
                return go.GetComponent<Button>();
            }

            // fallback: create a simple button
            var fallback = new GameObject("choiceBtn", typeof(RectTransform), typeof(Button));
            var txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(fallback.transform, false);
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "选项";
            return fallback.GetComponent<Button>();
        }

        private void ReleaseChoiceButton(Button btn)
        {
            btn.onClick.RemoveAllListeners();
            btn.gameObject.SetActive(false);
            btn.transform.SetParent(null);
            _choicePool.Push(btn);
        }

        // ========== Panel pooling (optional fallback) ==========
        public static void RegisterFallbackPrefab(GameObject prefab)
        {
            _fallbackPrefab = prefab;
        }

        public static DialogPanel GetPooledPanel(Transform parent)
        {
            if (_panelPool.Count > 0)
            {
                var p = _panelPool.Pop();
                p.transform.SetParent(parent, false);
                p.gameObject.SetActive(true);
                p.hasRemoved = false;
                return p;
            }

            if (_fallbackPrefab != null)
            {
                var go = GameObject.Instantiate(_fallbackPrefab, parent, false);
                var panel = go.GetComponent<DialogPanel>();
                if (panel != null) return panel;
                // if fallback prefab is old DialogBubble, try to add DialogPanel component at runtime
                var dp = go.AddComponent<DialogPanel>();
                return dp;
            }

            return null;
        }

        public static void ReleasePanelToPool(DialogPanel panel)
        {
            if (panel == null) return;
            panel.ClearChoices();
            panel.gameObject.SetActive(false);
            panel.transform.SetParent(null);
            _panelPool.Push(panel);
        }

        public void Hide()
        {
            HidePanel();
        }
    }
}
