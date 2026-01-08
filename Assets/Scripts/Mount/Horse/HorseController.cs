using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HorseController : MonoBehaviour, IMountMovement, IMountMovementConfigurable
{
    [Header("Cache (optional)")]
    [SerializeField] private Animator _Animator;

    private HorseDataSO _Data;
    private Rigidbody _RB;
    private MountInput _Input;

    // animator hash (runtime)
    private int _H_Speed;
    private int _H_Turn;
    private int _H_Brake;

    void Awake()
    {
        _RB = GetComponent<Rigidbody>();
        if (_RB == null)
            Debug.LogError($"[{name}] Rigidbody가 없습니다.", this);

        if (_Animator == null)
            _Animator = GetComponentInChildren<Animator>();
    }

    // MountEntity에서 cfg.ApplyData(_Data)로 호출됨
    public void ApplyData(MountDataSO data)
    {
        _Data = data as HorseDataSO;
        if (_Data == null)
        {
            Debug.LogError($"[{name}] HorseController인데 HorseDataSO가 아닙니다.", this);
            return;
        }

        // 파라미터 해시 캐싱 (Animator가 없어도 이동은 동작)
        if (_Animator != null)
        {
            _H_Speed = Animator.StringToHash(_Data.AnimParamSpeed);
            _H_Turn  = Animator.StringToHash(_Data.AnimParamTurn);
            _H_Brake = Animator.StringToHash(_Data.AnimParamBrake);
        }
    }

    // MountEntity가 드라이버 입력을 여기로 넣어줌
    public void SetInput(in MountInput input)
    {
        _Input = input;
    }

    public void FixedTick()
    {
        if (_Data == null) return;
        if (_RB == null) return;

        // 전/후진 목표 속도 계산
        float maxFwd = _Data.MaxSpeed / 3.6f;
        float maxRev = _Data.MaxReverseSpeed / 3.6f;

        float targetSpeed;
        if (_Input.Throttle >= 0f)
            targetSpeed = _Input.Throttle * maxFwd;
        else
            targetSpeed = _Input.Throttle * maxRev; // throttle이 음수라 targetSpeed도 음수

        // 현재 forward 성분 속도
        Vector3 v = _RB.linearVelocity;
        float curForwardSpeed = Vector3.Dot(v, transform.forward);

        // 가속/감속
        float accel = (_Input.Throttle >= 0f) ? _Data.Accel : _Data.ReverseAccel;

        // 브레이크면 목표 0으로 빠르게 감속
        if (_Input.Brake)
        {
            targetSpeed = 0f;
            accel = _Data.BrakeDecel;
        }

        float newForwardSpeed = Mathf.MoveTowards(
            curForwardSpeed,
            targetSpeed,
            accel * Time.fixedDeltaTime
        );

        // 속도 적용
        //   - 좌우 미끄럼/횡속 제거: 말이 옆으로 미끄는 느낌 방지
        Vector3 vertical = Vector3.up * v.y;
        Vector3 newVel = transform.forward * newForwardSpeed + vertical;
        _RB.linearVelocity = newVel;

        // 회전
        float yaw = _Input.Steer * _Data.TurnDegPerSec * Time.fixedDeltaTime;
        if (Mathf.Abs(yaw) > 0.0001f)
        {
            Quaternion delta = Quaternion.Euler(0f, yaw, 0f);
            _RB.MoveRotation(_RB.rotation * delta);
        }

        // 애니메이션 파라미터 업데이트
        UpdateAnimator(newForwardSpeed, maxFwd);
    }

    void UpdateAnimator(float forwardSpeed, float maxForwardSpeed)
    {
        if (_Animator == null) return;
        if (_Data == null) return;

        // -1~1 정규화 (후진 음수)
        float speedNorm = 0f;
        if (maxForwardSpeed > 0.01f)
            speedNorm = Mathf.Clamp(forwardSpeed / maxForwardSpeed, -1f, 1f);

        float turnNorm = Mathf.Clamp(_Input.Steer, -1f, 1f);
        float damp = _Data.AnimDampTime;

        _Animator.SetFloat(_H_Speed, speedNorm, damp, Time.fixedDeltaTime);
        _Animator.SetFloat(_H_Turn,  turnNorm,  damp, Time.fixedDeltaTime);
        _Animator.SetBool(_H_Brake, _Input.Brake);
    }
}
