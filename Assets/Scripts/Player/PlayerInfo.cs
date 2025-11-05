using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    #region --私有字段--

    private PlayerBaseData playerBaseData;

    private Quaternion playerRotate;

    private PlayerState playerState;

    private Vector3 mouseWorldPos;

    private Vector3 playerBottomPos;

    private Vector3 rotateDir;

    #endregion


    #region --属性字段--

    public float RunSpeed => playerBaseData.RunSpeed;

    public float WalkSpeed => playerBaseData.WalkSpeed;

    // 玩家基础信息属性
    public Quaternion PlayerRotate
    {
        get => playerRotate;
        set => playerRotate = value;
    }

    public Vector3 MouseWorldPos
    {
        get => mouseWorldPos;
        set => mouseWorldPos = value;
    }

    public Vector3 PlayerBottomPos
    {
        get => playerBottomPos;
        set => playerBottomPos = value;
    }

    public Vector3 RotateDir
    {
        get => rotateDir;
        set => rotateDir = value;
    }

    public PlayerState PlayerState
    {
        get => playerState;
        set => playerState = value;
    }

    #endregion

    #region --数据方法--

    public void Init(PlayerBaseData playerData)
    {
        playerBaseData = playerData;

        // 玩家旋转信息
        playerRotate = Quaternion.identity;

        // 玩家状态
        playerState = PlayerState.Idle;

        // 位置和方向信息
        mouseWorldPos = Vector3.zero;
        playerBottomPos = Vector3.zero;
        rotateDir = Vector3.zero;
    }

    #endregion
}