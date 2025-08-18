using UnityEngine;

public class NameplateDisplay : MonoBehaviour
{
    private Transform _Target;
    private Camera _Cam;


    public void SetTarget(Transform target)
    {
        _Target = target;
    }

    void Update()
    {
        if (!_Cam)
        {
            _Cam = Camera.main;
        }

        if (_Target)
        {
            transform.position = _Target.position;
        }

        if (_Cam)
        {
            Vector3 dir = transform.position - _Cam.transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }
}
