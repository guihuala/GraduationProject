using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// 单条配表类 用于只有key和value一一对应的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SCRefDataCore
    {
        private string _m_assetPath;//资产路径
        private string _m_sheetName;//
        public SCRefDataCore()
        {

        }

        public SCRefDataCore(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
        }

        protected string _assetPath
        {
            get { return _m_assetPath; }
        }

        protected string _sheetName
        {
            get { return _m_sheetName; }
        }

        public Dictionary<string, string> _m_keyValueMap = new Dictionary<string, string>();

        private const string EXCEL_EMPTY_FLAG = "*";

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

            _m_keyValueMap.Clear();

            // 解析 JSON 数据
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(jsonString);
            foreach (var row in data)
            {
                foreach (var pair in row)
                {
                    _m_keyValueMap.Add(pair.Key, pair.Value);
                }
            }

            _parseFromString();
        }

        protected abstract void _parseFromString();

        #region Get Methods

        protected string getString(string _key)
        {
            string result;
            if (!_m_keyValueMap.TryGetValue(_key, out result))
            {
                Debug.LogError($"_key不存在{_key}");
                return null;
            }

            return result;
        }

        protected int getInt(string _name)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }

            if (!int.TryParse(tempValue, out int result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是int");
            }

            return result;
        }

        protected object getEnum(string _name, Type _type)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }

            object obj = Enum.Parse(_type, tempValue);
            if (obj == null)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是enum");
            }

            return obj;
        }

        protected long getLong(string _name, bool _canNull = true)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }

            if (!long.TryParse(tempValue, out long result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是long");
            }

            return result;
        }

        protected bool getBool(string _name, bool _canNull = true)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return false;
            }

            if (!bool.TryParse(tempValue, out bool result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是bool");
            }

            return result;
        }

        protected float getFloat(string _name, bool _canNull = true)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }

            if (!float.TryParse(tempValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是float");
            }

            return result;
        }

        protected Vector2 getVector2(string _name, bool _canNull = true)
        {
            Vector2 v2 = new Vector2();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return v2;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 2)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是Vector2");
                return v2;
            }

            v2.Set(float.Parse(strs[0]), float.Parse(strs[1]));
            return v2;
        }

        protected Vector3 getVector3(string _name, bool _canNull = true)
        {
            Vector3 v3 = new Vector3();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return v3;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 3)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是Vector3");
                return v3;
            }

            v3.Set(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
            return v3;
        }

        protected Rect getRect(string _name, bool _canNull = true)
        {
            Rect rect = new Rect();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return rect;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 4)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是rect");
                return rect;
            }

            rect.Set(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]), float.Parse(strs[3]));
            return rect;
        }

        protected List<T> getList<T>(string _name, bool _canNull = true)
        {
            List<T> list = new List<T>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return list;
            }

            //空列表标识
            if (tempValue == EXCEL_EMPTY_FLAG)
                return list;

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            for (var i = 0; i < strs.Length; i++)
            {
                string tempStr = strs[i];
                object value = ParseValue(tempStr, typeof(T));
                if (value == null)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempStr}解析失败，完整数据：{value}");
                }
                else
                {
                    list.Add((T)value);
                }
            }


            return list;
        }

        protected List<Vector2> getVector2List(string _name, bool _canNull = true)
        {
            List<Vector2> vector2List = new List<Vector2>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return vector2List;
            }

            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    float x = float.Parse(valueStrs[0]);
                    float y = float.Parse(valueStrs[1]);
                    vector2List.Add(new Vector2(x, y));
                }
            }

            return vector2List;
        }

        protected List<Vector2Int> getVector2IntList(string _name, bool _canNull = true)
        {
            List<Vector2Int> Vector2IntList = new List<Vector2Int>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return Vector2IntList;
            }

            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    int x = int.Parse(valueStrs[0]);
                    int y = int.Parse(valueStrs[1]);
                    Vector2IntList.Add(new Vector2Int(x, y));
                }
            }

            return Vector2IntList;
        }

        protected List<Vector3> getVector3List(string _name, bool _canNull = true)
        {
            List<Vector3> vector3List = new List<Vector3>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return vector3List;
            }

            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 3)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    float x = float.Parse(valueStrs[0]);
                    float y = float.Parse(valueStrs[1]);
                    float z = float.Parse(valueStrs[2]);
                    vector3List.Add(new Vector3(x, y, z));
                }
            }

            return vector3List;
        }

        // 解析字段值
        protected static object ParseValue(string _value, Type _type)
        {
            try
            {
                if (_value.Equals(string.Empty))
                {
                    if (_type == typeof(string))
                    {
                        return "";
                    }

                    return Activator.CreateInstance(_type, true);
                }
                else
                {
                    _value = _value.Trim();

                    // 枚举 
                    if (_type.IsEnum)
                    {
                        return Enum.Parse(_type, _value);
                    }

                    // 字符串
                    else if (_type == typeof(string))
                    {
                        return _value;
                    }

                    // 浮点型
                    else if (_type == typeof(float))
                    {
                        if (_value == "0" || _value == "" || _value == string.Empty)
                            return 0f;

                        return float.Parse(_value, CultureInfo.InvariantCulture);
                    }

                    // 整形
                    else if (_type == typeof(int))
                    {
                        if (_value == "")
                            return 0;

                        return int.Parse(_value);
                    }

                    else if (_type == typeof(bool))
                    {
                        return bool.Parse(_value);
                    }

                    else if (_type == typeof(long))
                    {
                        return long.Parse(_value);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseValue type:{_type.ToString()}, value:{_value}, failed: {ex}");
            }

            return null;
        }

        #endregion
    }
}
