using GuiFramework;
using UnityEngine;


/// <summary>
/// 玩家控制器
/// </summary>
public partial class PlayerController : Singleton<PlayerController>, IStateMachineOwner
{
    private InputSettings inputSettings;
    public InputSettings InputSettings => inputSettings;

    private Rigidbody rb;
    public Rigidbody Rb => rb;
    
    public PlayerAnimator PlayerAnimator;
    private StateMachine stateMachine;

    [Header("Rotation")]
    [Tooltip("旋转速度，越大越快")] [SerializeField]
    private float rotationSpeed = 10f;
    
    protected override void Awake()
    {
        base.Awake();
        inputSettings = GetComponent<InputSettings>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        // 每帧调用旋转，使角色面向鼠标指针（如果有命中点）
        Rotate();
    }

    public void Init()
    {
        //初始化状态机
        stateMachine = PoolManager.Instance.GetObject<StateMachine>();
        stateMachine.Init(this);

        //初始化为Idle状态
        ChangeState(PlayerState.Idle);
    }
}