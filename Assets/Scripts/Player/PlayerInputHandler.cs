using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    private PlayerInputActions playerInputActions;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        // 初始化PlayerInputActions
        playerInputActions = new PlayerInputActions();
        
        // 获取PlayerMovement组件
        playerMovement = GetComponent<PlayerMovement>();

        // 设置输入回调
        playerInputActions.Player.AddCallbacks(this);
    }

    private void OnEnable()
    {
        // 启用输入
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // 禁用输入
        playerInputActions.Player.Disable();
    }

    // 当玩家移动时，处理输入并传递给PlayerMovement
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        playerMovement.SetMovementInput(movementInput);
    }
    
    public void OnInteract(InputAction.CallbackContext context) { }
    public void OnPause(InputAction.CallbackContext context) { }
    public void OnSpeedUp(InputAction.CallbackContext context) { }
    
    #region Utility Methods

    // public string GetSettingKey(InputActionType actionType, int controlTypeIndex)
    // {
    //     if (inputActionAsset == null)
    //     {
    //         Debug.LogError("InputActionAsset is not assigned");
    //         return string.Empty;
    //     }
    //
    //     var action = inputActionAsset.FindAction(actionType.ToString());
    //     if (action == null)
    //     {
    //         Debug.LogError($"Could not find action '{actionType}' in input actions");
    //         return string.Empty;
    //     }
    //     //只展示键位 其他信息都不展示
    //     return action.bindings[controlTypeIndex].ToDisplayString(DisplayStringOptions.DontIncludeInteractions);
    // }
    //     
    // public Vector3 CameraRelativeMoveDir(Transform cam)
    // {
    //     Vector2 in2 = MoveDir();
    //     if (in2.sqrMagnitude < 0.0001f) return Vector3.zero;
    //         
    //     Vector3 fwd = cam.forward; fwd.y = 0f; fwd.Normalize();
    //     Vector3 right = cam.right; right.y = 0f; right.Normalize();
    //         
    //     Vector3 move = fwd * in2.y + right * in2.x;
    //     return move.normalized;
    // }
        
    #endregion
}