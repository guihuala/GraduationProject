using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//专门用于载入界面的存档面板
public class OnlyLoad : MonoBehaviour
{
    public Transform grid; //存档位父物体
    public GameObject recordPrefab; //存档位预制体

    [Header("存档详情")] public GameObject detail; //存档详情
    public Image screenShot; //截图
    public Text gameTime; //游戏时间
    public Text sceneName; //场景名称
    public Text level; //玩家等级

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
        //读取存档数据并更新详情面板显示
        var data = Player.Instance.ReadForShow(i);
        screenShot.sprite = SAVE.LoadShot(i);
        gameTime.text = $"游戏时间  {TIME.GetFormatTime((int)data.gameTime)}";
        sceneName.text = $"场景名称  {data.scensName}";
        level.text = $"玩家等级  {data.level}";

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
        else
        {
            if (OnLoad != null)
                OnLoad(gridID);
        }
    }
}