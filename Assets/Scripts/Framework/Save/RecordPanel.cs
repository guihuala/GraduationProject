using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RecordPanel : MonoBehaviour
{
    public Transform grid;               //存档位置父物体
    public GameObject recordPrefab;      //存档预制体
    public GameObject recordPanel;      //存档面板显示/隐藏

    [Header("按钮")]
    public Button open;
    public Button exit;
    public Button save;
    public Button load;
    [ColorUsage(true)]
    public Color oriColor;              //按钮初始颜色
   
    [Header("存档详情")]   
    public GameObject detail;           //存档详情
    public Image screenShot;            //截图
    public Text gameTime;               //游戏时间
    public Text sceneName;              //场景名称
    public Text level;                  //玩家等级

    //Key为存档文件名 Value为存档序号
    Dictionary<string, int> RecordInGrid = new Dictionary<string, int>();
    bool isSave=false;     //是否为存档模式
    bool isLoad=false;     //是否为读档模式

    private void Start()
    {
        //初始化并生成存档位
        for (int i = 0; i < RecordData.recordNum; i++)
        {
            GameObject obj=Instantiate(recordPrefab, grid);
            //命名
            obj.name = (i + 1).ToString();
            obj.GetComponent<RecordUI>().SetID(i + 1);
            //如果该位置有存档则更新存档名和按钮颜色
            if (RecordData.Instance.recordName[i] != "")
            {
                obj.GetComponent<RecordUI>().SetName(i);               
                //添加键值
                RecordInGrid.Add(RecordData.Instance.recordName[i], i);
            }            
        }

        #region 监听
        RecordUI.OnLeftClick += LeftClickGrid;     
        RecordUI.OnRightClick += RightClickGrid;
        RecordUI.OnEnter += ShowDetails;
        RecordUI.OnExit += HideDetails;
        open.onClick.AddListener(() => CloseOrOpen());
        save.onClick.AddListener(()=>SaveOrLoad());
        load.onClick.AddListener(() => SaveOrLoad(false));
        exit.onClick.AddListener(QuitGame);
        #endregion

        //重置时间
        TIME.SetOriTime();
    }

    private void OnDestroy()
    {
        RecordUI.OnLeftClick -= LeftClickGrid;
        RecordUI.OnRightClick -= RightClickGrid;
        RecordUI.OnEnter -= ShowDetails;
        RecordUI.OnExit -= HideDetails;
    }

    private void Update()
    {
        TIME.SetCurTime();
    }


    //RecordUI.OnEnter回调
    void ShowDetails(int i)
    {
        //读取存档数据并更新详情面板显示
        var data = Player.Instance.ReadForShow(i);
        gameTime.text = $"游戏时间  {TIME.GetFormatTime((int)data.gameTime)}";
        sceneName.text = $"场景名称  {data.scensName}";
        level.text = $"玩家等级  {data.level}";
        screenShot.sprite = SAVE.LoadShot(i);

        //显示详情
        detail.SetActive(true);
    }

    //RecordUI.OnExit回调
    void HideDetails()
    {
        //隐藏详情
        detail.SetActive(false);
    }


    //按钮OPEN回调
    void CloseOrOpen()
    {       
        //切换面板显示/隐藏
        recordPanel.SetActive(!recordPanel.activeSelf);
        //切换文本
        open.transform.GetChild(0).GetComponent<Text>().text = (recordPanel.activeSelf) ? "CLOSE" : "OPEN";
        //切换是否可交互
        save.interactable = (recordPanel.activeSelf) ? true : false;
        load.interactable = (recordPanel.activeSelf) ? true : false;
    }


    //按钮save和load回调
    void SaveOrLoad(bool OnSave=true)
    {
        //切换模式
        isSave = OnSave;
        isLoad = !OnSave;
        //切换按钮颜色
        save.GetComponent<Image>().color = (isSave)?Color.white:oriColor;
        load.GetComponent<Image>().color = (isLoad)?Color.white:oriColor;
    }


    //左键点击
    void LeftClickGrid(int ID)
    {
        //存档
        if (isSave)
        {           
            NewRecord(ID);
        }
        //读档
        else if (isLoad)
        {
            //空位什么也不做
            if (RecordData.Instance.recordName[ID] == "")           
                return;           
            else
            {
                //读取该存档并更新玩家数据
                Player.Instance.Load(ID);    
                //将当前存档ID记录到最新存档
                RecordData.Instance.lastID = ID;
                RecordData.Instance.Save();

                //切换场景
                if (SceneManager.GetActiveScene().name != Player.Instance.scensName)
                {
                    SceneManager.LoadScene(Player.Instance.scensName);
                }
                TIME.SetOriTime();
            }
        }
    }

    //右键删除
    void RightClickGrid(int gridID)
    {
        if (RecordData.Instance.recordName[gridID] == "")        
            return;
        
        //非空位则删除
        else       
            DeleteRecord(gridID, false);
        
    }

    private void QuitGame()
    {
        string autoName = SAVE.FindAuto();
        if (autoName != "")
        {
            int autoID;
            //尝试从字典中获取存档序号
            //即使字典中没有该键值也会返回0
            RecordInGrid.TryGetValue(autoName, out autoID);
            Debug.Log($"找到自动存档序号为{autoID}");
            //删除原先的自动存档并新建一个自动存档
            NewRecord(autoID, ".auto");
        }
        else
        {
            Debug.Log("无自动存档");
            for (int i = 0; i < RecordData.recordNum; i++)
            {
                //空位
                if (RecordData.Instance.recordName[i] == "")
                {
                    NewRecord(i, ".auto");
                    break;
                }
            }

        }

        //退出游戏
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();                          
        #endif
    }


    void NewRecord(int i,string end=".save")
    {
        //该位置有原存档
        if (RecordData.Instance.recordName[i] != "")
        {
            //只删除键值
            DeleteRecord(i);
        }

        //生成存档名
        RecordData.Instance.recordName[i] = $"{System.DateTime.Now:yyyyMMdd_HHmmss}{end}";
        //将当前存档标记为最新存档并保存存档列表
        RecordData.Instance.lastID = i;
        RecordData.Instance.Save();
        //根据玩家数据生成该存档文件
        Player.Instance.Save(i);
        //添加新存档键值
        RecordInGrid.Add(RecordData.Instance.recordName[i], i);
        //更新新存档UI
        grid.GetChild(i).GetComponent<RecordUI>().SetName(i);
        //截图
        SAVE.CameraCapture(i, Camera.main, new Rect(0, 0, Screen.width, Screen.height));
        //显示详情
        ShowDetails(i);
    }


    //true为覆盖模式 false为删除模式
    void DeleteRecord(int i,bool isCover = true)
    {
        //删除存档文件
        Player.Instance.Delete(i);
        //删除键值
        RecordInGrid.Remove(RecordData.Instance.recordName[i]);

        if (!isCover)
        {           
            //清空存档名
            RecordData.Instance.recordName[i] = "";
            //更新UI
            grid.GetChild(i).GetComponent<RecordUI>().SetName(i);
            //删除截图
            SAVE.DeleteShot(i);
            //隐藏详情
            HideDetails();
        }
    }
}