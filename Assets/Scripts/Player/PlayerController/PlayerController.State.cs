using UnityEngine;

public partial class PlayerController
{
    public void Rotate()
    {
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