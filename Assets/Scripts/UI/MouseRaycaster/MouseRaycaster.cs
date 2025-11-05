using UnityEngine;

public class MouseRaycaster : Singleton<MouseRaycaster>
{
    public float raycastMaxDistance = 100f;
    private Vector3 _m_vMousePosition;
    int layerIndex;
    int layerMask;

    private Camera mainCamera;
    private RaycastHit hitInfo;
    private Ray ray;

    private void Start()
    {
        layerIndex = LayerMask.NameToLayer("Ground");
        layerMask = 1 << layerIndex;
        mainCamera = GetComponent<Camera>();
    }

    public Vector3 GetMousePosi()
    {
        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 发射射线 检测碰撞
        if (Physics.Raycast(ray, out hitInfo, raycastMaxDistance, layerMask))
        {
            Debug.DrawLine(this.transform.position, hitInfo.point);
            // 射线碰撞到了物体 获取碰撞到的交点
            return hitInfo.point;
        }
        else return Vector3.zero;
    }
    
    public Vector3 GetDirFromMouseToPosi(Vector3 posi)
    {
        Vector3 mousePosi = GetMousePosi();
        if (Vector3.Distance(posi, mousePosi) <= 5f) return Vector3.zero;
        Vector3 dir = (mousePosi - posi).normalized;
        dir.y = 0;
        return dir;
    }
}