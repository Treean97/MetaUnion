using UnityEngine;

public class NameplateDisplay : MonoBehaviour
{
    private Transform _Target;
    private Camera _Cam;


    public void SetDisplay(Transform target)
    {
        _Target = target;
        _Cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!_Cam || !_Target)
        {
            return;
        }

        // 카메라 방향 주시
        Vector3 dir = transform.position - _Cam.transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
