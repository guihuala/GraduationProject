using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerDashState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("fall");
        //todo
        player.Rb.AddForce(player.transform.forward * 100f, ForceMode.Impulse);
    }
}