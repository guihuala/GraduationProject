using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class KeybindingItem : MonoBehaviour
{
    public Text actionNameText;
    public Button rebindButton;
    private InputAction action;

    public void Init(InputAction inputAction)
    {
        action = inputAction;
        actionNameText.text = inputAction.name;
        UpdateBindingDisplay();

        rebindButton.onClick.AddListener(StartRebind);
    }

    private void UpdateBindingDisplay()
    {
        rebindButton.GetComponentInChildren<Text>().text = action.GetBindingDisplayString();
    }

    private void StartRebind()
    {
        rebindButton.GetComponentInChildren<Text>().text = "按任意键...";
        action.PerformInteractiveRebinding()
            .OnComplete(operation =>
            {
                operation.Dispose();
                UpdateBindingDisplay();
                SaveBinding(action);
            })
            .Start();
    }

    private void SaveBinding(InputAction action)
    {
        string key = $"Rebind_{action.actionMap.name}_{action.name}";
        string overrides = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(key, overrides);
        PlayerPrefs.Save();
    }

    public static void LoadAllBindings(PlayerInputActions actions)
    {
        foreach (var map in actions.asset.actionMaps)
        {
            foreach (var act in map.actions)
            {
                string key = $"Rebind_{map.name}_{act.name}";
                if (PlayerPrefs.HasKey(key))
                    act.LoadBindingOverridesFromJson(PlayerPrefs.GetString(key));
            }
        }
    }
}