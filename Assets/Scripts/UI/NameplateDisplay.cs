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

        Vector3 dir = transform.position - _Cam.transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
