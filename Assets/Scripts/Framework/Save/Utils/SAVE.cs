using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public static class SAVE
{
    //截图保存路径
    public static string shotPath = $"{Application.persistentDataPath}/Shot";

    private static readonly string EncryptionKey = "GuihuaMoku";
    private static readonly byte[] Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(16).Substring(0, 16));
    private static readonly byte[] Iv = new byte[16]; // 初始化向量

    static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    #region 存档元数据

    /// <summary>
    /// 存档元数据
    /// </summary>
    [System.Serializable]
    public class SaveMetadata
    {
        public string version = "1.0.0"; // 存档版本
        public string gameVersion = Application.version; // 游戏版本
        public long createTime; // 创建时间戳
        public long lastModifiedTime; // 最后修改时间戳
        public string checksum; // 数据校验和
        public int fileSize; // 文件大小
        public string platform = Application.platform.ToString(); // 平台信息
        public string playerName = ""; // 玩家名称（可选）
        public string sceneName = ""; // 场景名称
        public int playerLevel; // 玩家等级
        public float playTime; // 游戏时间
        public bool isAutoSave = false; // 是否为自动存档
        public string description = ""; // 存档描述（可选）
    }

    /// <summary>
    /// 带元数据的存档包装器
    /// </summary>
    [System.Serializable]
    private class SaveDataWrapper
    {
        public SaveMetadata metadata;
        public string gameData; // 加密后的游戏数据
    }

    /// <summary>
    /// 创建存档元数据
    /// </summary>
    private static SaveMetadata CreateMetadata(object gameData, bool isAutoSave = false, string playerName = "",
        string description = "")
    {
        string jsonData = JsonUtility.ToJson(gameData);

        return new SaveMetadata
        {
            version = "1.0.0",
            gameVersion = Application.version,
            createTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            lastModifiedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            checksum = CalculateChecksum(jsonData),
            fileSize = Encoding.UTF8.GetByteCount(jsonData),
            platform = Application.platform.ToString(),
            playerName = playerName,
            isAutoSave = isAutoSave,
            description = description
        };
    }

    /// <summary>
    /// 更新存档元数据
    /// </summary>
    private static void UpdateMetadata(ref SaveMetadata metadata, object gameData)
    {
        string jsonData = JsonUtility.ToJson(gameData);
        metadata.lastModifiedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        metadata.checksum = CalculateChecksum(jsonData);
        metadata.fileSize = Encoding.UTF8.GetByteCount(jsonData);
    }

    /// <summary>
    /// 计算数据校验和
    /// </summary>
    private static string CalculateChecksum(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    /// <summary>
    /// 验证存档完整性
    /// </summary>
    public static bool ValidateSave(string fileName)
    {
        try
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
                return false;

            string encryptedWrapper = File.ReadAllText(path);
            string decryptedWrapper = Decrypt(encryptedWrapper);

            if (string.IsNullOrEmpty(decryptedWrapper))
                return false;

            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(decryptedWrapper);
            string decryptedGameData = Decrypt(wrapper.gameData);

            if (string.IsNullOrEmpty(decryptedGameData))
                return false;

            // 验证校验和
            string currentChecksum = CalculateChecksum(decryptedGameData);
            return wrapper.metadata.checksum == currentChecksum;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取存档元数据（不加载完整数据）
    /// </summary>
    public static SaveMetadata GetSaveMetadata(string fileName)
    {
        try
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
                return null;

            string encryptedWrapper = File.ReadAllText(path);
            string decryptedWrapper = Decrypt(encryptedWrapper);

            if (string.IsNullOrEmpty(decryptedWrapper))
                return null;

            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(decryptedWrapper);
            return wrapper.metadata;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region 加密解密方法

    /// <summary>
    /// 加密字符串
    /// </summary>
    private static string Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key;
            aes.IV = Iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return System.Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    /// <summary>
    /// 解密字符串
    /// </summary>
    private static string Decrypt(string cipherText)
    {
        try
        {
            byte[] buffer = System.Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = Iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch
        {
            // 如果解密失败，返回空字符串
            return string.Empty;
        }
    }

    #endregion

    #region PlayerPrefs

    public static void PlayerPrefsSave(string key, object data)
    {
        //将对象序列化为JSON字符串
        string json = JsonUtility.ToJson(data);
        //对JSON数据进行加密
        string encryptedJson = Encrypt(json);
        PlayerPrefs.SetString(key, encryptedJson);
        PlayerPrefs.Save();
    }

    public static string PlayerPrefsLoad(string key)
    {
        //如果没有该键则返回null而不是空字符串
        string encryptedJson = PlayerPrefs.GetString(key, null);
        if (string.IsNullOrEmpty(encryptedJson))
            return null;

        //解密数据
        string decryptedJson = Decrypt(encryptedJson);
        return string.IsNullOrEmpty(decryptedJson) ? null : decryptedJson;
    }

    #endregion

    #region JSON

    public static void JsonSave(string fileName, object data, bool isAutoSave = false, string playerName = "",
        string description = "")
    {
        string jsonData = JsonUtility.ToJson(data);

        // 创建元数据
        SaveMetadata metadata = CreateMetadata(data, isAutoSave, playerName, description);

        // 加密游戏数据
        string encryptedGameData = Encrypt(jsonData);

        // 创建包装器
        SaveDataWrapper wrapper = new SaveDataWrapper
        {
            metadata = metadata,
            gameData = encryptedGameData
        };

        // 序列化并加密包装器
        string wrapperJson = JsonUtility.ToJson(wrapper);
        string encryptedWrapper = Encrypt(wrapperJson);

        File.WriteAllText(GetPath(fileName), encryptedWrapper);
        Debug.Log($"存档成功 {GetPath(fileName)} - 玩家: {playerName}, 场景: {metadata.sceneName}, 等级: {metadata.playerLevel}");
    }

    public static T JsonLoad<T>(string fileName)
    {
        string path = GetPath(fileName);
        //检查文件是否存在
        if (File.Exists(path))
        {
            string encryptedWrapper = File.ReadAllText(GetPath(fileName));
            //解密包装器
            string decryptedWrapper = Decrypt(encryptedWrapper);

            if (string.IsNullOrEmpty(decryptedWrapper))
            {
                Debug.LogError($"解密失败 {path}");
                return default;
            }

            // 反序列化包装器
            SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(decryptedWrapper);

            // 解密游戏数据
            string decryptedGameData = Decrypt(wrapper.gameData);

            if (string.IsNullOrEmpty(decryptedGameData))
            {
                Debug.LogError($"游戏数据解密失败 {path}");
                return default;
            }

            // 验证数据完整性
            string currentChecksum = CalculateChecksum(decryptedGameData);
            if (wrapper.metadata.checksum != currentChecksum)
            {
                Debug.LogError($"存档数据损坏 {path}");
                return default;
            }

            var data = JsonUtility.FromJson<T>(decryptedGameData);
            Debug.Log($"读取成功 {path} - 版本: {wrapper.metadata.version}, 游戏时间: {wrapper.metadata.playTime}小时");
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

    [UnityEditor.MenuItem("Tools/Debug/生成新密钥")]
    public static void GenerateNewKey()
    {
        //生成随机密钥
        using (Aes aes = Aes.Create())
        {
            aes.GenerateKey();
            string newKey = System.Convert.ToBase64String(aes.Key);
            Debug.Log($"新生成的密钥: {newKey}");
            Debug.Log($"密钥长度: {newKey.Length}");
        }
    }

    [UnityEditor.MenuItem("Tools/Debug/验证所有存档")]
    public static void ValidateAllSaves()
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            FileInfo[] files = new DirectoryInfo(Application.persistentDataPath).GetFiles("*");
            int validCount = 0;
            int totalCount = 0;

            foreach (var file in files)
            {
                if (file.Name.EndsWith(".save") || file.Name.EndsWith(".auto"))
                {
                    totalCount++;
                    bool isValid = ValidateSave(file.Name);
                    if (isValid)
                    {
                        validCount++;
                        Debug.Log($"✓ 存档有效: {file.Name}");
                    }
                    else
                    {
                        Debug.LogError($"✗ 存档损坏: {file.Name}");
                    }
                }
            }

            Debug.Log($"存档验证完成: {validCount}/{totalCount} 个存档有效");
        }
    }
#endif

    #endregion
}