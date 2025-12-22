using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MountEntity : MonoBehaviourPun
{
    [System.Serializable]
    public class SeatSlot
    {
        public Transform Anchor;          // 탑승자 붙을 위치
        public Transform DismountPoint;   // (선택) 하차 위치
        [HideInInspector] public int RiderViewId = -1;
    }

    [Header("Seats (0번이 운전석)")]
    [SerializeField] private SeatSlot[] _Seats;

    private IMountMovement _Movement;
    private MountInput _DriverInput;

    void Awake()
    {
        _Movement = GetComponent<IMountMovement>();
        if (_Movement == null)
            Debug.LogError($"[{name}] IMountMovement 구현체가 없습니다. (예: CarMovement)", this);

        if (_Seats == null || _Seats.Length == 0 || _Seats[0].Anchor == null)
            Debug.LogError($"[{name}] Seats[0] (운전석) Anchor가 필요합니다.", this);
    }

    public bool HasDriver => _Seats != null && _Seats.Length > 0 && _Seats[0].RiderViewId != -1;

    // 운전자 로컬에서만 호출되게 구성
    public void SetDriverInput(in MountInput input)
    {
        _DriverInput = input;
    }

    void FixedUpdate()
    {
        // 오직 소유자(운전자 클라)만 실제 물리 이동 적용
        if (!photonView.IsMine) return;
        if (!HasDriver) return;
        if (_Movement == null) return;

        _Movement.SetInput(_DriverInput);
        _Movement.FixedTick();
    }

    // ===== 탑승 =====

    /// <summary>
    /// 차량에 탑승 시도: 빈 좌석 자동 배치 (0번이 운전석)
    /// - 운전석이 비어있으면 0번
    /// - 아니면 1번~ 순서대로 빈 좌석
    /// </summary>
    public bool TryMount(GameObject riderGo)
    {
        if (_Seats == null || _Seats.Length == 0) return false;

        PhotonView riderPv = riderGo.GetComponent<PhotonView>();
        if (riderPv == null) return false;

        int seatIndex = FindFirstFreeSeatIndex();
        if (seatIndex < 0) return false;

        // 운전석을 타는 경우엔 소유권 필요(물리 적용이 owner에서만 되니까)
        if (seatIndex == 0 && !photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        photonView.RPC(nameof(RPC_EnterSeat), RpcTarget.All, seatIndex, riderPv.ViewID);
        return true;
    }

    int FindFirstFreeSeatIndex()
    {
        // 0번(운전석) 우선
        if (_Seats[0].RiderViewId == -1) return 0;

        // 나머지 좌석
        for (int i = 1; i < _Seats.Length; i++)
        {
            if (_Seats[i].RiderViewId == -1) return i;
        }
        return -1;
    }

    // ===== 하차 =====

    public bool TryDismount(GameObject riderGo)
    {
        PhotonView riderPv = riderGo.GetComponent<PhotonView>();
        if (riderPv == null) return false;

        int seatIndex = FindSeatIndexByRider(riderPv.ViewID);
        if (seatIndex < 0) return false;

        Vector3 dismountPos = GetDismountPos(seatIndex, riderGo.transform.position);
        photonView.RPC(nameof(RPC_ExitSeat), RpcTarget.All, seatIndex, riderPv.ViewID, dismountPos);
        return true;
    }

    int FindSeatIndexByRider(int riderViewId)
    {
        if (_Seats == null) return -1;
        for (int i = 0; i < _Seats.Length; i++)
        {
            if (_Seats[i].RiderViewId == riderViewId) return i;
        }
        return -1;
    }

    Vector3 GetDismountPos(int seatIndex, Vector3 fallbackPos)
    {
        if (_Seats == null || seatIndex < 0 || seatIndex >= _Seats.Length) return fallbackPos;
        Transform p = _Seats[seatIndex].DismountPoint;
        return p ? p.position : fallbackPos;
    }

    // ===== RPC =====

    [PunRPC]
    void RPC_EnterSeat(int seatIndex, int riderViewId)
    {
        if (_Seats == null || seatIndex < 0 || seatIndex >= _Seats.Length) return;

        PhotonView riderPv = PhotonView.Find(riderViewId);
        if (riderPv == null) return;

        SeatSlot seat = _Seats[seatIndex];
        if (seat.Anchor == null) return;

        // 이미 누가 타있으면 무시(동시 요청 방어)
        if (seat.RiderViewId != -1) return;

        GameObject riderGo = riderPv.gameObject;
        seat.RiderViewId = riderViewId;

        // 플레이어를 좌석에 부착
        riderGo.transform.SetParent(seat.Anchor, worldPositionStays: false);
        riderGo.transform.localPosition = Vector3.zero;
        riderGo.transform.localRotation = Quaternion.identity;

        // 플레이어 이동/동기화 끄기
        SetPlayerMountedState(riderGo, mounted: true);

        // 운전자만 "운전" 상태로 지정
        PlayerMountController rider = riderGo.GetComponent<PlayerMountController>();
        if (rider != null) rider.SetMount(this, isDriver: seatIndex == 0);
    }

    [PunRPC]
    void RPC_ExitSeat(int seatIndex, int riderViewId, Vector3 dismountWorldPos)
    {
        if (_Seats == null || seatIndex < 0 || seatIndex >= _Seats.Length) return;

        PhotonView riderPv = PhotonView.Find(riderViewId);
        if (riderPv == null) return;

        SeatSlot seat = _Seats[seatIndex];
        if (seat.RiderViewId != riderViewId) return;

        GameObject riderGo = riderPv.gameObject;

        // 부착 해제 + 위치 이동
        riderGo.transform.SetParent(null, worldPositionStays: true);
        riderGo.transform.position = dismountWorldPos;

        // 플레이어 이동/동기화 복구
        SetPlayerMountedState(riderGo, mounted: false);

        // 마운트/운전자 상태 해제
        PlayerMountController rider = riderGo.GetComponent<PlayerMountController>();
        if (rider != null) rider.SetMount(null, isDriver: false);

        seat.RiderViewId = -1;

        // 운전석이 비워졌으면 입력 초기화
        if (seatIndex == 0)
            _DriverInput = default;
    }

    private static void SetPlayerMountedState(GameObject riderGo, bool mounted)
    {
        CharacterController cc = riderGo.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !mounted;

        Rigidbody rb = riderGo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = mounted;
        }

        MonoBehaviour ptv = riderGo.GetComponent("PhotonTransformView") as MonoBehaviour;
        if (ptv != null) ptv.enabled = !mounted;
    }
}
