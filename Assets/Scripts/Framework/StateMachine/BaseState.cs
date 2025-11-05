namespace GuiFramework
{
    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class StateBase
    {
        protected StateMachine stateMachine;

        /// <summary>
        /// 初始化状态（状态对象第一次创建时调用）
        /// </summary>
        /// <param name="owner">状态拥有者</param>
        /// <param name="stateType">状态枚举值</param>
        /// <param name="stateMachine">所属状态机</param>
        public virtual void Init(IStateMachineOwner owner, int stateType, StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// 反初始化（对象归还池时调用；可释放引用以避免 GC 压力）
        /// </summary>
        public virtual void UnInit()
        {
            // 自身回收
            this.ObjectPushPool();
        }

        /// <summary>进入状态（每次切换到该状态时执行）</summary>
        public virtual void Enter() { }

        /// <summary>退出状态</summary>
        public virtual void Exit() { }

        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void FixedUpdate() { }
    }
}