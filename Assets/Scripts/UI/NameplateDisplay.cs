using UnityEngine;

public class NameplateDisplay : MonoBehaviour
{
    private Camera _Cam;

    public void SetDisplay()
    {
        _Cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!_Cam)
        {
            return;
        }

        // 카메라 방향 주시
        Vector3 dir = transform.position - _Cam.transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
