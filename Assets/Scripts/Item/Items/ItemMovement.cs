using Photon.Pun;
using UnityEngine;

/// <summary>
/// Rigidbody 없이:
/// - 드랍 시 포물선
/// - 벽/가파른 면은 반사(튕김)
/// - 지면에 닿으면 호버 전환(월드 +Y로 일정 높이 유지)
/// - 항상 회전, 호버 중 바운싱
/// </summary>
public class ItemMovement : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Drop Motion")]
    [SerializeField] private float _MinHorizSpeed = 1.0f;
    [SerializeField] private float _MaxHorizSpeed = 2.5f;
    [SerializeField] private float _UpSpeed = 5.0f;
    [SerializeField] private Vector3 _Gravity = new(0f, -9.8f, 0f);

    [Header("Collision / Hover")]
    [SerializeField] private LayerMask _IgnoreLayer;
    [SerializeField] private float _Radius = 0.15f;  // 아이템 반경(충돌 여유)
    [SerializeField] private float _HoverHeight = 0.5f;  // 지면에서 띄울 높이(+Y)
    [SerializeField] private float _Bounciness = 0.5f;   // 반사 감쇠(0~1)
    [SerializeField] private float _SleepSpeed = 0.15f;  // 너무 느리면 멈춤 보정
    [SerializeField] private float _GroundMinNormalY  = 0.6f;   // 지면 판정 임계치(≈ 53° 이하)

    [Header("Visual")]
    [SerializeField] private float _RotateSpeed = 30f;   // deg/sec
    [SerializeField] private float _BobAmp = 0.05f; // 호버 바운싱 크기
    [SerializeField] private float _BobSpeed = 2.0f;  // 호버 바운싱 속도


    private enum SimState { Idle, Dropping, Hover }
    private SimState _State = SimState.Idle;

    private Vector3 _Vel; // 드랍 중 속도
    private Vector3 _HoverAnchor; // 지면 히트 지점
    private float _BobSeed; // 바운싱 위상 오프셋
    private int _CastMask;

    void Awake()
    {
        _BobSeed = Random.value * 10f;

        int self = 1 << gameObject.layer;
        _CastMask = ~_IgnoreLayer.value & ~self;
    }

    void Update()
    {
        // 회전: 모든 클라에서 동일 수행(시각 연출)
        transform.Rotate(Vector3.up, _RotateSpeed * Time.deltaTime, Space.World);

        // 이동 시뮬은 Owner만
        if (!photonView.IsMine) return;

        if (_State == SimState.Dropping)
            SimulateDrop(Time.deltaTime);
        else if (_State == SimState.Hover)
            MaintainHover(); // 필요 시 이동 플랫폼 추적
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            LaunchRandom(); // 자동 드랍 시작
            
            Debug.Log("[ItemMovement] Launched in OnPhotonInstantiate");
        }
            
    }

    /// <summary>랜덤 초기속도로 드랍 시작</summary>
    public void LaunchRandom()
    {
        Vector2 d = Random.insideUnitCircle;
        if (d.sqrMagnitude < 1e-4f) d = Vector2.right;
        d.Normalize();

        Vector3 horiz = new(d.x, 0f, d.y);
        float spd = Random.Range(_MinHorizSpeed, _MaxHorizSpeed);
        Vector3 v0 = horiz * spd + Vector3.up * _UpSpeed;

        LaunchWithVelocity(v0);
    }

    /// <summary>지정한 초기속도로 드랍 시작</summary>
    public void LaunchWithVelocity(Vector3 initialVelocity)
    {
        _Vel   = initialVelocity;
        _State = SimState.Dropping;
    }

    private void SimulateDrop(float dt)
    {
        Vector3 pos  = transform.position;
        Vector3 next = pos + _Vel * dt + 0.5f * _Gravity * dt * dt;

        Vector3 delta = next - pos;
        float   dist  = delta.magnitude;

        if (dist > 1e-5f &&
            Physics.SphereCast(
                pos,
                _Radius,
                delta.normalized,
                out RaycastHit hit,
                dist,
                _CastMask,
                QueryTriggerInteraction.Ignore)) // 모든 레이어, 트리거 무시
        {
            // 충돌면 바로 밖으로 밀기(겹침 방지)
            transform.position = hit.point + hit.normal * _Radius;

            // 중력 적용 후의 속도를 기준으로 처리
            Vector3 vAfter = _Vel + _Gravity * dt;

            // 지면 판정: 완만한 면(법선 Y가 임계 이상) → 호버 전환
            if (hit.normal.y >= _GroundMinNormalY)
            {
                _HoverAnchor = hit.point;
                _Vel = Vector3.zero;
                _State = SimState.Hover;
                return;
            }

            // 벽/가파른 면: 반사(튕김) + 감쇠
            _Vel = Vector3.Reflect(vAfter, hit.normal) * _Bounciness;

            // 너무 느리면 그대로 떨어지게 보정(정체 방지)
            if (_Vel.magnitude < _SleepSpeed)
                _Vel += hit.normal * 0.01f; // 약간 밀어내어 다음 프레임 진행

            return;
        }

        // 충돌 없음 → 통상 적분
        transform.position = next;
        _Vel += _Gravity * dt;
    }

    private void MaintainHover()
    {
        // 지면 앵커 재확인(이동 플랫폼 대응; 정적 지형이면 주석 처리 가능)
        if (Physics.Raycast(
            _HoverAnchor + Vector3.up,
            Vector3.down,
            out RaycastHit hit,
            2f,
            _CastMask,
            QueryTriggerInteraction.Ignore))
        {
            _HoverAnchor = hit.point;
        }

        float bob = Mathf.Sin((Time.time + _BobSeed) * _BobSpeed) * _BobAmp;
        transform.position = _HoverAnchor + Vector3.up * (_HoverHeight + bob);
    }
}
