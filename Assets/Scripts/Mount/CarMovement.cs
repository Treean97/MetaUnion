using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour, IMountMovement
{
    [SerializeField] private VehicleConfigSO _Config;

    private Rigidbody _Rb;
    private MountInput _Input;

    void Awake()
    {
        _Rb = GetComponent<Rigidbody>();

        if (_Config == null)
            Debug.LogError($"[{name}] VehicleConfigSO가 비어있습니다.", this);
    }

    public void SetInput(in MountInput input)
    {
        _Input = input;
    }

    public void FixedTick()
    {
        if (_Config == null) return;

        // 전진/후진
        Vector3 force = transform.forward * (_Input.Throttle * _Config.Accel);
        _Rb.AddForce(force, ForceMode.Acceleration);

        // 조향
        float yaw = _Input.Steer * _Config.TurnDegPerSec * Time.fixedDeltaTime;
        _Rb.MoveRotation(_Rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // 브레이크
        _Rb.linearDamping = _Input.Brake ? _Config.BrakeDrag : 0f;
    }
}
