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
        [SerializeField] private Text speakerNameText;
        [SerializeField] private Text dialogText;
        // Optional TMP fallbacks (in case runtime factory uses TextMeshPro)
        [SerializeField] private TextMeshProUGUI speakerNameTMP;
        [SerializeField] private TextMeshProUGUI dialogTextTMP;

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
            // Prefer Text if assigned, otherwise try TMP fallback
            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName;
                Debug.Log($"DialogPanel: set speakerNameText to '{speakerName}'");
            }
            else if (speakerNameTMP != null)
            {
                speakerNameTMP.text = speakerName;
                Debug.Log($"DialogPanel: set speakerNameTMP to '{speakerName}'");
            }
            else
            {
                Debug.LogWarning("DialogPanel: no speaker name UI component assigned (Text or TMP)");
            }

            if (dialogText != null)
            {
                dialogText.text = text;
                Debug.Log($"DialogPanel: set dialogText to '{text}' (Text)");
            }
            else if (dialogTextTMP != null)
            {
                dialogTextTMP.text = text;
                Debug.Log($"DialogPanel: set dialogTextTMP to '{text}' (TMP)");
            }
            else
            {
                Debug.LogWarning("DialogPanel: no dialog text UI component assigned (Text or TMP)");
            }

            gameObject.SetActive(true);
        }

        public void AddChoice(string choiceText, System.Action onChoiceSelected)
        {
            var btn = GetChoiceButton();
            btn.transform.SetParent(choicesContainer, false);

            // Try to set TMP label first, then fall back to UnityEngine.UI.Text
            var tmpLabel = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpLabel != null)
            {
                tmpLabel.text = choiceText;
                Debug.Log($"DialogPanel: added TMP choice '{choiceText}'");
            }
            else
            {
                var uiLabel = btn.GetComponentInChildren<Text>();
                if (uiLabel != null)
                {
                    uiLabel.text = choiceText;
                    Debug.Log($"DialogPanel: added UI.Text choice '{choiceText}'");
                }
                else
                {
                    Debug.LogWarning("DialogPanel: added choice but no label component found on button");
                }
            }

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

            // fallback: create a simple button with UI.Text label
            var fallback = new GameObject("choiceBtn", typeof(RectTransform), typeof(Button), typeof(Image));
            var txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(fallback.transform, false);
            var text = txtObj.AddComponent<Text>();
            text.text = "选项";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            var img = fallback.GetComponent<Image>();
            img.color = new Color(0.9f, 0.9f, 0.9f, 1f);
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
