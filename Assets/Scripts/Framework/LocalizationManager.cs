using UnityEngine;
using System.Collections.Generic;

public static class LocalizationManager
{
    // 动作名称本地化字典
    private static readonly Dictionary<string, string> actionNames = new Dictionary<string, string>()
    {
        { "Move", "移动" },
        { "Interaction", "交互" },
        { "Dash", "冲刺" },
        { "GamePause", "暂停游戏" },
        { "UI_Interaction", "UI交互" },
        { "UI_Bag", "打开背包" }
    };

    // 分组名称本地化字典
    private static readonly Dictionary<string, string> groupNames = new Dictionary<string, string>()
    {
        { "Keyboard", "键盘" },
        { "Gamepad", "手柄" },
        { "WASD", "WASD" },
        { "ArrowKeys", "方向键" },
        { "Mouse", "鼠标" },
        { "Composite", "复合输入" }
    };

    // 动作映射名称本地化字典
    private static readonly Dictionary<string, string> mapNames = new Dictionary<string, string>()
    {
        { "Player", "玩家控制" },
        { "UI", "界面控制" },
        { "Gameplay", "游戏控制" },
        { "GameMap", "游戏控制" },
        { "UIMap", "界面控制" }
    };

    // 复合绑定部分名称本地化字典
    private static readonly Dictionary<string, string> compositePartNames = new Dictionary<string, string>()
    {
        { "up", "上" },
        { "down", "下" },
        { "left", "左" },
        { "right", "右" },
        { "forward", "前" },
        { "backward", "后" }
    };

    // 复合绑定显示名称字典
    private static readonly Dictionary<string, string> compositeDisplayNames = new Dictionary<string, string>()
    {
        { "WASD", "WASD" },
        { "Arrow", "方向键" },
        { "2DVector", "方向输入" }
    };

    /// <summary>
    /// 获取本地化的动作名称
    /// </summary>
    public static string GetActionName(string actionName)
    {
        if (actionNames.TryGetValue(actionName, out string localizedName))
        {
            return localizedName;
        }
        return actionName;
    }

    /// <summary>
    /// 获取本地化的分组名称
    /// </summary>
    public static string GetGroupName(string groupName)
    {
        if (groupNames.TryGetValue(groupName, out string localizedName))
        {
            return localizedName;
        }
        return groupName;
    }

    /// <summary>
    /// 获取本地化的动作映射名称
    /// </summary>
    public static string GetMapName(string mapName)
    {
        if (mapNames.TryGetValue(mapName, out string localizedName))
        {
            return localizedName;
        }
        return mapName;
    }

    /// <summary>
    /// 获取本地化的复合绑定部分名称
    /// </summary>
    public static string GetCompositePartName(string partName)
    {
        string key = partName.ToLower();
        if (compositePartNames.TryGetValue(key, out string localizedName))
        {
            return localizedName;
        }
        return partName;
    }

    /// <summary>
    /// 获取本地化的复合绑定显示名称
    /// </summary>
    public static string GetCompositeDisplayName(string compositeName)
    {
        if (compositeDisplayNames.TryGetValue(compositeName, out string localizedName))
        {
            return localizedName;
        }
        return compositeName;
    }

    /// <summary>
    /// 根据绑定路径获取设备类型
    /// </summary>
    public static string GetDeviceTypeFromBinding(string bindingPath)
    {
        if (string.IsNullOrEmpty(bindingPath))
            return "其他";

        if (bindingPath.Contains("<Keyboard>") || bindingPath.Contains("<Keyboard>/"))
            return "键盘";
        else if (bindingPath.Contains("<Gamepad>") || bindingPath.Contains("<Gamepad>/"))
            return "手柄";
        else if (bindingPath.Contains("<Mouse>") || bindingPath.Contains("<Mouse>/"))
            return "鼠标";
        else
            return "其他";
    }
}