using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour, IMountMovement, IMountMovementConfigurable
{
    private CarDataSO _Data;
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

    public void ApplyData(MountDataSO data)
    {
        _Data = data as CarDataSO;
        if (_Data == null)
            Debug.LogError($"[{name}] CarMovement인데 CarDataSO가 아닙니다.", this);
    }


    public void FixedTick()
    {
        if (_Data == null) return;

        // 전진/후진
        Vector3 force = transform.forward * (_Input.Throttle * _Data.Accel);
        _Rb.AddForce(force, ForceMode.Acceleration);

        // 조향
        float yaw = _Input.Steer * _Data.TurnDegPerSec * Time.fixedDeltaTime;
        _Rb.MoveRotation(_Rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // 브레이크
        _Rb.linearDamping = _Input.Brake ? _Data.BrakeDrag : 0f;
    }
}
