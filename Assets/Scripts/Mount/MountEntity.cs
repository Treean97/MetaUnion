using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MountEntity : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    [System.Serializable]
    public class SeatSlot
    {
        public Transform Anchor;          // 탑승자 붙을 위치
        public Transform DismountPoint;   // 하차 위치
        [HideInInspector] public int RiderViewId = -1;
    }

    [Header("탑승물 정보")]
    [SerializeField] private MountDataSO _Data;
    public MountDataSO Data => _Data;

    // 데이터 캐싱
    float _NoDriverDecel, _NoDriverStopSpeed;

    [Header("Seats (0번이 운전석)")]
    [SerializeField] private SeatSlot[] _Seats;

    private VehicleSound _VehicleSound;
    
    private Rigidbody _RB;
    private IMountMovement _Movement;
    private MountInput _DriverInput;

    // 운전석 탑승 pending
    private bool _HasPendingDriverEnter;
    private int _PendingDriverRiderViewId;

    void Awake()
    {
        _Movement = GetComponent<IMountMovement>();
        if (_Movement == null)
            Debug.LogError($"[{name}] IMountMovement 구현체가 없습니다.", this);

        if (_Movement is IMountMovementConfigurable cfg)
            cfg.ApplyData(_Data);

        if (_Seats == null || _Seats.Length == 0 || _Seats[0].Anchor == null)
            Debug.LogError($"[{name}] Seats[0] (운전석) Anchor가 필요합니다.", this);
        
        _RB = GetComponent<Rigidbody>();
        if(_RB == null)
            Debug.LogError("Rigidbody가 없습니다.", this);

        if (_Data != null)
        {
            _NoDriverDecel = _Data.NoDriverDecel;
            _NoDriverStopSpeed = _Data.NoDriverStopSpeed;
        }    

        _VehicleSound = GetComponent<VehicleSound>();

        ResetSeat();
    }

    public bool HasDriver => _Seats != null && _Seats.Length > 0 && _Seats[0].RiderViewId != -1;

    // 운전자 로컬에서만 호출되게 구성
    public void SetDriverInput(in MountInput input)
    {
        _DriverInput = input;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (!HasDriver)
        {
            if (_RB == null) return;

            float decelDelta = _NoDriverDecel * Time.fixedDeltaTime;

            // 선형 속도
            Vector3 v = _RB.linearVelocity;
            v = Vector3.MoveTowards(v, Vector3.zero, decelDelta);
            if (v.sqrMagnitude < _NoDriverStopSpeed * _NoDriverStopSpeed) v = Vector3.zero;
            _RB.linearVelocity = v;


            if (_RB.linearVelocity == Vector3.zero)
            _RB.Sleep();

            return;
        }

        if (_Movement == null) return;
        _Movement.SetInput(_DriverInput);
        _Movement.FixedTick();
    }


    void ResetSeat()
    {
        if (_Seats == null) return;
        for (int i = 0; i < _Seats.Length; i++)
            _Seats[i].RiderViewId = -1;
    }

    // 탑승
    public bool TryMount(GameObject riderGo)
    {
        if (_Seats == null || _Seats.Length == 0)
            return false;

        PhotonView riderPv = riderGo.GetComponent<PhotonView>();
        if (riderPv == null)
            return false;

        int seatIndex = FindFirstFreeSeatIndex();
        if (seatIndex < 0)
            return false;

        // 운전석이면 소유권 확보 후 RPC 동기화
        if (seatIndex == 0 && !photonView.IsMine)
        {
            // 이미 다른 pending이 있으면 중복 방지
            if (_HasPendingDriverEnter)
                return false;

            _HasPendingDriverEnter = true;
            _PendingDriverRiderViewId = riderPv.ViewID;

            photonView.RequestOwnership();
            return true;
        }

        // 조수석/뒷좌석 또는 이미 소유권이 내 것인 운전석이면 즉시 탑승 처리
        photonView.RPC(nameof(RPC_EnterSeat), RpcTarget.All, seatIndex, riderPv.ViewID);
        return true;
    }

    int FindFirstFreeSeatIndex()
    {
        if (_Seats[0].RiderViewId == -1) return 0;

        for (int i = 1; i < _Seats.Length; i++)
        {
            if (_Seats[i].RiderViewId == -1) return i;
        }
        return -1;
    }

    // 하차
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

    // 소유권 콜백
    // 현재 소유자가 "요청받았을 때" 넘겨줄지 결정
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (targetView != photonView) return;

        // 운전석이 비어있을 때만 넘겨줌
        if (_Seats != null && _Seats.Length > 0 && _Seats[0].RiderViewId == -1)
        {
            targetView.TransferOwnership(requestingPlayer);
        }
    }

    // 소유권 이전 완료되면 pending 운전석 탑승을 처리
    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (targetView != photonView) return;

        if (!_HasPendingDriverEnter)
            return;

        if (!photonView.IsMine)
            return;

        // 소유권 확보, 운전석 탑승 RPC
        photonView.RPC(nameof(RPC_EnterSeat), RpcTarget.All, 0, _PendingDriverRiderViewId);

        _HasPendingDriverEnter = false;
        _PendingDriverRiderViewId = -1;
    }

    // RPC
    [PunRPC]
    void RPC_EnterSeat(int seatIndex, int riderViewId)
    {
        if (_Seats == null || seatIndex < 0 || seatIndex >= _Seats.Length)
            return;

        PhotonView riderPv = PhotonView.Find(riderViewId);
        if (riderPv == null)
            return;

        SeatSlot seat = _Seats[seatIndex];
        if (seat.Anchor == null)
            return;

        if (seat.RiderViewId != -1)
            return;

        if (seatIndex == 0)
        {
            _VehicleSound?.SetRunning(true);
        }

        GameObject riderGo = riderPv.gameObject;
        seat.RiderViewId = riderViewId;

        riderGo.transform.SetParent(seat.Anchor, worldPositionStays: false);
        riderGo.transform.localPosition = Vector3.zero;
        riderGo.transform.localRotation = Quaternion.identity;

        SetPlayerMountedState(riderGo, mounted: true);

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

        riderGo.transform.SetParent(null, worldPositionStays: true);
        riderGo.transform.position = dismountWorldPos;

        SetPlayerMountedState(riderGo, mounted: false);

        PlayerMountController rider = riderGo.GetComponent<PlayerMountController>();
        if (rider != null) rider.SetMount(null, isDriver: false);

        seat.RiderViewId = -1;

        if (seatIndex == 0)
        {
            _DriverInput = default;

            // 운전자가 내리면 pending도 초기화
            _HasPendingDriverEnter = false;
            _PendingDriverRiderViewId = -1;
            _VehicleSound?.SetRunning(false);

            // 하차 시 소유권을 마스터 클라이언트로 반환
            if (photonView.IsMine && PhotonNetwork.IsMasterClient == false)
            {
                photonView.TransferOwnership(PhotonNetwork.MasterClient);
            }
        }
    }

    private void SetPlayerMountedState(GameObject riderGo, bool mounted)
    {
        var applier = riderGo.GetComponent<IMountStateApplier>();
        applier?.ApplyMounted(mounted);
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        if (targetView != photonView) return;

        _HasPendingDriverEnter = false;
        _PendingDriverRiderViewId = -1;
    }
}
