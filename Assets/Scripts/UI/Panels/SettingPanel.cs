using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem; // 新增

public class SettingsPanel : BasePanel
{
    [Header("组件配置")]
    public Slider bgmVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("多语言")]
    public Dropdown languageDropdown;

    [Header("导航/页签")]
    public Button tabBasicButton;     // “基础设置”页签按钮
    public Button tabBindingsButton;  // “键位设置”页签按钮
    public GameObject basicPage;      // 页：基础设置（包含音量/语言）
    public GameObject bindingsPage;   // 页：键位设置

    [Header("键位设置")]
    public Transform keybindingContainer;     // 键位条目父节点（建议挂 VerticalLayoutGroup）
    public GameObject keybindingItemPrefab;   // 键位条目预制体（含 KeybindingItem）

    public Button backButton;

    private PlayerInputActions inputActions;   // 你的 .inputactions 生成类实例

    private void Start()
    {
        // === 1) 音量初始值 + 监听（你原有逻辑，保留） ===
        bgmVolumeSlider.value = AudioManager.Instance.bgmVolumeFactor;
        sfxVolumeSlider.value = AudioManager.Instance.sfxVolumeFactor;

        bgmVolumeSlider.onValueChanged.AddListener(ChangeBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(ChangeSfxVolume);

        backButton.onClick.AddListener(SaveSettings);

        // === 2) 语言下拉初始化（你原有逻辑，保留） ===
        InitLanguageDropdown();

        // === 3) 页签绑定 ===
        if (tabBasicButton)    tabBasicButton.onClick.AddListener(() => SwitchPage(true));
        if (tabBindingsButton) tabBindingsButton.onClick.AddListener(() => SwitchPage(false));
        SwitchPage(true); // 默认显示基础设置

        // === 4) 键位系统初始化 ===
        // 注意：确保已在 .inputactions 上勾选 “Generate C# Class”，并名为 PlayerInputActions
        inputActions = new PlayerInputActions();

        // 从 PlayerPrefs 载入用户自定义绑定（KeybindingItem 已实现该静态方法）
        KeybindingItem.LoadAllBindings(inputActions);

        // 生成键位条目 UI
        InitKeybindingUI();
    }

    private void SwitchPage(bool showBasic)
    {
        if (basicPage)   basicPage.SetActive(showBasic);
        if (bindingsPage) bindingsPage.SetActive(!showBasic);

        // （可选）更新选中态样式：例如按钮高亮/禁用
        if (tabBasicButton)    tabBasicButton.interactable = !showBasic;
        if (tabBindingsButton) tabBindingsButton.interactable = showBasic;
    }

    private void InitKeybindingUI()
    {
        if (keybindingContainer == null || keybindingItemPrefab == null || inputActions == null) return;

        // 清空旧条目（防止重复生成）
        for (int i = keybindingContainer.childCount - 1; i >= 0; i--)
            GameObject.Destroy(keybindingContainer.GetChild(i).gameObject);

        // 遍历每个 Action，实例化一条 KeybindingItem 并初始化
        foreach (var map in inputActions.asset.actionMaps)
        {
            foreach (var action in map.actions)
            {
                var itemGO = GameObject.Instantiate(keybindingItemPrefab, keybindingContainer);
                var item = itemGO.GetComponent<KeybindingItem>();
                if (item != null)
                {
                    item.Init(action); // 内部会显示当前绑定、支持交互式重绑与保存覆盖
                }
            }
        }
    }

    // ========== 语言相关（保留原逻辑） ==========
    private void InitLanguageDropdown()
    {
        if (languageDropdown == null) return;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        languageDropdown.options = locales
            .Select(l => new Dropdown.OptionData(l.LocaleName)) // TMP 用 new TMP_Dropdown.OptionData(...)
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

    private void OnLanguageDropdownChanged(int index)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count) return;

        var locale = locales[index];
        LocalizationSettings.SelectedLocale = locale;
        PlayerPrefs.SetString("LanguageCode", locale.Identifier.Code);
        PlayerPrefs.Save();

        // 若有自定义的非 LocalizeStringEvent 文本/资源，需要在这里手动刷新
        // RefreshCustomLocalizedContent();
    }

    // ========== 音量相关（保留原逻辑） ==========
    private void ChangeMainVolume(float value)
    {
        AudioManager.Instance.ChangeMainVolume(value);
    }

    private void ChangeBgmVolume(float value)
    {
        AudioManager.Instance.ChangeBgmVolume(value);
    }

    private void ChangeSfxVolume(float value)
    {
        AudioManager.Instance.ChangeSfxVolume(value);
    }

    // ========== 保存 ==========
    private void SaveSettings()
    {
        // 音量参数保存（你原有逻辑）
        PlayerPrefs.SetFloat("MainVolume", AudioManager.Instance.mainVolume);
        PlayerPrefs.SetFloat("BgmVolumeFactor", AudioManager.Instance.bgmVolumeFactor);
        PlayerPrefs.SetFloat("SfxVolumeFactor", AudioManager.Instance.sfxVolumeFactor);

        // 键位重绑由 KeybindingItem 在完成重绑时逐条保存为 JSON（无需在此重复保存）
        // 参考：KeybindingItem.SaveBinding / LoadAllBindings。:contentReference[oaicite:5]{index=5}

        PlayerPrefs.Save();

        UIManager.Instance.ClosePanel(panelName);
        Debug.Log("Settings Saved!");
    }
}
