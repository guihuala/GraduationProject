using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家数据，存的是游戏中的实际数据
/// </summary>
public class Player : SingletonPersistent<Player>
{
    public enum Difficulty
    {
        easy,
        normal,
        hard
    }           //难度枚举

    public int level;                   //玩家等级
    public string scensName;            //存档时所在场景  
    public float gameTime;              //游戏时间
    public bool isFullScreen;           //是否全屏
    public Difficulty difficulty;       //难度
    [ColorUsage(true)]
    public Color color;                 //模型颜色
    
    public class SaveData
    {
        public string scensName;
        public int level;
        public float gameTime;
        public bool isFullScreen;
        public Color color;
        public Player.Difficulty difficulty;
    }

    SaveData ForSave()
    {
        var savedata = new SaveData();
        savedata.scensName = scensName;
        savedata.level = level;
        savedata.gameTime = gameTime;
        savedata.isFullScreen = isFullScreen;
        savedata.color = color;
        savedata.difficulty = difficulty;
        return savedata;
    }

    void ForLoad(SaveData savedata)
    {
        scensName = savedata.scensName;
        level= savedata.level;
        gameTime= savedata.gameTime;
        isFullScreen= savedata.isFullScreen;
        color=savedata.color;
        difficulty= savedata.difficulty;
    }

    public void Save(int id)
    {
        SAVE.JsonSave(RecordData.Instance.recordName[id], ForSave());
    }

    public void Load(int id)
    {
        var saveData = SAVE.JsonLoad<SaveData>(RecordData.Instance.recordName[id]);
        ForLoad(saveData);
    }

    public SaveData ReadForShow(int id)
    {
        return SAVE.JsonLoad<SaveData>(RecordData.Instance.recordName[id]);
    }

    public void Delete(int id)
    {
        SAVE.JsonDelete(RecordData.Instance.recordName[id]);
    }
}