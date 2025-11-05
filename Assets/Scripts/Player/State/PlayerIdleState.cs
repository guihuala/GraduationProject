using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("idle");
    }

    public override void Update()
    {
        base.Update();
        player.Rotate();
        Vector2 inputVal = player.InputSettings.MoveDir();
        
        if (inputVal != Vector2.zero)
        {
            player.ChangeState(PlayerState.Move);
        }
    }
}