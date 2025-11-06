using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SAVE
{
    //截图保存路径
    public static string shotPath = $"{Application.persistentDataPath}/Shot";

    static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    #region PlayerPrefs

    public static void PlayerPrefsSave(string key, object data)
    {
        //将对象序列化为JSON字符串
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public static string PlayerPrefsLoad(string key)
    {
        //如果没有该键则返回null而不是空字符串
        return PlayerPrefs.GetString(key, null);
    }

    #endregion

    #region JSON

    public static void JsonSave(string fileName, object data)
    {
        string json = JsonUtility.ToJson(data);

        File.WriteAllText(GetPath(fileName), json);
        Debug.Log($"存档成功 {GetPath(fileName)}");
    }

    public static T JsonLoad<T>(string fileName)
    {
        string path = GetPath(fileName);
        //检查文件是否存在
        if (File.Exists(path))
        {
            string json = File.ReadAllText(GetPath(fileName));
            var data = JsonUtility.FromJson<T>(json);
            Debug.Log($"读取成功 {path}");
            return data;
        }
        else
        {
            //如果文件不存在返回默认值
            return default;
        }
    }

    public static void JsonDelete(string fileName)
    {
        File.Delete(GetPath(fileName));
    }

    public static string FindAuto()
    {
        //检查目录是否存在
        if (Directory.Exists(Application.persistentDataPath))
        {
            //获取所有存档文件
            FileInfo[] fileInfos = new DirectoryInfo(Application.persistentDataPath).GetFiles("*");
            for (int i = 0; i < fileInfos.Length; i++)
            {
                //查找自动存档
                if (fileInfos[i].Name.EndsWith(".auto"))
                {
                    return fileInfos[i].Name;
                }
            }
        }

        return "";
    }

    #endregion

    #region 截图

    /*使用ScreenCapture截图，但会包含UI
    该方法保存的路径在Asset文件夹下
    需要等待一帧才能读取
    ScreenCapture.CaptureScreenshot(path);
    */

    /*使用相机渲染截图，可以控制截图范围
    */
    public static void CameraCapture(int i, Camera camera, Rect rect)
    {
        //创建截图目录
        if (!Directory.Exists(SAVE.shotPath))
            Directory.CreateDirectory(SAVE.shotPath);
        string path = Path.Combine(SAVE.shotPath, $"{i}.png");

        int w = (int)rect.width;
        int h = (int)rect.height;

        RenderTexture rt = new RenderTexture(w, h, 0);
        //将相机渲染到RenderTexture
        camera.targetTexture = rt;
        camera.Render();

        ////渲染第二个相机（如果有的话）
        //Camera c2 = camera.GetUniversalAdditionalCameraData().cameraStack[0];
        //c2.targetTexture=rt;
        //c2.Render();

        //激活RenderTexture
        RenderTexture.active = rt;

        //创建4通道mipChain为false的纹理
        Texture2D t2D = new Texture2D(w, h, TextureFormat.RGB24, true);

        //等待渲染完成，避免截图不全的问题(?)
        //yield return new WaitForEndOfFrame();
        //从RenderTexture读取像素到Texture2D
        t2D.ReadPixels(rect, 0, 0);
        t2D.Apply();

        //编码为PNG
        byte[] bytes = t2D.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        //恢复相机设置    
        camera.targetTexture = null;
        //c2.targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(rt);
    }


    public static Sprite LoadShot(int i)
    {
        var path = Path.Combine(shotPath, $"{i}.png");

        Texture2D t = new Texture2D(640, 360);
        t.LoadImage(GetImgByte(path));
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
    }


    static byte[] GetImgByte(string path)
    {
        FileStream s = new FileStream(path, FileMode.Open);
        byte[] imgByte = new byte[s.Length];
        s.Read(imgByte, 0, imgByte.Length);
        s.Close();
        return imgByte;
    }

    public static void DeleteShot(int i)
    {
        var path = Path.Combine(shotPath, $"{i}.png");
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"删除截图{i}");
        }
    }

    #endregion

    #region 工具

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Clear Data/存档列表")]
    public static void DeleteRecord()
    {
        UnityEngine.PlayerPrefs.DeleteAll();
        Debug.Log("清空存档列表");
    }

    [UnityEditor.MenuItem("Tools/Clear Data/玩家存档文件")]
    public static void DeletePlayerData()
    {
        ClearDirectory(Application.persistentDataPath);
        Debug.Log("删除所有玩家存档文件");
    }

    static void ClearDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            FileInfo[] f = new DirectoryInfo(path).GetFiles("*");
            for (int i = 0; i < f.Length; i++)
            {
                Debug.Log($"删除文件{f[i].Name}");
                File.Delete(f[i].FullName);
            }
        }
    }

    [UnityEditor.MenuItem("Tools/Clear Data/截图文件")]
    public static void DeleteScreenShot()
    {
        ClearDirectory(shotPath);
        Debug.Log("删除所有截图");
    }

    [UnityEditor.MenuItem("Tools/Clear Data/所有数据")]
    public static void DeleteAll()
    {
        DeletePlayerData();
        DeleteRecord();
        DeleteScreenShot();
    }
#endif

    #endregion
}