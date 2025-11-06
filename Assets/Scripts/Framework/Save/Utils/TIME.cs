using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; //TimeSpan


public static class TIME
{
    public static float oriT; //原始时间
    public static float curT; //当前时间    

    public static void SetOriTime()
    {
        float tempT = Time.realtimeSinceStartup;
        oriT = Player.Instance.gameTime - tempT;
        SetCurTime();
    }

    public static void SetCurTime()
    {
        //当前时间不能小于0
        curT = Mathf.Max(TIME.oriT + Time.realtimeSinceStartup, 0);
        Player.Instance.gameTime = curT;
    }


    //将秒数格式化为00:00:00
    public static string GetFormatTime(int seconds)
    {
        //创建时间跨度对象
        TimeSpan ts = new TimeSpan(0, 0, seconds);
        return $"{ts.Hours.ToString("00")}:{ts.Minutes.ToString("00")}:{ts.Seconds.ToString("00")}";
    }

    //将8位日期字符串格式化为YYYY/MM/DD
    public static void SetDate(ref string date)
    {
        date = date.Insert(4, "/");
        date = date.Insert(7, "/");
    }

    //将6位时间字符串格式化为HH:MM:SS
    public static void SetTime(ref string time)
    {
        time = time.Insert(2, ":");
        time = time.Insert(5, ":");
    }
}