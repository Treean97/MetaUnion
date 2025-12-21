using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour, IMountMovement
{
    [SerializeField] private float _Accel = 25f;
    [SerializeField] private float _TurnDegPerSec = 120f;
    [SerializeField] private float _BrakeDrag = 3f;

    private Rigidbody _Rb;
    private MountInput _Input;

    void Awake()
    {
        _Rb = GetComponent<Rigidbody>();
    }

    public void SetInput(in MountInput input)
    {
        _Input = input;
    }

    public void FixedTick()
    {
        // 전진/후진
        Vector3 force = transform.forward * (_Input.Throttle * _Accel);
        _Rb.AddForce(force, ForceMode.Acceleration);

        // 조향
        float yaw = _Input.Steer * _TurnDegPerSec * Time.fixedDeltaTime;
        _Rb.MoveRotation(_Rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // 브레이크
        _Rb.linearDamping = _Input.Brake ? _BrakeDrag : 0f;
    }
}
