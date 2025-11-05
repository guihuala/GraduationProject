using System;
using UnityEngine;

/// <summary>
/// 为未继承 MonoBehaviour 的脚本提供 Update / LateUpdate / FixedUpdate 的回调分发；
/// 也提供若干 Instantiate 便捷方法。
/// </summary>
public class MonoManager : Singleton<MonoManager>
{
    private Action _updateEvent;
    private Action _lateUpdateEvent;
    private Action _fixedUpdateEvent;

    /// <summary>添加 Update 监听</summary>
    public void AddUpdateListener(Action action) => _updateEvent += action;

    /// <summary>移除 Update 监听</summary>
    public void RemoveUpdateListener(Action action) => _updateEvent -= action;

    /// <summary>添加 LateUpdate 监听</summary>
    public void AddLateUpdateListener(Action action) => _lateUpdateEvent += action;

    /// <summary>移除 LateUpdate 监听</summary>
    public void RemoveLateUpdateListener(Action action) => _lateUpdateEvent -= action;

    /// <summary>添加 FixedUpdate 监听</summary>
    public void AddFixedUpdateListener(Action action) => _fixedUpdateEvent += action;

    /// <summary>移除 FixedUpdate 监听</summary>
    public void RemoveFixedUpdateListener(Action action) => _fixedUpdateEvent -= action;

    private void Update() => _updateEvent?.Invoke();
    private void LateUpdate() => _lateUpdateEvent?.Invoke();
    private void FixedUpdate() => _fixedUpdateEvent?.Invoke();

    public GameObject InstantiateGameObject(GameObject obj)
    {
        return UnityEngine.Object.Instantiate(obj);
    }

    public GameObject InstantiateGameObject(GameObject obj, Vector3 pos, Quaternion rotation)
    {
        return UnityEngine.Object.Instantiate(obj, pos, rotation);
    }

    public GameObject InstantiateGameObject(GameObject obj, Vector3 pos, Quaternion rotation, float delayToDestroy)
    {
        GameObject go = UnityEngine.Object.Instantiate(obj, pos, rotation);
        UnityEngine.Object.Destroy(go, delayToDestroy);
        return go;
    }
}

