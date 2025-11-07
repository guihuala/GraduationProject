using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro;

public class KeybindingItem : MonoBehaviour
{
    public Text actionNameText;
    public Text bindingText;
    public Button rebindButton;
    public Button resetButton;
    
    private InputAction action;
    private int bindingIndex = 0;
    private string bindingGroup = "";

    public void Init(InputAction inputAction, int index = 0, string group = "")
    {
        action = inputAction;
        bindingIndex = index;
        bindingGroup = group;

        // 显示本地化的动作名称和分组
        if (string.IsNullOrEmpty(bindingGroup))
        {
            actionNameText.text = LocalizationManager.GetActionName(inputAction.name);
        }
        else
        {
            actionNameText.text = $"{LocalizationManager.GetActionName(inputAction.name)} - {LocalizationManager.GetGroupName(bindingGroup)}";
        }
        
        UpdateBindingDisplay();

        rebindButton.onClick.AddListener(StartRebind);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetBinding);
    }

    private void UpdateBindingDisplay()
    {
        string displayText = action.GetBindingDisplayString(bindingIndex);
        
        if (bindingText != null)
            bindingText.text = displayText;
        
        if (rebindButton != null)
        {
            var buttonText = rebindButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = displayText;
        }
    }

    private void StartRebind()
    {
        // 禁用按钮防止重复点击
        rebindButton.interactable = false;
        if (resetButton != null) resetButton.interactable = false;
        
        // 显示提示
        if (bindingText != null)
            bindingText.text = "<color=yellow>等待输入...</color>";
        
        // 设置重绑定参数
        var rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse") // 可选：排除鼠标
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                rebindButton.interactable = true;
                if (resetButton != null) resetButton.interactable = true;
                
                operation.Dispose();
                UpdateBindingDisplay();
                SaveBinding(action);
                
                // 显示成功提示
                StartCoroutine(ShowRebindSuccess());
            })
            .OnCancel(operation =>
            {
                rebindButton.interactable = true;
                if (resetButton != null) resetButton.interactable = true;
                
                operation.Dispose();
                UpdateBindingDisplay();
            });

        // 根据设备类型限制输入
        if (bindingGroup.Contains("Keyboard") || bindingGroup.Contains("WASD") || bindingGroup.Contains("ArrowKeys"))
        {
            rebindOperation.WithControlsHavingToMatchPath("<Keyboard>");
        }
        else if (bindingGroup.Contains("Gamepad"))
        {
            rebindOperation.WithControlsHavingToMatchPath("<Gamepad>");
        }
        else if (bindingGroup.Contains("Mouse"))
        {
            rebindOperation.WithControlsHavingToMatchPath("<Mouse>");
        }

        rebindOperation.Start();
    }

    private IEnumerator ShowRebindSuccess()
    {
        if (bindingText != null)
        {
            string originalText = bindingText.text;
            bindingText.text = "<color=green>✓ 绑定成功!</color>";
            yield return new WaitForSeconds(1f);
            bindingText.text = originalText;
        }
    }

    private void ResetBinding()
    {
        // 移除绑定覆盖，恢复默认设置
        action.RemoveBindingOverride(bindingIndex);
        UpdateBindingDisplay();
        SaveBinding(action);
        
        // 显示重置提示
        StartCoroutine(ShowResetSuccess());
    }

    private IEnumerator ShowResetSuccess()
    {
        if (bindingText != null)
        {
            bindingText.text = "<color=green>✓ 已重置!</color>";
            yield return new WaitForSeconds(1f);
            UpdateBindingDisplay();
        }
    }

    private void SaveBinding(InputAction action)
    {
        string key = $"Rebind_{action.actionMap.name}_{action.name}";
        string overrides = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(key, overrides);
        PlayerPrefs.Save();
        
        Debug.Log($"已保存键位绑定: {key}");
    }

    public static void LoadAllBindings(PlayerInputActions actions)
    {
        foreach (var map in actions.asset.actionMaps)
        {
            foreach (var act in map.actions)
            {
                string key = $"Rebind_{map.name}_{act.name}";
                if (PlayerPrefs.HasKey(key))
                {
                    string overrides = PlayerPrefs.GetString(key);
                    if (!string.IsNullOrEmpty(overrides))
                    {
                        act.LoadBindingOverridesFromJson(overrides);
                        Debug.Log($"已加载键位绑定: {key}");
                    }
                }
            }
        }
    }

    // 清理方法
    private void OnDestroy()
    {
        if (rebindButton != null)
            rebindButton.onClick.RemoveAllListeners();
        
        if (resetButton != null)
            resetButton.onClick.RemoveAllListeners();
    }
}