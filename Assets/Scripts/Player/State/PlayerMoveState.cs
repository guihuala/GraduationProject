using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    private Vector3 dir;
    private float speed = 5;
    private bool isRunning;

    public override void Enter()
    {
        player.PlayAnimation("walk");
        UpdateSpeed();
    }

    public override void Update()
    {
        base.Update();
        
        if (player.InputSettings.GetDashDown())
        {
            player.ChangeState(PlayerState.Dash);
            return;
        }
        
        if (isRunning)
        {
        }

        Vector3 move = player.InputSettings.CameraRelativeMoveDir(CameraController.Instance.transform);

        if (move == Vector3.zero)
        {
            player.ChangeState(PlayerState.Idle);
        }
        else
        {
            dir = move;
        }

        player.Rotate();
    }

    private void UpdateSpeed()
    {
    }

    public override void FixedUpdate()
    {
        player.Rb.velocity = dir * speed;
    }
}