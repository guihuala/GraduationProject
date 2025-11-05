using System.Collections.Generic;

namespace GuiFramework
{
    public interface IStateMachineOwner { }

    /// <summary>
    /// 有限状态机
    /// </summary>
    public class StateMachine
    {
        /// <summary>当前状态类型值</summary>
        public int CurrStateType { get; private set; } = -1;

        /// <summary>当前激活状态实例</summary>
        private StateBase currStateObj;

        /// <summary>拥有者</summary>
        private IStateMachineOwner owner;

        /// <summary>已创建/缓存的状态：Key=状态类型值</summary>
        private Dictionary<int, StateBase> stateDic = new Dictionary<int, StateBase>();

        /// <summary>初始化</summary>
        public void Init(IStateMachineOwner owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T">新状态类型（需继承 StateBase）</typeparam>
        /// <param name="newStateType">新状态类型值</param>
        /// <param name="reCurrstate">若与当前相同，是否仍然刷新</param>
        public bool ChangeState<T>(int newStateType, bool reCurrstate = false) where T : StateBase, new()
        {
            if (newStateType == CurrStateType && !reCurrstate) return false;

            // 退出旧状态并移除回调
            if (currStateObj != null)
            {
                currStateObj.RemoveUpdate(currStateObj.Update);
                currStateObj.RemoveLateUpdate(currStateObj.LateUpdate);
                currStateObj.RemoveFixedUpdate(currStateObj.FixedUpdate);
                currStateObj.Exit();
            }

            // 进入新状态
            currStateObj = GetState<T>(newStateType);
            CurrStateType = newStateType;

            currStateObj.OnUpdate(currStateObj.Update);
            currStateObj.OnLateUpdate(currStateObj.LateUpdate);
            currStateObj.OnFixedUpdate(currStateObj.FixedUpdate);
            currStateObj.Enter();

            return true;
        }

        /// <summary>从对象池/缓存获取状态实例</summary>
        private StateBase GetState<T>(int stateType) where T : StateBase, new()
        {
            if (stateDic.ContainsKey(stateType)) return stateDic[stateType];

            StateBase state = PoolManager.Instance.GetObject<T>();
            state.Init(owner, stateType, this);
            stateDic.Add(stateType, state);
            return state;
        }

        /// <summary>
        /// 停止状态机（退出当前状态，清空并归还所有已创建状态）
        /// </summary>
        public void Stop()
        {
            if (currStateObj != null)
            {
                currStateObj.Exit();
                currStateObj.RemoveUpdate(currStateObj.Update);
                currStateObj.RemoveLateUpdate(currStateObj.LateUpdate);
                currStateObj.RemoveFixedUpdate(currStateObj.FixedUpdate);
            }

            CurrStateType = -1;
            currStateObj = null;

            // 归还缓存状态
            var enumerator = stateDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.UnInit();
            }
            stateDic.Clear();
        }

        /// <summary>
        /// 销毁状态机（兼容旧拼写 Destory）
        /// </summary>
        public void Destroy()
        {
            Stop();
            owner = null;
            this.ObjectPushPool();
        }

        /// <summary>
        /// 兼容旧接口：Destory（请改用 Destroy）
        /// </summary>
        [System.Obsolete("请改用 Destroy()", false)]
        public void Destory() => Destroy();
    }
}
