using UnityEngine;

[RequireComponent(typeof(PlayerTracker))]
public class LookAtTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] float _TargetHeightOffset = 1.5f; // 타겟 머리 높이 보정
    PlayerTracker _Tracker;

    [Header("Weights")]
    [Range(0, 1)] public float _HeadWeight = 1f;
    [Range(0, 1)] public float _EyesWeight = 0.7f;
    [Range(0, 1)] public float _ClampWeight = 0.6f;
    public float _BlendInSpeed = 6f;
    public float _BlendOutSpeed = 4f;

    [Header("Angular Limits (deg/sec)")]
    [SerializeField] float _MaxYawDegPerSec   = 240f;   // 좌우
    [SerializeField] float _MaxPitchDegPerSec = 180f;   // 상하

    [Header("Neutral Look")]
    [SerializeField] float _NeutralDistance = 10f;      // 타겟 없을 때 정면 응시 거리
    [SerializeField] float _NeutralHeight   = 1.6f;     // 타겟 없을 때 응시 높이

    Animator _Anim;
    Transform _Head;           // 머리 본(없으면 null)
    float _W;                  // 전체 IK 블렌드
    Vector3 _AimPos;           // 현재 실제 응시점(각속도 제한 적용 후 유지)

    void Awake()
    {
        _Anim = GetComponent<Animator>();
        _Tracker = GetComponent<PlayerTracker>();
        _Head = _Anim.GetBoneTransform(HumanBodyBones.Head);

        // 초기 응시점: 정면
        _AimPos = transform.position + transform.forward * _NeutralDistance + Vector3.up * _NeutralHeight;
    }

    void Update()
    {
        bool hasTarget = _Tracker && _Tracker.CurrentTarget;
        float target = hasTarget ? 1f : 0f;
        float speed = hasTarget ? _BlendInSpeed : _BlendOutSpeed;
        _W = Mathf.MoveTowards(_W, target, speed * Time.deltaTime);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!_Anim) return;

        if (_W <= 0.001f) { _Anim.SetLookAtWeight(0f); return; }

        // 원하는 목표점(Desired) 계산
        Vector3 desiredPos;
        if (_Tracker && _Tracker.CurrentTarget)
        {
            var t = _Tracker.CurrentTarget;
            desiredPos = t.position + Vector3.up * _TargetHeightOffset;
        }
        else
        {
            desiredPos = transform.position + transform.forward * _NeutralDistance + Vector3.up * _NeutralHeight;
        }

        // 각속도 제한으로 _AimPos 갱신
        Vector3 headPos = _Head ? _Head.position : transform.position + Vector3.up * _NeutralHeight;

        Vector3 currDir = (_AimPos - headPos).sqrMagnitude > 1e-6f
            ? (_AimPos - headPos).normalized
            : transform.forward;

        Vector3 destDir = (desiredPos - headPos).sqrMagnitude > 1e-6f
            ? (desiredPos - headPos).normalized
            : transform.forward;

        // 로컬 공간에서 요/피치로 변환
        Quaternion worldToLocal = Quaternion.Inverse(transform.rotation);
        Vector3 currLocal = (worldToLocal * currDir).normalized;
        Vector3 destLocal = (worldToLocal * destDir).normalized;

        float currYaw = Mathf.Atan2(currLocal.x, currLocal.z) * Mathf.Rad2Deg;
        float destYaw = Mathf.Atan2(destLocal.x, destLocal.z) * Mathf.Rad2Deg;
        float currPitch = Mathf.Asin(Mathf.Clamp(currLocal.y, -1f, 1f)) * Mathf.Rad2Deg;
        float destPitch = Mathf.Asin(Mathf.Clamp(destLocal.y, -1f, 1f)) * Mathf.Rad2Deg;

        float yawStep = _MaxYawDegPerSec   * Time.deltaTime;
        float pitchStep = _MaxPitchDegPerSec * Time.deltaTime;

        float newYaw = (_MaxYawDegPerSec   > 0f) ? Mathf.MoveTowardsAngle(currYaw,   destYaw,   yawStep)   : destYaw;
        float newPitch = (_MaxPitchDegPerSec > 0f) ? Mathf.MoveTowardsAngle(currPitch, destPitch, pitchStep) : destPitch;

        Vector3 newLocalDir = (Quaternion.Euler(newPitch, newYaw, 0f) * Vector3.forward).normalized;
        Vector3 newWorldDir = (transform.rotation * newLocalDir).normalized;

        // 목표까지의 현재 거리 유지(너무 가까우면 최소 1m)
        float keepDist = Mathf.Max(1f, Vector3.Distance(headPos, desiredPos));
        _AimPos = headPos + newWorldDir * keepDist;

        // IK 적용
        _Anim.SetLookAtPosition(_AimPos);
        _Anim.SetLookAtWeight(_W, bodyWeight: 0f, headWeight: _HeadWeight, eyesWeight: _EyesWeight, clampWeight: _ClampWeight);
    }
}
