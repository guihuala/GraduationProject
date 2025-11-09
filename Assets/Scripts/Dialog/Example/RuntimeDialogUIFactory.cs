using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Dialog
{
    /// <summary>
    /// Creates a minimal runtime DialogPanel UI (Canvas + DialogPanel GameObject) and registers it
    /// as the fallback prefab for DialogPanel. Useful for quick demos where no prefab exists.
    /// </summary>
    [DisallowMultipleComponent]
    public class RuntimeDialogUIFactory : MonoBehaviour
    {
        [Tooltip("If true the factory runs on Awake and registers a runtime panel prefab")] [SerializeField]
        private bool createOnAwake = true;

        [Tooltip("Optional name for the runtime Canvas GameObject")] [SerializeField]
        private string canvasName = "RuntimeDialogUICanvas";

        private void Awake()
        {
            if (createOnAwake)
            {
                CreateAndRegister();
            }
        }

        [ContextMenu("Create And Register Runtime Dialog UI")]
        public void CreateAndRegister()
        {
            // If already registered (and cached), skip
            // We'll still create a new one to ensure it's present

            // Create Canvas root
            var canvasGO = new GameObject(canvasName);
            canvasGO.layer = LayerMask.NameToLayer("UI");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create panel root
            var panelGO = new GameObject("DialogPanel_Runtime");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var rect = panelGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 160);

            var bg = panelGO.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            // Add DialogPanel (inherits BasePanel) - ensure component exists
            var dialogPanel = panelGO.AddComponent<DialogPanel>();

            // Speaker Name
            var speakerGO = new GameObject("SpeakerName");
            speakerGO.transform.SetParent(panelGO.transform, false);
            var speakerRT = speakerGO.AddComponent<RectTransform>();
            speakerRT.anchorMin = new Vector2(0, 1);
            speakerRT.anchorMax = new Vector2(1, 1);
            speakerRT.pivot = new Vector2(0.5f, 1f);
            speakerRT.anchoredPosition = new Vector2(0, -8);
            speakerRT.sizeDelta = new Vector2(0, 24);
            var speakerTMP = speakerGO.AddComponent<TextMeshProUGUI>();
            speakerTMP.text = "Speaker";
            speakerTMP.fontSize = 18;
            speakerTMP.alignment = TextAlignmentOptions.MidlineLeft;
            speakerTMP.color = Color.white;

            // Dialog Text
            var textGO = new GameObject("DialogText");
            textGO.transform.SetParent(panelGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0.3f);
            textRT.anchorMax = new Vector2(1, 0.8f);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.anchoredPosition = new Vector2(0, -10);
            textRT.sizeDelta = new Vector2(0, 0);
            var dialogTMP = textGO.AddComponent<TextMeshProUGUI>();
            dialogTMP.text = "Dialog text...";
            dialogTMP.fontSize = 16;
            dialogTMP.alignment = TextAlignmentOptions.TopLeft;
            dialogTMP.color = Color.white;

            // Choices container
            var choicesGO = new GameObject("ChoicesContainer");
            choicesGO.transform.SetParent(panelGO.transform, false);
            var choicesRT = choicesGO.AddComponent<RectTransform>();
            choicesRT.anchorMin = new Vector2(0, 0f);
            choicesRT.anchorMax = new Vector2(1, 0.3f);
            choicesRT.pivot = new Vector2(0.5f, 0f);
            choicesRT.anchoredPosition = new Vector2(0, 8);
            choicesRT.sizeDelta = new Vector2(0, 0);

            // Create a simple Button prefab for choices
            var btnGO = new GameObject("ChoiceButton_Prefab");
            btnGO.layer = LayerMask.NameToLayer("UI");
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(160, 28);
            var btnImage = btnGO.AddComponent<Image>();
            btnImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            var button = btnGO.AddComponent<Button>();

            var labelGO = new GameObject("Text");
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0, 0);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.sizeDelta = new Vector2(0, 0);
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text = "选项";
            labelTMP.color = Color.black;
            labelTMP.alignment = TextAlignmentOptions.Center;

            // Make prefab inactive to be used as template
            btnGO.SetActive(false);

            // Assign private serialized fields via reflection
            System.Type dpType = typeof(DialogPanel);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            // speakerNameText (TextMeshProUGUI)
            var f1 = dpType.GetField("speakerNameText", flags);
            if (f1 != null) f1.SetValue(dialogPanel, speakerTMP);

            // dialogText (TextMeshProUGUI)
            var f2 = dpType.GetField("dialogText", flags);
            if (f2 != null) f2.SetValue(dialogPanel, dialogTMP);

            // choicesContainer (RectTransform)
            var f3 = dpType.GetField("choicesContainer", flags);
            if (f3 != null) f3.SetValue(dialogPanel, choicesRT);

            // choicePrefab (Button)
            var f4 = dpType.GetField("choicePrefab", flags);
            if (f4 != null) f4.SetValue(dialogPanel, button);

            // offsetY
            var f5 = dpType.GetField("offsetY", flags);
            if (f5 != null) f5.SetValue(dialogPanel, 2f);

            // ensure panel is inactive initially
            panelGO.SetActive(false);

            // Register fallback prefab so DialogManager can instantiate it if needed
            DialogPanel.RegisterFallbackPrefab(panelGO);

            Debug.Log("RuntimeDialogUIFactory: Created and registered runtime DialogPanel prefab.");

            // Mark canvas to persist so demo dialogs work across scene loads if desired
            DontDestroyOnLoad(canvasGO);
        }
    }
}

