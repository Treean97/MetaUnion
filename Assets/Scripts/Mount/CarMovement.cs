using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour, IMountMovement, IMountMovementConfigurable
{
    private CarDataSO _Data;
    private Rigidbody _Rb;
    private MountInput _Input;

    [Header("Minimal weighty arcade")]
    [SerializeField] private float _ThrottleSmooth = 6f;  // 높을수록 입력 반영이 빨라짐
    [SerializeField] private float _SteerSmooth = 8f;
    [SerializeField] private float _CoastDrag = 0.8f;     // 악셀 안 밟을 때 저항
    [SerializeField] private float _MaxSpeed = 10f;       // 최고속도 제한

    private float _T; // 스무딩된 throttle
    private float _S; // 스무딩된 steer

    void Awake()
    {
        _Rb = GetComponent<Rigidbody>();
    }

    public void SetInput(in MountInput input) => _Input = input;

    public void ApplyData(MountDataSO data)
    {
        _Data = data as CarDataSO;
        if (_Data == null)
            Debug.LogError($"[{name}] CarMovement인데 CarDataSO가 아닙니다.", this);
    }

    public void FixedTick()
    {
        if (_Data == null) return;

        float dt = Time.fixedDeltaTime;

        // 입력 스무딩
        _T = Mathf.MoveTowards(_T, Mathf.Clamp(_Input.Throttle, -1f, 1f), _ThrottleSmooth * dt);
        _S = Mathf.MoveTowards(_S, Mathf.Clamp(_Input.Steer, -1f, 1f), _SteerSmooth * dt);

        // 바닥 평면 전방
        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        // 최고속도 제한
        float speed = Vector3.Dot(_Rb.linearVelocity, fwd);
        if (Mathf.Abs(speed) < _MaxSpeed || Mathf.Sign(_T) != Mathf.Sign(speed))
        {
            Vector3 force = fwd * (_T * _Data.Accel);
            _Rb.AddForce(force, ForceMode.Acceleration);
        }

        // 조향
        float yaw = _S * _Data.TurnDegPerSec * dt;
        _Rb.MoveRotation(_Rb.rotation * Quaternion.Euler(0f, yaw, 0f));

        // 브레이크
        if (_Input.Brake) _Rb.linearDamping = _Data.BrakeDrag;
        else _Rb.linearDamping = (Mathf.Abs(_T) < 0.05f) ? _CoastDrag : 0f;
    }
}
