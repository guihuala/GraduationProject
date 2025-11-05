using System.Collections.Generic;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// GameObject 对象池数据
    /// </summary>
    public class GameObjectPoolData
    {
        /// <summary>该类目（prefab 名称）在对象池中的父节点</summary>
        public GameObject fatherObj;

        /// <summary>对象容器</summary>
        public Queue<GameObject> poolQueue;

        public GameObjectPoolData(GameObject obj, GameObject poolRootObj)
        {
            // 以 prefab 名称创建父节点，挂到对象池根节点下
            fatherObj = new GameObject(obj.name);
            fatherObj.transform.SetParent(poolRootObj.transform);
            poolQueue = new Queue<GameObject>();
            // 首次创建时将传入对象回收到容器
            PushObj(obj);
        }

        /// <summary>将对象放入对象池</summary>
        public void PushObj(GameObject obj)
        {
            poolQueue.Enqueue(obj);
            obj.transform.SetParent(fatherObj.transform);
            obj.SetActive(false);
        }

        /// <summary>从对象池中取出对象</summary>
        public GameObject GetObj(Transform parent = null)
        {
            GameObject obj = poolQueue.Dequeue();
            obj.SetActive(true);
            obj.transform.SetParent(parent);

            if (parent == null)
            {
                // 回归到当前活动场景
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
                    obj,
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            return obj;
        }
    }
}