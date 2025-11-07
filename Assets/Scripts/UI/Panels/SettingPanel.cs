using System.Linq;
using GuiFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SettingsPanel : BasePanel
{
    [Header("组件配置")] public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("多语言")] public TMP_Dropdown languageDropdown;

    [Header("导航/页签")] public Button tabBasicButton;
    public Button tabBindingsButton;
    public GameObject basicPage;
    public GameObject bindingsPage;

    [Header("键位设置")] public Transform keybindingContainer;
    public GameObject keybindingItemPrefab;
    public Button resetAllBindingsButton;
    public Text keybindingHintText;

    [Header("按钮")] public Button backButton;

    private PlayerInputActions inputActions;
    private bool settingsChanged = false;

    private void Start()
    {
        InitializeSettings();
        InitializeUIEvents();
        InitializeKeybindings();

        if (inputActions != null)
        {
            InitKeybindingUI();
        }

        // 初始提示
        if (keybindingHintText != null)
        {
            keybindingHintText.text = "点击任意动作旁边的按钮来重新绑定键位";
        }
    }

    private void InitializeSettings()
    {
        // 音量设置
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.value = AudioManager.Instance.bgmVolumeFactor;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolumeFactor;

        // 语言设置
        InitLanguageDropdown();
    }

    private void InitializeUIEvents()
    {
        // 音量滑块事件
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(ChangeBgmVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(ChangeSfxVolume);

        // 按钮事件
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        // 页签切换
        if (tabBasicButton != null)
            tabBasicButton.onClick.AddListener(() => SwitchPage(true));

        if (tabBindingsButton != null)
            tabBindingsButton.onClick.AddListener(() => SwitchPage(false));

        // 重置所有绑定
        if (resetAllBindingsButton != null)
            resetAllBindingsButton.onClick.AddListener(ResetAllBindings);

        // 默认显示基础设置页
        SwitchPage(true);

        // 标记设置未改变
        settingsChanged = false;
    }

    private void InitializeKeybindings()
    {
        try
        {
            inputActions = new PlayerInputActions();
            KeybindingItem.LoadAllBindings(inputActions);
            InitKeybindingUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"初始化键位设置失败: {e.Message}");
        }
    }

    private void InitKeybindingUI()
    {
        if (keybindingContainer == null || keybindingItemPrefab == null || inputActions == null)
        {
            Debug.LogWarning("键位设置UI组件未完全配置");
            return;
        }

        // 清空旧条目
        foreach (Transform child in keybindingContainer)
        {
            Destroy(child.gameObject);
        }

        // 按 Action Map 分组显示
        foreach (var map in inputActions.asset.actionMaps)
        {
            // 跳过空的动作组
            if (map.actions.Count == 0) continue;

            // 添加分组标题
            CreateGroupHeader(map.name);

            // 添加动作条目
            foreach (var action in map.actions)
            {
                if (action.bindings.Count > 0 && !action.name.StartsWith("UI_"))
                {
                    CreateAllBindingItems(action);
                }
            }
        }
    }

    private void CreateAllBindingItems(InputAction action)
    {
        var bindingItems = GetBindingItemsForAction(action);

        // 为每个绑定创建独立的条目
        foreach (var item in bindingItems)
        {
            var itemGO = Instantiate(keybindingItemPrefab, keybindingContainer);
            var keybindingItem = itemGO.GetComponent<KeybindingItem>();
            if (keybindingItem != null)
            {
                keybindingItem.Init(action, item.bindingIndex, item.displayName);
            }
            else
            {
                Debug.LogWarning($"KeybindingItem 组件在预制体 {keybindingItemPrefab.name} 上未找到");
            }
        }
    }

    private List<BindingItem> GetBindingItemsForAction(InputAction action)
    {
        var items = new List<BindingItem>();
        var compositeGroups = new Dictionary<string, List<(int index, string partName)>>();

        // 首先收集所有复合绑定及其部分
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite)
            {
                // 复合绑定开始
                string compositeName = LocalizationManager.GetCompositeDisplayName(binding.name);
                compositeGroups[compositeName] = new List<(int, string)>();
            }
            else if (binding.isPartOfComposite && compositeGroups.Count > 0)
            {
                // 复合绑定的部分
                var lastComposite = compositeGroups.Last();
                string partName = LocalizationManager.GetCompositePartName(binding.name);
                lastComposite.Value.Add((i, partName));
            }
            else if (!binding.isComposite && !binding.isPartOfComposite)
            {
                // 普通绑定（如手柄摇杆、鼠标等）
                string deviceType = LocalizationManager.GetDeviceTypeFromBinding(binding.path);
                string displayName = $"{LocalizationManager.GetActionName(action.name)} - {deviceType}";
                items.Add(new BindingItem
                {
                    bindingIndex = i,
                    displayName = displayName
                });
            }
        }

        // 为每个复合绑定的每个部分创建独立条目
        foreach (var compositeGroup in compositeGroups)
        {
            string compositeName = compositeGroup.Key;
            foreach (var part in compositeGroup.Value)
            {
                string displayName = $"{LocalizationManager.GetActionName(action.name)} - {compositeName} - {part.partName}";
                items.Add(new BindingItem
                {
                    bindingIndex = part.index,
                    displayName = displayName
                });
            }
        }

        return items;
    }

    // 辅助类用于存储绑定项信息
    private class BindingItem
    {
        public int bindingIndex;
        public string displayName;
    }

    private void CreateGroupHeader(string headerText)
    {
        var headerGO = new GameObject("Header_" + headerText);
        headerGO.transform.SetParent(keybindingContainer);

        // 添加布局元素
        var layoutElement = headerGO.AddComponent<LayoutElement>();
        layoutElement.minHeight = 40;

        // 添加水平布局
        var horizontalLayout = headerGO.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayout.padding = new RectOffset(10, 0, 5, 5);

        // 创建文本组件
        var textGO = new GameObject("HeaderText");
        textGO.transform.SetParent(headerGO.transform);

        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = LocalizationManager.GetMapName(headerText);
        text.fontSize = 18;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.8f, 0.8f, 1f); // 浅蓝色

        var textLayout = textGO.AddComponent<LayoutElement>();
        textLayout.flexibleWidth = 1;
    }

    private void ResetAllBindings()
    {
        if (inputActions == null) return;

        UIManager.Instance.ShowConfirm(
            "重置所有键位",
            "确定要将所有键位重置为默认设置吗？此操作不可撤销。",
            "确定重置",
            "取消",
            (confirmed) =>
            {
                if (confirmed)
                {
                    PerformResetAllBindings();
                }
            }
        );
    }

    private void PerformResetAllBindings()
    {
        foreach (var map in inputActions.asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.RemoveAllBindingOverrides();
                string key = $"Rebind_{map.name}_{action.name}";
                if (PlayerPrefs.HasKey(key))
                    PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.Save();
        InitKeybindingUI();
        ShowTempMessage("所有键位已重置为默认设置", 2f);
        settingsChanged = true;
    }

    private void ShowTempMessage(string message, float duration)
    {
        if (keybindingHintText != null)
        {
            StartCoroutine(ShowTempMessageCoroutine(message, duration));
        }
    }

    private IEnumerator ShowTempMessageCoroutine(string message, float duration)
    {
        string originalText = keybindingHintText.text;
        keybindingHintText.text = $"<color=green>{message}</color>";
        keybindingHintText.color = Color.green;

        yield return new WaitForSeconds(duration);

        keybindingHintText.text = originalText;
        keybindingHintText.color = Color.white;
    }

    private void SwitchPage(bool showBasic)
    {
        if (basicPage != null)
            basicPage.SetActive(showBasic);

        if (bindingsPage != null)
            bindingsPage.SetActive(!showBasic);

        // 更新按钮交互状态
        if (tabBasicButton != null)
            tabBasicButton.interactable = !showBasic;

        if (tabBindingsButton != null)
            tabBindingsButton.interactable = showBasic;

        // 更新按钮样式（可选）
        UpdateTabAppearance(tabBasicButton, showBasic);
        UpdateTabAppearance(tabBindingsButton, !showBasic);
    }

    private void UpdateTabAppearance(Button tabButton, bool isActive)
    {
        if (tabButton == null) return;

        var colors = tabButton.colors;
        colors.normalColor = isActive ? new Color(0.2f, 0.4f, 0.8f) : Color.gray;
        colors.selectedColor = new Color(0.2f, 0.6f, 1f);
        tabButton.colors = colors;

        // 更新文本颜色
        var text = tabButton.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.color = isActive ? new Color(0.9f, 0.9f, 1f) : new Color(0.7f, 0.7f, 0.7f);
        }
    }

    // ========== 语言设置 ==========
    private void InitLanguageDropdown()
    {
        if (languageDropdown == null) return;

        try
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (locales == null || locales.Count == 0)
            {
                Debug.LogWarning("没有可用的语言设置");
                return;
            }

            languageDropdown.ClearOptions();
            languageDropdown.options = locales
                .Select(l => new TMP_Dropdown.OptionData(l.LocaleName))
                .ToList();

            // 从存档恢复，或使用当前选中的 Locale
            string savedCode = PlayerPrefs.GetString("LanguageCode",
                LocalizationSettings.SelectedLocale != null
                    ? LocalizationSettings.SelectedLocale.Identifier.Code
                    : (locales.Count > 0 ? locales[0].Identifier.Code : "en"));

            int idx = Mathf.Max(0, locales.FindIndex(l => l.Identifier.Code == savedCode));
            languageDropdown.value = idx;
            languageDropdown.RefreshShownValue();

            // 监听切换
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"初始化语言下拉菜单失败: {e.Message}");
        }
    }

    private void OnLanguageDropdownChanged(int index)
    {
        if (languageDropdown == null) return;

        try
        {
            var locales = LocalizationSettings.AvailableLocales.Locales;
            if (index < 0 || index >= locales.Count) return;

            var locale = locales[index];
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString("LanguageCode", locale.Identifier.Code);

            settingsChanged = true;

            Debug.Log($"语言已切换为: {locale.LocaleName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"切换语言失败: {e.Message}");
        }
    }

    // ========== 音量设置 ==========
    private void ChangeBgmVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeBgmVolume(value);
            settingsChanged = true;
        }
    }

    private void ChangeSfxVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeSfxVolume(value);
            settingsChanged = true;
        }
    }

    // ========== 保存设置 ==========
    private void SaveAllSettings()
    {
        try
        {
            // 音量参数保存
            if (AudioManager.Instance != null)
            {
                PlayerPrefs.SetFloat("MainVolume", AudioManager.Instance.mainVolume);
                PlayerPrefs.SetFloat("BgmVolumeFactor", AudioManager.Instance.bgmVolumeFactor);
                PlayerPrefs.SetFloat("SfxVolumeFactor", AudioManager.Instance.sfxVolumeFactor);
            }

            // 键位重绑由 KeybindingItem 在完成重绑时逐条保存
            // 这里只需要保存其他设置

            PlayerPrefs.Save();
            settingsChanged = false;

            Debug.Log("所有设置已保存!");

            // 显示保存成功提示
            ShowTempMessage("设置已保存!", 1.5f);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存设置失败: {e.Message}");
        }
    }

    private void OnBackButtonClicked()
    {
        SaveAllSettings();
        ClosePanel();
    }

    private void ClosePanel()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel(panelName);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}