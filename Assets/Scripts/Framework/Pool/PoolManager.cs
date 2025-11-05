using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

namespace GuiFramework
{
    /// <summary>
    /// 对象池管理器（单例）：
    /// - 管理 GameObject 池与普通对象池
    /// - 支持预热（initSpawnAmount）
    /// - 提供清理接口
    /// </summary>
    public class PoolManager : Singleton<PoolManager>
    {
        [SerializeField] private GameObject poolRootObj;

        /// <summary>GameObject 对象池：Key=prefab 名称</summary>
        public Dictionary<string, GameObjectPoolData> gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();

        /// <summary>普通对象池：Key=类型全名</summary>
        public Dictionary<string, ObjectPoolData> objectPoolDic = new Dictionary<string, ObjectPoolData>();

        /// <summary>预热数量（当无池时先生成并回收该数量）</summary>
        public int initSpawnAmount = 3;

        #region GameObject 池

        /// <summary>
        /// 获取 GameObject；若池不存在则预热后重试
        /// </summary>
        public GameObject GetGameObject(string assetName, GameObject prefab = null, Transform parent = null)
        {
            GameObject obj = null;

            if (!gameObjectPoolDic.ContainsKey(assetName))
            {
                // 预热
                if (prefab != null)
                {
                    for (int i = 0; i < initSpawnAmount; i++)
                    {
                        obj = UnityEngine.Object.Instantiate(prefab);
                        PushGameObject(obj);
                    }
                }
                // 预热后重试
                return GetGameObject(assetName);
            }

            if (gameObjectPoolDic.TryGetValue(assetName, out GameObjectPoolData poolData) &&
                poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(parent);
            }

            return obj;
        }

        /// <summary>回收 GameObject 到对象池</summary>
        public void PushGameObject(GameObject obj)
        {
            string name = obj.name.Replace("(Clone)", "");
            if (gameObjectPoolDic.TryGetValue(name, out GameObjectPoolData poolData))
            {
                poolData.PushObj(obj);
            }
            else
            {
                gameObjectPoolDic.Add(name, new GameObjectPoolData(obj, poolRootObj));
            }
        }

        #endregion

        #region 普通对象池

        /// <summary>获取普通对象；如无可用则批量创建并回收，然后重试</summary>
        public T GetObject<T>() where T : class, new()
        {
            T obj;
            if (CheckObjectCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)objectPoolDic[name].GetObj();
                return obj;
            }
            else
            {
                for (int i = 0; i < initSpawnAmount; i++)
                {
                    obj = new T();
                    PushObject(obj);
                }
                return GetObject<T>();
            }
        }

        /// <summary>回收普通对象</summary>
        public void PushObject(object obj)
        {
            string name = obj.GetType().FullName;
            if (objectPoolDic.ContainsKey(name))
            {
                objectPoolDic[name].PushObj(obj);
            }
            else
            {
                objectPoolDic.Add(name, new ObjectPoolData(obj));
            }
        }

        private bool CheckObjectCache<T>()
        {
            string name = typeof(T).FullName;
            return objectPoolDic.ContainsKey(name) && objectPoolDic[name].poolQueue.Count > 0;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理对象池
        /// </summary>
        /// <param name="clearGameObject">是否清空 GameObject 池（并销毁池中父节点）</param>
        /// <param name="clearCObject">是否清空普通对象池</param>
        public void Clear(bool clearGameObject = true, bool clearCObject = true)
        {
            if (clearGameObject)
            {
                for (int i = 0; i < poolRootObj.transform.childCount; i++)
                {
                    UnityEngine.Object.Destroy(poolRootObj.transform.GetChild(i).gameObject);
                }
                gameObjectPoolDic.Clear();
            }

            if (clearCObject)
            {
                objectPoolDic.Clear();
            }
        }

        public void ClearAllGameObject() => Clear(true, false);

        public void ClearGameObject(string prefabName)
        {
            var child = poolRootObj.transform.Find(prefabName);
            if (child != null)
            {
                UnityEngine.Object.Destroy(child.gameObject);
                gameObjectPoolDic.Remove(prefabName);
            }
        }

        public void ClearGameObject(GameObject prefab) => ClearGameObject(prefab.name);

        public void ClearAllObject() => Clear(false, true);

        public void ClearObject<T>() => objectPoolDic.Remove(typeof(T).FullName);

        public void ClearObject(Type type) => objectPoolDic.Remove(type.FullName);

        #endregion
    }
}
