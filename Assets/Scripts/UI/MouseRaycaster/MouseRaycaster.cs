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

    [Tooltip("If raycast misses, use this Y for fallback ground plane intersection")]
    public float fallbackGroundY = 0f;

    private void Start()
    {
        layerIndex = LayerMask.NameToLayer("Ground");
        if (layerIndex < 0)
        {
            Debug.LogWarning("MouseRaycaster: 'Ground' layer not found. Raycasts will use default (all) layers.");
            layerMask = ~0; // all layers
        }
        else
        {
            layerMask = 1 << layerIndex;
        }

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("MouseRaycaster: No Camera found on this GameObject and Camera.main is null. Mouse raycasting will not work.");
            }
            else
            {
                Debug.LogWarning("MouseRaycaster: Camera component not found on this GameObject, falling back to Camera.main");
            }
        }

        _m_vMousePosition = Vector3.zero;
    }

    public Vector3 GetMousePosi()
    {
        if (mainCamera == null)
        {
            // try to recover
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("MouseRaycaster: No camera available to cast rays.");
                return Vector3.zero;
            }
        }

        ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 发射射线 检测碰撞
        if (Physics.Raycast(ray, out hitInfo, raycastMaxDistance, layerMask))
        {
            Debug.DrawLine(this.transform.position, hitInfo.point);
            // 射线碰撞到了物体 获取碰撞到的交点
            _m_vMousePosition = hitInfo.point;
            return hitInfo.point;
        }
        else
        {
            // Raycast 没有命中 Ground 层，使用水平面（fallbackGroundY）与射线求交作为回退
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, fallbackGroundY, 0f));
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 point = ray.GetPoint(enter);
                _m_vMousePosition = point;
                return point;
            }

            // 如果都失败，返回上一次有效位置（如果有），否则 Vector3.zero
            if (_m_vMousePosition != Vector3.zero)
                return _m_vMousePosition;

            return Vector3.zero;
        }
    }
    
    public Vector3 GetDirFromMouseToPosi(Vector3 posi)
    {
        Vector3 mousePosi = GetMousePosi();
        if (mousePosi == Vector3.zero) return Vector3.zero;
        if (Vector3.Distance(posi, mousePosi) <= 5f) return Vector3.zero;
        Vector3 dir = (mousePosi - posi).normalized;
        dir.y = 0;
        return dir;
    }
}