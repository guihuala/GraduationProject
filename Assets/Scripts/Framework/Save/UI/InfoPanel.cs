using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class InfoPanel : MonoBehaviour
{
    [Header("按钮")] 
    public Button[] Add;
    public Button[] Sub;
    
    [Header("显示")] 
    public Text sceneName;
    public Text level;
    public Text gameTime;
    public Text isFullScreen;
    public Text difficulty;
    public Text playerName; //玩家名称显示
    public Image colorImg;
    
    [Header("颜色预设")] 
    [ColorUsage(true)] 
    public Color[] colorPreset;
    public Material m;

    [Header("玩家名称输入")]
    public InputField playerNameInput; //玩家名称输入框

    int difficultyID;
    int colorID = 3;

    private void Awake()
    {
        Add[0].onClick.AddListener(() => ChangeScene(1));
        Sub[0].onClick.AddListener(() => ChangeScene(-1));
        Add[1].onClick.AddListener(() => Player.Instance.level++);
        Sub[1].onClick.AddListener(() => Player.Instance.level--);
        Add[2].onClick.AddListener(() => Player.Instance.isFullScreen = true);
        Sub[2].onClick.AddListener(() => Player.Instance.isFullScreen = false);
        Add[3].onClick.AddListener(() =>
        {
            difficultyID++;
            Player.Instance.difficulty = (Player.Difficulty)difficultyID;
        });
        Sub[3].onClick.AddListener(() =>
        {
            difficultyID--;
            Player.Instance.difficulty = (Player.Difficulty)difficultyID;
        });
        Add[4].onClick.AddListener(() =>
        {
            colorID++;
            Player.Instance.color = colorPreset[colorID];
        });
        Sub[4].onClick.AddListener(() =>
        {
            colorID--;
            Player.Instance.color = colorPreset[colorID];
        });

        //玩家名称输入监听
        if (playerNameInput != null)
        {
            playerNameInput.onEndEdit.AddListener(OnPlayerNameChanged);
            playerNameInput.text = Player.Instance.playerName;
        }
    }

    //不能在Awake中获取场景名，因为场景加载顺序问题
    private void Start()
    {
        //获取当前场景并更新玩家数据
        Scene curScene = SceneManager.GetActiveScene();
        Player.Instance.scensName = curScene.name;
        sceneName.text = Player.Instance.scensName;

        //初始化难度ID对应关系
        difficultyID = (int)Player.Instance.difficulty;
    }

    private void LateUpdate()
    {
        //实时更新玩家UI      
        level.text = Player.Instance.level.ToString();
        isFullScreen.text = Player.Instance.isFullScreen.ToString();
        difficulty.text = Player.Instance.difficulty.ToString();
        playerName.text = $"玩家: {Player.Instance.playerName}";
        colorImg.color = Player.Instance.color;
        m.SetColor("_BaseColor", Player.Instance.color);
        gameTime.text = TIME.GetFormatTime((int)TIME.curT);
    }

    //场景+/-按钮回调函数
    void ChangeScene(int d)
    {
        int i = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(i + d);
    }

    //玩家名称改变回调
    void OnPlayerNameChanged(string newName)
    {
        if (!string.IsNullOrEmpty(newName))
        {
            Player.Instance.playerName = newName;
            Debug.Log($"玩家名称已更新: {newName}");
        }
    }
}