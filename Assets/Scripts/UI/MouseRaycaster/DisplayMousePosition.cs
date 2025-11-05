using UnityEngine;


public class DisplayMousePosition : MonoBehaviour
{
    [SerializeField] private GameObject showMousePrefab;
    Vector3 lastFrameMousePosi = Vector3.zero;
    private GameObject _m_gMouseObj;

    void Start()
    {
        _m_gMouseObj = Instantiate(showMousePrefab);
    }
    
    void Update()
    {
        if (lastFrameMousePosi != MouseRaycaster.Instance.GetMousePosi()) updateMouseShow();
    }
    
    private void updateMouseShow()
    {
        lastFrameMousePosi = MouseRaycaster.Instance.GetMousePosi();
        _m_gMouseObj.transform.position = lastFrameMousePosi;
    }
}