using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// 单条配表集合类 用于第一行是标题 下面全是配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SCRefDataList<T> where T : SCRefDataCore, new()
    {
        private List<T> _m_refDataList;
        public List<T> refDataList => _m_refDataList;

        private string _m_assetPath;
        private string _m_sheetName;

        public SCRefDataList(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
            _m_refDataList = new List<T>();
        }

        public void readFromJson()
        {
            if (string.IsNullOrEmpty(_m_assetPath) || string.IsNullOrEmpty(_m_sheetName))
            {
                Debug.LogError(_m_assetPath + "或" + _m_sheetName + "没有信息，导出失败！");
                return;
            }

            string jsonPath = "Assets/Resources/RefData/ExportJson/" + _m_sheetName + ".json";
            if (!File.Exists(jsonPath))
            {
                Debug.LogError("JSON 文件不存在：" + jsonPath);
                return;
            }

            string jsonData = File.ReadAllText(jsonPath);
            parseFromJson(jsonData);
        }

        protected void parseFromJson(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                Debug.LogError("JSON 数据为空");
                return;
            }

            // 反序列化 JSON 数据
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonString);
            foreach (var row in data)
            {
                T dataCore = new T();
                // 直接将每行数据存储到 _m_keyValueMap 中
                foreach (var pair in row)
                {
                    dataCore._m_keyValueMap.Add(pair.Key, pair.Value);
                }
                _m_refDataList.Add(dataCore);
            }
        }
    }
}
