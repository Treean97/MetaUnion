using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MountEntity : MonoBehaviourPun
{
    [Header("Seat")]
    [SerializeField] private Transform _DriverSeat;   // 운전석 위치(Transform)

    private IMountMovement _Movement;
    private MountInput _DriverInput;

    // 탑승자(운전자) PhotonViewId
    private int _DriverViewId = -1;

    void Awake()
    {
        _Movement = GetComponent<IMountMovement>();
        if (_Movement == null)
        {
            Debug.LogError($"[{name}] IMountMovement 구현체가 없습니다. (예: CarMovement)");
        }
    }

    public bool HasDriver => _DriverViewId != -1;

    // RiderController가 매 프레임 호출(운전자 로컬에서만 호출되게 구성)
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

    // ===== 탑승/하차 =====

    public bool TryMount(GameObject riderGo)
    {
        if (HasDriver) return false;

        PhotonView riderPv = riderGo.GetComponent<PhotonView>();
        if (riderPv == null) return false;

        // 운전자만 소유권을 가져가도록 요청
        if (!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

        int riderViewId = riderPv.ViewID;

        // 모든 클라에 탑승 브로드캐스트
        photonView.RPC(nameof(RPC_Mount), RpcTarget.All, riderViewId);
        return true;
    }

    public bool TryDismount(GameObject riderGo, Vector3 dismountWorldPos)
    {
        if (!HasDriver) return false;

        PhotonView riderPv = riderGo.GetComponent<PhotonView>();
        if (riderPv == null) return false;
        if (riderPv.ViewID != _DriverViewId) return false;

        photonView.RPC(nameof(RPC_Dismount), RpcTarget.All, _DriverViewId, dismountWorldPos);
        return true;
    }

    [PunRPC]
    void RPC_Mount(int riderViewId)
    {
        PhotonView riderPv = PhotonView.Find(riderViewId);
        if (riderPv == null) return;

        GameObject riderGo = riderPv.gameObject;

        _DriverViewId = riderViewId;

        // 플레이어를 좌석에 부착
        riderGo.transform.SetParent(_DriverSeat, worldPositionStays: false);
        riderGo.transform.localPosition = Vector3.zero;
        riderGo.transform.localRotation = Quaternion.identity;

        // 플레이어 이동/동기화 끄기
        SetPlayerMountedState(riderGo, mounted: true);

        // 운전자에게 현재 Mount 지정
        PlayerMountController rider = riderGo.GetComponent<PlayerMountController>();
        if (rider != null) rider.SetMount(this);
    }

    [PunRPC]
    void RPC_Dismount(int riderViewId, Vector3 dismountWorldPos)
    {
        PhotonView riderPv = PhotonView.Find(riderViewId);
        if (riderPv == null) return;

        GameObject riderGo = riderPv.gameObject;

        // 부착 해제 + 위치 이동
        riderGo.transform.SetParent(null, worldPositionStays: true);
        riderGo.transform.position = dismountWorldPos;

        // 플레이어 이동/동기화 복구
        SetPlayerMountedState(riderGo, mounted: false);

        // 운전자 Mount 해제
        PlayerMountController rider = riderGo.GetComponent<PlayerMountController>();
        if (rider != null) rider.SetMount(null);

        _DriverViewId = -1;
        _DriverInput = default;
    }

    private static void SetPlayerMountedState(GameObject riderGo, bool mounted)
    {
        // 프로젝트마다 컨트롤러가 다르니 "가장 흔한 것들"만 최소로 처리
        CharacterController cc = riderGo.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !mounted;

        Rigidbody rb = riderGo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = mounted; // 탑승 중엔 물리 끔(선호에 따라 조정)
        }

        // 플레이어가 PhotonTransformView로 자기 위치를 동기화하고 있으면
        // 탑승 중엔 꺼야 "부모 이동 + 네트워크 이동" 이중 적용이 덜 남.
        MonoBehaviour ptv = riderGo.GetComponent("PhotonTransformView") as MonoBehaviour;
        if (ptv != null) ptv.enabled = !mounted;
    }
}
