using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoLoader : SingletonNoMono<SoLoader>
{
    public Dictionary<string, ScriptableObject> soDic;

    //装Entity的 id -> Entity 也就是一行数据的字典
    private Dictionary<string, object> entityDic;

    public void Init()
    {
        soDic = new Dictionary<string, ScriptableObject>();
        entityDic = new Dictionary<string, object>();

        ScriptableObject[] allSobj = Resources.LoadAll<ScriptableObject>("ScriptObject");

        Debug.Log($"共找到 {allSobj.Length} 个 ScriptableObject");
        foreach (var so in allSobj)
        {
            if (so != null)
            {
                if (!soDic.ContainsKey(so.name)) soDic.Add(so.name, so);
                else Debug.LogWarning("重复加载！！！！");
            }
            else Debug.LogWarning("So为空异常！！！");
        }
    }

    public data_config GetGameDataConfig()
    {
        return soDic[SoConst.DATA_CONFIG] as data_config;
    }

    /// <summary>
    /// 通过id  配表名 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dataName"></param>
    /// <param name="soName"></param>
    /// <returns></returns>
    private object GetDataById(string id, string dataName, string soName)
    {
        // 直接从缓存中查找
        string compositeKey = $"{dataName}_{id}";
        if (entityDic.TryGetValue(compositeKey, out object cachedData))
        {
            return cachedData;
        }

        // 缓存中没有，从SO中查找

        if (!soDic.TryGetValue(soName, out ScriptableObject so))
        {
            Debug.LogError("�Ҳ���SO: " + soName);
            return null;
        }

        // 在SO中查找实体列表
        object foundEntity = FindEntityInSo(so, id);
        if (foundEntity != null)
        {
            // 添加到缓存
            entityDic[compositeKey] = foundEntity;
            return foundEntity;
        }

        Debug.LogWarning($"在SO '{soName}' 中未找到ID为 {id} 的实体");
        return null;
    }

    private object FindEntityInSo(ScriptableObject so, string id)
    {
        //field是字段    value是拿值
        //GetProperty是拿属性    public string Id { get; set; } // id 是属性
        var fields = so.GetType().GetFields();

        foreach (var field in fields)
        {
            // 检查字段是否为 List<> 类型

            if (field.FieldType.IsGenericType &&
                field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                //从so这个fields获取一个field

                var entityList = field.GetValue(so);
                if (entityList == null) continue;

                // 更高效的方式 直接转换为 IList 避免反射 Count

                var list = entityList as System.Collections.IList;
                if (list == null) continue;

                for (int i = 0; i < list.Count; i++)
                {
                    var entity = list[i]; // 直接通过索引访问 
                    if (entity == null) continue;

                    // 检查实体是否有 "id" 字段

                    var idField = entity.GetType().GetField("id");
                    if (idField != null)
                    {
                        string entityId = idField.GetValue(entity)?.ToString();
                        if (entityId == id)
                        {
                            return entity;
                        }
                    }
                }
            }
        }

        return null;
    }
}