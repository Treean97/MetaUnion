using Photon.Pun;
using UnityEngine;

/// <summary>
/// UI/상호작용으로 이미 진행 중인 이모트에 참여/나가기.
/// - 참여 시: EmoteManager.JoinEmote로 슬롯 예약 → RPC로 전 클라 재생
/// - 나가기: EmoteManager.LeaveEmote로 해제 → 복귀
/// - 위치/각도 스냅은 '소유자만', 나머지는 Photon Transform View가 보간
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerEmote : MonoBehaviourPun
{
    [Header("Blend")]
    [SerializeField] private float _CrossFade = 0.1f;
    [SerializeField] private string _IdleState = "Movement";

    private Animator _Anim;
    private EmoteAnchor _Anchor;
    private int _SlotIndex = -1;
    private bool _IsInEmote;

    // 복귀
    private Vector3 _ReturnPos;
    private Quaternion _ReturnRot;

    public bool InEmote => _IsInEmote;
    public int CurrentSlotIndex => _SlotIndex;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;

    void Awake()
    {
        // 자식에 Animator가 있는 구조까지 커버
        _Anim = GetComponentInChildren<Animator>();
    }

    // ===== 외부(UI/상호작용) 진입 포인트 =====
    public void RequestJoinSequential(EmoteAnchor anchor)
    {
        if (!anchor) return;

        if (!EmoteManager._Inst.JoinEmote(anchor, this, out var slot))
        {
            OnJoinRejected_Full();
            return;
        }

        // 성공 시 BeginJoin 호출
        BeginJoin(anchor, slot);
    }

    public void RequestLeave()
    {
        if (!_Anchor) return;
        EmoteManager._Inst.LeaveEmote(_Anchor, this);
        DoLeaveAndReturn();
    }

    // ===== 내부 로직 =====
    public void BeginJoin(EmoteAnchor anchor, int slotIndex)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        // 소유자만 복귀점 기록 + 슬롯 스냅
        if (photonView.IsMine)
        {
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;

            // Use Local을 쓴다면 여기서 로컬 변환으로 맞춰도 됨(팀 규칙에 맞춰 통일)
            transform.SetPositionAndRotation(anchor.GetSlotWorldPos(slotIndex), anchor.GetSlotWorldRot(slotIndex));
        }

        float t = EmoteManager.GetNormalizedTime(anchor);
        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All, anchor.photonView.ViewID, slotIndex, t);
    }

    [PunRPC]
    public void RPC_PlayEmote(int anchorViewId, int slotIndex, float normalizedTime)
    {
        var pv = PhotonView.Find(anchorViewId);
        var anchor = pv ? pv.GetComponent<EmoteAnchor>() : null;
        if (!anchor || !anchor.EmoteSO) return;

        _Anchor = anchor;
        _SlotIndex = slotIndex;
        _IsInEmote = true;

        var so = anchor.EmoteSO;

        if (_Anim)
        {
            // 상태 존재 검증(오타/세팅 실수 방지)
            int hash = Animator.StringToHash(so.StateName);
            if (_Anim.HasState(so.Layer, hash))
                _Anim.CrossFadeInFixedTime(so.StateName, _CrossFade, so.Layer, Mathf.Clamp01(normalizedTime));
            else
                Debug.LogError($"[Emote] 상태 없음: Layer={so.Layer}, State='{so.StateName}'");
        }
    }

    public void DoLeaveAndReturn()
    {
        if (_IsInEmote && _Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        // 소유자만 복귀 텔레포트(원격은 PTV 보간)
        if (photonView.IsMine)
            transform.SetPositionAndRotation(_ReturnPos, _ReturnRot);

        _IsInEmote = false;
        _Anchor = null;
        _SlotIndex = -1;
    }

    [PunRPC]
    public void RPC_ForceLeaveAndReturn()
    {
        DoLeaveAndReturn();
    }

    public void OnJoinRejected_Full()
    {
        Debug.Log("[Emote] 참여 거절: 슬롯이 가득 찼습니다.");
        // TODO: UI 토스트 등
    }

    // 도우미: 내 PhotonViewID 얻기
    public int TryGetViewID()
    {
        var pv = GetComponent<PhotonView>();
        return pv ? pv.ViewID : -1;
    }

    void LateUpdate()
    {
        // 앵커가 이동하는 퍼포먼스라면, 소유자만 슬롯에 스냅 유지(원격은 PTV 보간)
        if (_IsInEmote && _Anchor && photonView.IsMine)
        {
            transform.SetPositionAndRotation(
                _Anchor.GetSlotWorldPos(_SlotIndex),
                _Anchor.GetSlotWorldRot(_SlotIndex)
            );
        }
    }
}
