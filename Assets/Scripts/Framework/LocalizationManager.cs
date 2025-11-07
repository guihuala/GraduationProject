using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Localization.Tables;

public static class LocalizationManager
{
    // 本地化表格引用
    private static LocalizedStringTable _stringTable = new LocalizedStringTable { TableReference = "UI" };
    private static StringTable _currentStringTable;
    
    // 缓存字典，避免频繁查找
    private static Dictionary<string, string> _cachedLocalizations = new Dictionary<string, string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // 监听本地化设置变化
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        
        // 初始加载字符串表
        LoadStringTable();
    }

    private static async void LoadStringTable()
    {
        try
        {
            var tableLoading = _stringTable.GetTableAsync();
            await tableLoading.Task;
            
            _currentStringTable = tableLoading.Result;
            if (_currentStringTable != null)
            {
                _cachedLocalizations.Clear();
                Debug.Log("本地化表格加载成功");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载本地化表格失败: {e.Message}");
        }
    }

    private static void OnLocaleChanged(Locale locale)
    {
        // 语言切换时重新加载表格
        LoadStringTable();
    }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    public static string GetLocalizedString(string key, string fallback = "")
    {
        // 首先检查缓存
        if (_cachedLocalizations.TryGetValue(key, out string cachedValue))
        {
            return cachedValue;
        }

        // 如果有字符串表，从表中获取
        if (_currentStringTable != null)
        {
            var entry = _currentStringTable.GetEntry(key);
            if (entry != null && !string.IsNullOrEmpty(entry.Value))
            {
                _cachedLocalizations[key] = entry.Value;
                return entry.Value;
            }
        }

        // 回退到硬编码的本地化
        string fallbackValue = GetHardcodedLocalization(key, fallback);
        _cachedLocalizations[key] = fallbackValue;
        return fallbackValue;
    }

    /// <summary>
    /// 硬编码的本地化回退（在没有本地化表格时使用）
    /// </summary>
    private static string GetHardcodedLocalization(string key, string fallback)
    {
        // 动作名称本地化
        var actionNames = new Dictionary<string, string>()
        {
            { "ACTION_Move", "移动" },
            { "ACTION_Interaction", "交互" },
            { "ACTION_Dash", "冲刺" },
            { "ACTION_GamePause", "暂停游戏" },
            { "ACTION_UI_Interaction", "UI交互" },
            { "ACTION_UI_Bag", "打开背包" }
        };

        // 分组名称本地化
        var groupNames = new Dictionary<string, string>()
        {
            { "GROUP_Keyboard", "键盘" },
            { "GROUP_Gamepad", "手柄" },
            { "GROUP_WASD", "WASD" },
            { "GROUP_ArrowKeys", "方向键" },
            { "GROUP_Mouse", "鼠标" },
            { "GROUP_Composite", "复合输入" }
        };

        // 动作映射名称本地化
        var mapNames = new Dictionary<string, string>()
        {
            { "MAP_Player", "玩家控制" },
            { "MAP_UI", "界面控制" },
            { "MAP_Gameplay", "游戏控制" },
            { "MAP_GameMap", "游戏控制" },
            { "MAP_UIMap", "界面控制" }
        };

        // 复合绑定部分名称本地化
        var compositePartNames = new Dictionary<string, string>()
        {
            { "PART_up", "上" },
            { "PART_down", "下" },
            { "PART_left", "左" },
            { "PART_right", "右" },
            { "PART_forward", "前" },
            { "PART_backward", "后" }
        };

        // 复合绑定显示名称
        var compositeDisplayNames = new Dictionary<string, string>()
        {
            { "COMPOSITE_WASD", "WASD" },
            { "COMPOSITE_Arrow", "方向键" },
            { "COMPOSITE_2DVector", "方向输入" }
        };

        // UI 文本
        var uiTexts = new Dictionary<string, string>()
        {
            { "UI_RebindHint", "点击任意动作旁边的按钮来重新绑定键位" },
            { "UI_ResetAllConfirmTitle", "重置所有键位" },
            { "UI_ResetAllConfirmMessage", "确定要将所有键位重置为默认设置吗？此操作不可撤销。" },
            { "UI_ResetAllConfirmOK", "确定重置" },
            { "UI_ResetAllConfirmCancel", "取消" },
            { "UI_RebindWaiting", "等待输入..." },
            { "UI_RebindSuccess", "✓ 绑定成功!" },
            { "UI_ResetSuccess", "✓ 已重置!" },
            { "UI_AllBindingsReset", "所有键位已重置为默认设置" },
            { "UI_SettingsSaved", "设置已保存!" }
        };

        // 按前缀查找对应的字典
        if (key.StartsWith("ACTION_") && actionNames.TryGetValue(key, out string actionValue))
            return actionValue;
        else if (key.StartsWith("GROUP_") && groupNames.TryGetValue(key, out string groupValue))
            return groupValue;
        else if (key.StartsWith("MAP_") && mapNames.TryGetValue(key, out string mapValue))
            return mapValue;
        else if (key.StartsWith("PART_") && compositePartNames.TryGetValue(key, out string partValue))
            return partValue;
        else if (key.StartsWith("COMPOSITE_") && compositeDisplayNames.TryGetValue(key, out string compositeValue))
            return compositeValue;
        else if (key.StartsWith("UI_") && uiTexts.TryGetValue(key, out string uiValue))
            return uiValue;

        return string.IsNullOrEmpty(fallback) ? key : fallback;
    }

    // 便捷方法
    public static string GetActionName(string actionName) 
        => GetLocalizedString($"ACTION_{actionName}", actionName);

    public static string GetGroupName(string groupName) 
        => GetLocalizedString($"GROUP_{groupName}", groupName);

    public static string GetMapName(string mapName) 
        => GetLocalizedString($"MAP_{mapName}", mapName);

    public static string GetCompositePartName(string partName) 
        => GetLocalizedString($"PART_{partName.ToLower()}", partName);

    public static string GetCompositeDisplayName(string compositeName) 
        => GetLocalizedString($"COMPOSITE_{compositeName}", compositeName);

    public static string GetUIText(string uiKey, string fallback = "") 
        => GetLocalizedString($"UI_{uiKey}", fallback);

    /// <summary>
    /// 根据绑定路径获取设备类型
    /// </summary>
    public static string GetDeviceTypeFromBinding(string bindingPath)
    {
        if (string.IsNullOrEmpty(bindingPath))
            return GetLocalizedString("GROUP_Other", "其他");

        if (bindingPath.Contains("<Keyboard>") || bindingPath.Contains("<Keyboard>/"))
            return GetLocalizedString("GROUP_Keyboard", "键盘");
        else if (bindingPath.Contains("<Gamepad>") || bindingPath.Contains("<Gamepad>/"))
            return GetLocalizedString("GROUP_Gamepad", "手柄");
        else if (bindingPath.Contains("<Mouse>") || bindingPath.Contains("<Mouse>/"))
            return GetLocalizedString("GROUP_Mouse", "鼠标");
        else
            return GetLocalizedString("GROUP_Other", "其他");
    }
}