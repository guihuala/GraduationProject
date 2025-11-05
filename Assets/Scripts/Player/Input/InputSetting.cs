using System;
using GuiFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputBinding;

public class InputSettings : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActionAsset inputActionAsset;
    private ControlMap currentControlMap;

    private InputAction interactionAction;
    private InputAction moveAction;
    private InputAction dashAction;
    private InputAction gamePauseAction;
    
    private InputAction UI_interactionAction;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        inputActionAsset = playerInput.actions;
        currentControlMap = ControlMap.GameMap;
        playerInput.currentActionMap = inputActionAsset.actionMaps[(int)currentControlMap];
        Init();
    }

    private void OnDestroy()
    {
        Discard();
    }

    private void Init()
    {
        //gamemap
        moveAction = inputActionAsset.FindAction("Move");
        interactionAction = inputActionAsset.FindAction("Interaction");
        dashAction = inputActionAsset.FindAction("Dash");
        gamePauseAction = inputActionAsset.FindAction("GamePause");
        // uimap
        UI_interactionAction = inputActionAsset.FindAction("UI_Interaction");
        
        interactionAction.performed += OnInteractionActionPerformed;

        UI_interactionAction.performed += OnInteractionActionPerformed;

        dashAction.performed += OnDashActionPerformed;
        gamePauseAction.performed += OnGamePauseActionPerformed;

        MsgCenter.RegisterMsg(MsgConst.ON_CONTROL_MAP_CHG, OnInputMapChg);
    }

    private void Discard()
    {
        interactionAction.performed -= OnInteractionActionPerformed;
        dashAction.performed -= OnDashActionPerformed;
        gamePauseAction.performed -= OnGamePauseActionPerformed;

        UI_interactionAction.performed -= OnInteractionActionPerformed;
        
        MsgCenter.UnregisterMsg(MsgConst.ON_CONTROL_MAP_CHG, OnInputMapChg);
    }

    public Vector2 MoveDir()
    {
        Vector2 inputDir = moveAction.ReadValue<Vector2>();
        return inputDir.normalized;
    }

    public virtual bool GetDashDown() => dashAction.WasPressedThisFrame();

    public virtual bool GetGamePauseDown() => gamePauseAction.WasPerformedThisFrame();

    #region Input Action Callbacks

    private void OnInteractionActionPerformed(InputAction.CallbackContext context)
    {
        if (currentControlMap == ControlMap.GameMap)
        {
            MsgCenter.SendMsgAct(MsgConst.ON_INTERACTION_PRESS);
        }
        else
        {
            MsgCenter.SendMsgAct(MsgConst.ON_UI_INTERACTION_PRESS);
        }
    }

    private void OnDashActionPerformed(InputAction.CallbackContext context)
    {
        MsgCenter.SendMsgAct(MsgConst.ON_DASH_PRESS);
    }
    
    private void OnGamePauseActionPerformed(InputAction.CallbackContext context)
    {
        MsgCenter.SendMsgAct(MsgConst.ON_GAMEPAUSE_PRESS);
    }

    #endregion

    #region Utility Methods

    public string GetSettingKey(InputActionType actionType, int controlTypeIndex)
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("InputActionAsset is not assigned");
            return string.Empty;
        }

        var action = inputActionAsset.FindAction(actionType.ToString());
        if (action == null)
        {
            Debug.LogError($"Could not find action '{actionType}' in input actions");
            return string.Empty;
        }

        //只展示键位 其他信息都不展示
        return action.bindings[controlTypeIndex].ToDisplayString(DisplayStringOptions.DontIncludeInteractions);
    }

    public Vector3 CameraRelativeMoveDir(Transform cam)
    {
        Vector2 in2 = MoveDir();
        if (in2.sqrMagnitude < 0.0001f) return Vector3.zero;

        Vector3 fwd = cam.forward;
        fwd.y = 0f;
        fwd.Normalize();
        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = fwd * in2.y + right * in2.x;
        return move.normalized;
    }

    #endregion

    #region RegisterCallbacks

    private void OnInputMapChg(object[] objs)
    {
        if (objs == null || objs.Length == 0) return;
        currentControlMap = (ControlMap)objs[0];
        playerInput.currentActionMap = inputActionAsset.actionMaps[(int)currentControlMap];
        Discard();
        Init();
    }

    #endregion
}