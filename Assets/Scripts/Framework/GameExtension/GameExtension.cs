using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace GuiFramework
{
    /// <summary>
    /// 常用扩展方法集合：
    /// - Attribute 读取
    /// - 数组内容相等判断
    /// - 对象池便捷入口（GameObject / object）
    /// - 非 MonoBehaviour 下的 Update/LateUpdate/FixedUpdate 事件注册与协程代理
    /// </summary>
    public static class KidGameExtension
    {
        #region 通用

        /// <summary>获取类型上的自定义特性</summary>
        public static T GetAttribute<T>(this object obj) where T : Attribute
        {
            return obj.GetType().GetCustomAttribute<T>();
        }

        /// <summary>获取指定 Type 上的自定义特性</summary>
        public static T GetAttribute<T>(this object obj, Type type) where T : Attribute
        {
            return type.GetCustomAttribute<T>();
        }

        /// <summary>数组内容逐项比较（长度与每项 Equals）</summary>
        public static bool ArraryEquals(this object[] objs, object[] other)
        {
            if (other == null || objs == null || objs.Length != other.Length) return false;
            for (int i = 0; i < objs.Length; i++)
            {
                if (!Equals(objs[i], other[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// 正确拼写别名：ArrayEquals（保持旧方法兼容）
        /// </summary>
        public static bool ArrayEquals(this object[] objs, object[] other) => ArraryEquals(objs, other);

        #endregion

        #region 资源回收（对象池）

        /// <summary>GameObject 放回对象池</summary>
        public static void GameObjectPushPool(this GameObject go)
        {
            PoolManager.Instance.PushGameObject(go);
        }

        /// <summary>GameObject 放回对象池（Component 扩展）</summary>
        public static void GameObjectPushPool(this Component com)
        {
            GameObjectPushPool(com.gameObject);
        }

        /// <summary>普通对象放回对象池</summary>
        public static void ObjectPushPool(this object obj)
        {
            PoolManager.Instance.PushObject(obj);
        }

        #endregion

        #region Mono 调度

        /// <summary>注册 Update 回调</summary>
        public static void OnUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddUpdateListener(action);
        }

        /// <summary>移除 Update 回调</summary>
        public static void RemoveUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveUpdateListener(action);
        }

        /// <summary>注册 LateUpdate 回调</summary>
        public static void OnLateUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddLateUpdateListener(action);
        }

        /// <summary>移除 LateUpdate 回调</summary>
        public static void RemoveLateUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveLateUpdateListener(action);
        }

        /// <summary>注册 FixedUpdate 回调</summary>
        public static void OnFixedUpdate(this object obj, Action action)
        {
            MonoManager.Instance.AddFixedUpdateListener(action);
        }

        /// <summary>移除 FixedUpdate 回调</summary>
        public static void RemoveFixedUpdate(this object obj, Action action)
        {
            MonoManager.Instance.RemoveFixedUpdateListener(action);
        }

        /// <summary>开启协程（代理到 MonoManager）</summary>
        public static Coroutine StartCoroutine(this object obj, IEnumerator routine)
        {
            return MonoManager.Instance.StartCoroutine(routine);
        }

        /// <summary>停止协程（代理到 MonoManager）</summary>
        public static void StopCoroutine(this object obj, Coroutine routine)
        {
            MonoManager.Instance.StopCoroutine(routine);
        }

        /// <summary>停止所有协程（代理到 MonoManager）</summary>
        public static void StopAllCoroutines(this object obj)
        {
            MonoManager.Instance.StopAllCoroutines();
        }

        #endregion

        #region GameObject 判空

        public static bool IsNull(this GameObject obj)
        {
            return ReferenceEquals(obj, null);
        }

        #endregion
    }
}
