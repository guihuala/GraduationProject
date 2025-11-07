using System.Collections;
using System.Collections.Generic;
using GuiFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleUI : MonoBehaviour
{
    public Button Continue; //继续游戏
    public Button Load; //载入游戏
    public Button New; //新游戏
    public Button Exit; //退出游戏
    public Button Option;

    public GameObject recordPanel;
    
    private void Awake()
    {
        //读取最新存档
        Continue.onClick.AddListener(() => LoadRecord(RecordData.Instance.lastID));
        //打开/关闭存档列表
        Load.onClick.AddListener(OpenRecordPanel);
        //存档被点击时调用
        OnlyLoad.OnLoad += LoadRecord;
        //新存档(所有数据初始化)
        New.onClick.AddListener(NewGame);
        //退出游戏(此处不存档)
        Exit.onClick.AddListener(QuitGame);
        Option.onClick.AddListener(OnSettingsButtonClicked);
    }

    private void OnDestroy()
    {
        OnlyLoad.OnLoad -= LoadRecord;
    }

    private void Start()
    {
        //读取存档列表
        RecordData.Instance.Load();

        //检查最新存档的有效性
        if (RecordData.Instance.lastID != 233)
        {
            bool isValid = Player.Instance.ValidateSave(RecordData.Instance.lastID);
            if (isValid)
            {
                Continue.interactable = true;
                Load.interactable = true;
                
                //检查版本兼容性
                var metadata = Player.Instance.GetSaveMetadata(RecordData.Instance.lastID);
                if (metadata != null && metadata.gameVersion != Application.version)
                {
                    //显示版本警告，但仍然允许继续游戏
                    Debug.LogWarning($"存档版本({metadata.gameVersion})与当前游戏版本({Application.version})不匹配");
                }
            }
            else
            {
                Debug.LogError("最新存档已损坏，无法继续游戏");
                Continue.interactable = false;
                Load.interactable = true; //仍然允许载入其他存档
            }
        }
    }

    void LoadRecord(int i)
    {
        //验证存档完整性
        if (!Player.Instance.ValidateSave(i))
        {
            Debug.LogError($"存档 {i} 已损坏，无法加载");
            //可以在这里添加错误提示UI
            return;
        }

        //检查版本兼容性
        var metadata = Player.Instance.GetSaveMetadata(i);
        if (metadata != null && metadata.gameVersion != Application.version)
        {
            //显示版本警告对话框
            ShowVersionWarning(metadata.gameVersion, i);
        }
        else
        {
            //直接加载存档
            ActuallyLoadRecord(i);
        }
    }

    void ActuallyLoadRecord(int i)
    {
        //载入指定存档数据
        Player.Instance.Load(i);

        //如果最新存档不是i，就更新最新存档的序号，并保存
        if (i != RecordData.Instance.lastID)
        {
            RecordData.Instance.lastID = i;
            RecordData.Instance.Save();
        }

        //跳转场景
        SceneManager.LoadScene(Player.Instance.scensName);
    }

    void OpenRecordPanel()
    {
        recordPanel.SetActive(!recordPanel.activeSelf);
    }

    void NewGame()
    {
        //初始化玩家数据
        Player.Instance.level = 1;
        Player.Instance.gameTime = 0;
        Player.Instance.playerName = "Player";
        Player.Instance.scensName = "Cube"; //设置默认场景

        //跳转至默认场景
        SceneManager.LoadScene(Player.Instance.scensName);
    }
    
    public void OnSettingsButtonClicked()
    {
        UIManager.Instance.OpenPanel("SettingPanel");
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ShowVersionWarning(string saveVersion, int saveID)
    {
        //这里可以显示一个对话框，询问用户是否继续
        //为了简化，这里直接继续加载，但记录警告
        Debug.LogWarning($"版本不匹配: 存档版本({saveVersion}) != 当前版本({Application.version})，继续加载...");
        ActuallyLoadRecord(saveID);
        
        //实际项目中可以这样实现：
        //versionWarningDialog.SetActive(true);
        //设置对话框文本
        //var dialogText = versionWarningDialog.GetComponentInChildren<Text>();
        //dialogText.text = $"存档版本({saveVersion})与当前游戏版本({Application.version})不匹配，可能会遇到问题。是否继续？";
        //设置确认按钮的回调
        //var confirmBtn = versionWarningDialog.transform.Find("ConfirmButton").GetComponent<Button>();
        //confirmBtn.onClick.RemoveAllListeners();
        //confirmBtn.onClick.AddListener(() => {
        //    versionWarningDialog.SetActive(false);
        //    ActuallyLoadRecord(saveID);
        //});
    }
}