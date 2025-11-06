using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RecordUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text indexText; //序号
    public Text recordName; //存档名
    public GameObject auto; //自动存档标记
    public Image rect; //边框
    [ColorUsage(true)] public Color enterColor; //鼠标悬停时边框颜色

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

        //隐藏详情面板
        if (OnExit != null)
            OnExit();
    }

    //设置存档序号显示
    public void SetID(int i)
    {
        indexText.text = i.ToString();
    }


    public void SetName(int i)
    {
        //空档则显示空档并隐藏Auto标记
        if (RecordData.Instance.recordName[i] == "")
        {
            recordName.text = "空档";
            auto.SetActive(false);
        }
        else
        {
            //从存档文件名中提取日期和时间
            string full = RecordData.Instance.recordName[i];
            //提取前8位日期
            string date = full.Substring(0, 8);
            //提取后6位时间
            string time = full.Substring(9, 6);
            //格式化日期
            TIME.SetDate(ref date);
            TIME.SetTime(ref time);
            //显示存档名
            recordName.text = date + " " + time;

            //根据后缀判断是否为自动存档
            if (full.Substring(full.Length - 4) == "auto")
                auto.SetActive(true);
            else
                auto.SetActive(false);
        }
    }
}