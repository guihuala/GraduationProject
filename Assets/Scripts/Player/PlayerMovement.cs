using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 movementInput;
    public float moveSpeed = 5f;  // 玩家移动速度
    private void Update()
    {
        // 执行玩家移动
        MovePlayer();
    }

    // 设置玩家移动输入
    public void SetMovementInput(Vector2 input)
    {
        movementInput = input;
    }

    // 玩家移动逻辑
    private void MovePlayer()
    {
        // 将输入转换为3D空间的移动（忽略Z轴）
        Vector3 movement = new Vector3(movementInput.x, 0f, movementInput.y) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}