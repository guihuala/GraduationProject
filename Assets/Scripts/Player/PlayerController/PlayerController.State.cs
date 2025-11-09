using UnityEngine;

public partial class PlayerController
{
    public void Rotate()
    {
        // 获取鼠标在地面上的世界坐标
        if (MouseRaycaster.Instance == null) return;
        Vector3 mouseWorld = MouseRaycaster.Instance.GetMousePosi();
        if (mouseWorld == Vector3.zero) return; // 未命中地面

        // 计算水平面上的朝向（忽略y轴差值）
        Vector3 dir = mouseWorld - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        float t = Mathf.Clamp01(rotationSpeed * Time.deltaTime);

        // 如果有刚体，优先使用刚体旋转（非物理驱动的平滑旋转），否则直接设置 transform
        if (Rb != null)
        {
            // 使用 Rigidbody 的 rotation（插值）以兼容物理对象
            Rb.rotation = Quaternion.Slerp(Rb.rotation, targetRot, t);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, t);
        }
    }

    public void ChangeState(PlayerState playerState, bool reCurrstate = false)
    {
        switch (playerState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<PlayerIdleState>((int)playerState, reCurrstate);
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<PlayerMoveState>((int)playerState, reCurrstate);
                break;
            case PlayerState.Dash:
                stateMachine.ChangeState<PlayerDashState>((int)playerState, reCurrstate);
                break;
            case PlayerState.Dead:
                stateMachine.ChangeState<PlayerDeadState>((int)playerState, reCurrstate);
                break;
            default:
                break;
        }
    }
    
    public void PlayAnimation(string animationName)
    {
        PlayerAnimator.PlayAnimation(animationName);
    }
}