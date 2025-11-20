using System;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
    [Header("Box Detect (local +Z forward)")]
    [SerializeField] float _Near   = 0.0f;   // 시작 여유 거리(발 앞)
    [SerializeField] float _Depth  = 8.0f;   // 전방 길이
    [SerializeField] float _Width  = 6.0f;   // 좌우 폭
    [SerializeField] float _Height = 3.0f;   // 상하 높이
    [SerializeField] float _HeightOffset = 1.5f; // 박스 중심 높이(캐릭터 키에 맞춤)

    [Header("Horizontal FOV")]
    [SerializeField, Range(0f, 180f)] float _FovDeg = 160f; // 0이면 각도 체크 끔(박스만 사용)
    float _CosHalfYaw; // 내부 프리컴퓨트

    [Header("Filter")]
    [SerializeField] LayerMask _TargetMask = ~0; // 레이어만 사용
    [SerializeField] bool _ExcludeSelf = true;   // 자기 자신/같은 루트 제외
    [SerializeField] Transform _Root;            // 미지정 시 자기 자신

    readonly Collider[] _Hits = new Collider[64];

    Transform _Self;
    Transform _CurrentTarget;
    public Transform CurrentTarget => _CurrentTarget;

    public event Action<Transform> OnTargetChanged;

    void Awake()
    {
        _Self = transform;
        if (!_Root) _Root = _Self;

        _CosHalfYaw = (_FovDeg <= 0f) ? -1f : Mathf.Cos((_FovDeg * 0.5f) * Mathf.Deg2Rad);
    }

    void OnValidate()
    {
        // 에디터에서 값 바꿀 때도 코사인 갱신
        _CosHalfYaw = (_FovDeg <= 0f) ? -1f : Mathf.Cos((_FovDeg * 0.5f) * Mathf.Deg2Rad);
    }

    void Update()
    {
        // 전방 박스 볼륨으로 후보 수집
        Vector3 center =
            _Self.position
            + Vector3.up * _HeightOffset
            + _Self.forward * (_Near + _Depth * 0.5f);

        Vector3 halfExtents = new Vector3(_Width * 0.5f, _Height * 0.5f, _Depth * 0.5f);
        Quaternion orientation = _Self.rotation;

        int count = Physics.OverlapBoxNonAlloc(
            center, halfExtents, _Hits, orientation, _TargetMask, QueryTriggerInteraction.Ignore);

        // 가장 가까운 대상 선택
        Transform picked = PickNearestInBox(_Hits, count);

        if (picked != _CurrentTarget)
        {
            _CurrentTarget = picked;
            OnTargetChanged?.Invoke(_CurrentTarget);
        }
    }

    Transform PickNearestInBox(Collider[] hits, int count)
    {
        Transform best = null;

        // 전방 가까움(로컬 z) 우선, 동률이면 전체 거리로 보조 비교
        float bestForwardZ = float.PositiveInfinity;
        float bestDist = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            var col = hits[i];
            if (!col) continue;

            Transform t = col.transform;

            // 자기 자신/같은 루트 제외
            if (_ExcludeSelf && (t == _Self || t.IsChildOf(_Root))) continue;

            // 콜라이더 중심
            Vector3 p = col.bounds.center;

            // 수평 FOV 필터: 로컬 XZ 평면에서 전방(+Z) 기준
            if (!PassYawFov(p)) continue;

            // 전방 거리
            Vector3 local = _Self.InverseTransformPoint(p);
            float forwardZ = Mathf.Max(0f, local.z - _Near);

            // 전체 거리
            float dist = Vector3.Distance(_Self.position, p);

            if (forwardZ < bestForwardZ || (Mathf.Approximately(forwardZ, bestForwardZ) && dist < bestDist))
            {
                bestForwardZ = forwardZ;
                bestDist = dist;
                best = t;
            }
        }

        return best;
    }

    bool PassYawFov(Vector3 worldPoint)
    {
        if (_FovDeg <= 0f) return true; // 각도 체크 끔(박스만)

        Vector3 local = _Self.InverseTransformPoint(worldPoint);
        Vector3 dirXZ = new Vector3(local.x, 0f, local.z);
        float sq = dirXZ.sqrMagnitude;
        if (sq < 1e-6f) return false;

        dirXZ /= Mathf.Sqrt(sq);
        float dot = Vector3.Dot(Vector3.forward, dirXZ); // 로컬 전방과의 코사인
        return dot >= _CosHalfYaw;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // 빨간색 기즈모로 박스 시각화
        var tr = Application.isPlaying ? _Self : transform;
        if (!tr) tr = transform;

        Vector3 center =
            tr.position
            + Vector3.up * _HeightOffset
            + tr.forward * (_Near + _Depth * 0.5f);

        Vector3 halfExtents = new Vector3(_Width * 0.5f, _Height * 0.5f, _Depth * 0.5f);

        Matrix4x4 m = Matrix4x4.TRS(center, tr.rotation, Vector3.one);
        Gizmos.matrix = m;
        Gizmos.color = Color.red; // 요청: 빨간색
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);

        // Near 평면 보조선(빨간색)
        Vector3 nearCenter = Vector3.back * (_Depth * 0.5f);
        Vector3 nearSize = new Vector3(_Width, _Height, 0f);
        Gizmos.DrawWireCube(nearCenter, nearSize);

        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
