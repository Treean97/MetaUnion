using Photon.Pun;
using UnityEngine;

/// <summary>
/// RPC 없이 로컬 동작만 수행.
/// - 참여/퇴장 요청은 EmoteManager의 룸 프로퍼티 CAS로 처리
/// - 상태 반영은 EmoteManager.Reconcile에서 이 스크립트의 ApplyEmoteLocal/DoLeaveAndReturn 호출
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerEmote : MonoBehaviourPun
{
    [Header("Blend")]
    [SerializeField] private float _CrossFade = 0.1f;
    [SerializeField] private string _IdleState = "Idle";

    private Animator _Anim;
    private EmoteAnchor _Anchor;
    private int _SlotIndex = -1;
    private bool _IsInEmote;

    private Vector3 _ReturnPos;
    private Quaternion _ReturnRot;

    public bool InEmote => _IsInEmote;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;

    void Awake() => _Anim = GetComponentInChildren<Animator>();

    // ==== 참여 요청(룸 프로퍼티) ====
    public void RequestJoinViaRoomProp(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.EmoteSO) return;
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;
        int view  = photonView ? photonView.ViewID : -1;
        if (view <= 0) return;

        // 성공 시 곧바로 재생하지 않는다. Reconcile이 호출되어 로컬 적용된다.
        if (!EmoteManager._Inst.TryJoinSlot(anchor, actor, view, out _))
        {
            // 경합 실패(만석 등) → 필요 시 UI 토스트
            Debug.Log("[Emote] 참여 실패(만석/경합)");
        }
    }

    // ==== 퇴장 요청(룸 프로퍼티) ====
    public void RequestLeaveViaRoomProp()
    {
        if (!_Anchor) return;
        int actor = PhotonNetwork.LocalPlayer.ActorNumber;
        int view  = photonView ? photonView.ViewID : -1;
        if (view <= 0) return;

        EmoteManager._Inst.LeaveSlot(_Anchor, actor, view);
        // 성공 시 Reconcile이 호출되어 로컬 종료된다. (즉시 꺼도 무방)
        // DoLeaveAndReturn();
    }

    // ==== Reconcile에서 호출: 로컬 재생 ====
    public void ApplyEmoteLocal(EmoteAnchor anchor, int slotIndex, float normalizedTime)
    {
        if (!anchor || !anchor.EmoteSO) return;

        _Anchor    = anchor;
        _SlotIndex = slotIndex;
        _IsInEmote = true;

        if (photonView.IsMine)
        {
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;

            // 슬롯 스냅(안전)
            if (_SlotIndex >= 0 && _SlotIndex < anchor.SlotCount)
                transform.SetPositionAndRotation(anchor.GetSlotWorldPos(_SlotIndex), anchor.GetSlotWorldRot(_SlotIndex));
        }

        var so = anchor.EmoteSO;
        if (_Anim)
        {
            int hash = Animator.StringToHash(so.StateName);
            if (_Anim.HasState(so.Layer, hash))
                _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime)); // 진행 중 지점부터
            else
                Debug.LogError($"[Emote] 상태 없음: Layer={so.Layer}, State='{so.StateName}'");
        }
    }

    // ==== Reconcile에서 호출: 로컬 종료 ====
    public void DoLeaveAndReturn()
    {
        if (_IsInEmote && _Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        if (photonView.IsMine)
            transform.SetPositionAndRotation(_ReturnPos, _ReturnRot);

        _IsInEmote = false;
        _Anchor    = null;
        _SlotIndex = -1;
    }
}
