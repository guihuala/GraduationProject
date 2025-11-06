using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class RecordUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text indexText; //序号
    public Text recordName; //存档名
    public GameObject auto; //自动存档标记
    public Image rect; //边框
    [ColorUsage(true)] public Color enterColor; //鼠标悬停时边框颜色
    public Color corruptedColor = Color.red; //损坏存档颜色

    public static System.Action<int> OnLeftClick;
    public static System.Action<int> OnRightClick;
    public static System.Action<int> OnEnter;
    public static System.Action OnExit;

    int id;

    private void Start()
    {
        id = transform.GetSiblingIndex();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (OnLeftClick != null)
                OnLeftClick(id);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (OnRightClick != null)
                OnRightClick(id);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //改变边框颜色
        rect.color = enterColor;

        //非空存档才触发显示详情事件
        if (recordName.text != "空档")
        {
            if (OnEnter != null)
                OnEnter(id);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //恢复边框颜色
        rect.color = Color.white;
    }

    //设置存档序号显示
    public void SetID(int i)
    {
        indexText.text = i.ToString();
    }

    public void SetName(int i)
    {
        //空档则显示空档并隐藏其他元素
        if (RecordData.Instance.recordName[i] == "")
        {
            recordName.text = "空档";
            auto.SetActive(false);
            recordName.color = Color.white;
        }
        else
        {
            // 获取存档元数据
            var metadata = Player.Instance.GetSaveMetadata(i);
            
            if (metadata != null)
            {
                // 显示格式化日期时间
                DateTime createTime = DateTimeOffset.FromUnixTimeSeconds(metadata.createTime).LocalDateTime;
                recordName.text = $"{createTime:MM/dd HH:mm}";

                // 显示自动存档标识
                auto.SetActive(metadata.isAutoSave);
                
                // 版本兼容性警告
                bool versionMismatch = metadata.gameVersion != Application.version;
                
                // 验证存档完整性
                bool isValid = Player.Instance.ValidateSave(i);
                if (!isValid)
                {
                    recordName.color = corruptedColor;
                    recordName.text = "存档已损坏";
                }
                else
                {
                    recordName.color = versionMismatch ? Color.yellow : Color.white;
                }
            }
            else
            {
                // 元数据读取失败，使用旧方式显示
                string full = RecordData.Instance.recordName[i];
                string date = full.Substring(0, 8);
                string time = full.Substring(9, 6);            
                TIME.SetDate(ref date);
                TIME.SetTime(ref time);
                recordName.text = date + " " + time;
                
                // 隐藏其他信息
                auto.SetActive(false);
                recordName.color = Color.white;
            }
        }
    }
}