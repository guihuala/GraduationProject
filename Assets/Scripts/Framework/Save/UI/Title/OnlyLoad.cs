using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//专门用于载入界面的存档面板
public class OnlyLoad : MonoBehaviour
{
    public Transform grid; //存档位父物体
    public GameObject recordPrefab; //存档位预制体

    [Header("存档详情")] 
    public GameObject detail; //存档详情
    public Image screenShot; //截图
    public Text gameTime; //游戏时间
    public Text sceneName; //场景名称
    public Text level; //玩家等级
    public Text playerName; //玩家名称
    public Text saveTime; //存档时间
    public Text gameVersion; //游戏版本
    public Text platform; //平台信息
    public Text fileSize; //文件大小
    public Text description; //存档描述
    public GameObject versionWarning; //版本警告

    //存档被点击时调用
    public static System.Action<int> OnLoad;

    private void Start()
    {
        //初始化并生成存档位   
        for (int i = 0; i < RecordData.recordNum; i++)
        {
            GameObject obj = Instantiate(recordPrefab, grid);
            //命名
            obj.name = (i + 1).ToString();
            obj.GetComponent<RecordUI>().SetID(i + 1);
            //如果该位置有存档则更新存档名和按钮颜色
            if (RecordData.Instance.recordName[i] != "")
                obj.GetComponent<RecordUI>().SetName(i);
        }

        RecordUI.OnLeftClick += LeftClickGrid;
        RecordUI.OnEnter += ShowDetails;
        RecordUI.OnExit += HideDetails;
    }

    private void OnDestroy()
    {
        RecordUI.OnLeftClick -= LeftClickGrid;
        RecordUI.OnEnter -= ShowDetails;
        RecordUI.OnExit -= HideDetails;
    }

    //RecordUI.OnEnter回调
    void ShowDetails(int i)
    {
        //读取存档数据和元数据
        var data = Player.Instance.ReadForShow(i);
        var metadata = Player.Instance.GetSaveMetadata(i);
        
        //基本游戏数据
        screenShot.sprite = SAVE.LoadShot(i);
        gameTime.text = $"游戏时间  {TIME.GetFormatTime((int)data.gameTime)}";
        sceneName.text = $"场景名称  {data.scensName}";
        level.text = $"玩家等级  {data.level}";
        playerName.text = $"玩家名称  {data.playerName}";

        //元数据显示
        if (metadata != null)
        {
            DateTime saveDateTime = DateTimeOffset.FromUnixTimeSeconds(metadata.createTime).LocalDateTime;
            saveTime.text = $"存档时间  {saveDateTime:yyyy/MM/dd HH:mm:ss}";
            gameVersion.text = $"游戏版本  {metadata.gameVersion}";
            platform.text = $"平台  {metadata.platform}";
            fileSize.text = $"文件大小  {FormatFileSize(metadata.fileSize)}";
            description.text = string.IsNullOrEmpty(metadata.description) ? "描述  无" : $"描述  {metadata.description}";
            
            //版本兼容性警告
            versionWarning.SetActive(metadata.gameVersion != Application.version);
            
            //数据完整性验证
            bool isValid = Player.Instance.ValidateSave(i);
            if (!isValid)
            {
                Debug.LogWarning($"存档 {i} 数据可能已损坏");
                //可以添加视觉提示，比如改变边框颜色
            }
        }
        else
        {
            //隐藏元数据相关UI
            saveTime.text = "";
            gameVersion.text = "";
            platform.text = "";
            fileSize.text = "";
            description.text = "";
            versionWarning.SetActive(false);
        }

        //显示详情
        detail.SetActive(true);
    }

    //RecordUI.OnExit回调
    void HideDetails()
    {
        //隐藏详情
        detail.SetActive(false);
    }

    //左键点击存档位
    void LeftClickGrid(int gridID)
    {
        //空档位什么也不做
        if (RecordData.Instance.recordName[gridID] == "")
            return;
        
        //检查存档完整性
        bool isValid = Player.Instance.ValidateSave(gridID);
        if (!isValid)
        {
            Debug.LogError($"存档已损坏，无法加载: {gridID}");
            //可以在这里添加弹窗提示
            return;
        }

        if (OnLoad != null)
            OnLoad(gridID);
    }

    //格式化文件大小显示
    private string FormatFileSize(int bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{(bytes / 1024f):0.0} KB";
        else
            return $"{(bytes / (1024f * 1024f)):0.0} MB";
    }
}