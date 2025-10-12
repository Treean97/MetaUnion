using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// UI/상호작용으로 이미 진행 중인 이모트에 참여/나가기.
/// - 참여 시: EmoteManager.JoinEmote로 슬롯 예약 → RPC로 전 클라 재생
/// - 나가기: EmoteManager.LeaveEmote로 해제 → 복귀
/// - 위치/각도 스냅은 '소유자만', 나머지는 Photon Transform View가 보간
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerEmote : MonoBehaviourPunCallbacks
{
    [Header("Blend")]
    [SerializeField] private float _CrossFade = 0.1f;
    [SerializeField] private string _IdleState = "Movement";

    private Animator _Anim;
    private EmoteAnchor _Anchor;
    private int _SlotIndex = -1;
    private bool _IsInEmote;

    // 복귀 지점(소유자만 사용)
    private Vector3 _ReturnPos;
    private Quaternion _ReturnRot;

    public bool InEmote => _IsInEmote;
    public int CurrentSlotIndex => _SlotIndex;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;

    void Awake()
    {
        _Anim = GetComponentInChildren<Animator>();
    }

    // ===== 외부(UI/상호작용) 진입 포인트 =====

    /// <summary>
    /// 참여 시도(슬롯 예약 → 진행률 계산 → 재생 RPC)
    /// </summary>
    public void RequestJoinSequential(EmoteAnchor anchor)
    {
        if (!anchor) return;

        if (!EmoteManager._Inst.JoinEmote(anchor, this, out var slot))
        {
            OnJoinRejected_Full();
            return;
        }

        // ── 룸 시작시각 & "실제 상태 길이" 기반 진행률 계산 ──
        double start = PhotonNetwork.Time;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;
        if (room != null && room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            start = (double)startObj;

        float effectiveLen = anchor.EmoteSO.Length; // 기본값: SO 길이
        if (TryResolveStateLength(_Anim, anchor.EmoteSO.StateName, anchor.EmoteSO.Layer, out float clipLen))
        {
            effectiveLen = Mathf.Max(0.01f, clipLen);

            // 길이 불일치 경고(디버깅용)
            if (Mathf.Abs(effectiveLen - anchor.EmoteSO.Length) > 0.05f)
                Debug.LogWarning($"[Emote] SO.Length({anchor.EmoteSO.Length:F2}) != Clip.Length({effectiveLen:F2}) → SO 길이 보정 권장");
        }

        float t = (float)(((PhotonNetwork.Time - start) % effectiveLen) / effectiveLen);
        BeginJoin(anchor, slot, t);
    }

    /// <summary>
    /// 나가기(슬롯 해제 후 복귀)
    /// </summary>
    public void RequestLeave()
    {
        if (!_Anchor) return;

        // 슬롯 해제
        EmoteManager._Inst.LeaveEmote(_Anchor, this);

        // 모든 클라에 나의 복귀를 통지 (다른 클라 Animator도 Idle로 전환)
        photonView.RPC(nameof(RPC_ForceLeaveAndReturn), RpcTarget.All);
    }


    // ===== 내부 합류/재생 =====

    /// <summary>
    /// 사전 계산된 진행률로 합류 시작(권장 경로)
    /// </summary>
    public void BeginJoin(EmoteAnchor anchor, int slotIndex, float normalizedTime)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        if (photonView.IsMine)
        {
            // 복귀 지점 저장 + 슬롯 위치/각도 스냅
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;
            transform.SetPositionAndRotation(anchor.GetSlotWorldPos(slotIndex), anchor.GetSlotWorldRot(slotIndex));
        }

        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All,
            anchor.photonView.ViewID, slotIndex, Mathf.Clamp01(normalizedTime));
    }

    /// <summary>
    /// 레거시 경로(내부에서 진행률 산출해서 호출) — 유지용
    /// </summary>
    public void BeginJoin(EmoteAnchor anchor, int slotIndex)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        double start = PhotonNetwork.Time;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;
        if (room != null && room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            start = (double)startObj;

        float effectiveLen = anchor.EmoteSO.Length;
        if (TryResolveStateLength(_Anim, anchor.EmoteSO.StateName, anchor.EmoteSO.Layer, out var clipLen))
            effectiveLen = Mathf.Max(0.01f, clipLen);

        float t = (float)(((PhotonNetwork.Time - start) % effectiveLen) / effectiveLen);
        BeginJoin(anchor, slotIndex, t);
    }

    [PunRPC]
    public void RPC_PlayEmote(int anchorViewId, int slotIndex, float normalizedTime)
    {
        var pv = PhotonView.Find(anchorViewId);
    var anchor = pv ? pv.GetComponent<EmoteAnchor>() : null;
    if (!anchor || !anchor.EmoteSO) { SafeLeaveIfBroken(); return; }

    _Anchor = anchor;
    _SlotIndex = slotIndex;
    _IsInEmote = true;

    var so = anchor.EmoteSO;

        if (_Anim)
        {
            // 정확한 진행도 보장: Play(state, layer, normalizedTime)
            _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime));
            _Anim.Update(0f); // 즉시 평가(프레임 지연 없이 시점 반영)
        }    
    }

    /// <summary>
    /// 로컬 복귀(애니메이터/위치 되돌림)
    /// </summary>
    public void DoLeaveAndReturn()
    {
        if (_IsInEmote && _Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

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
        Debug.Log("[Emote] 참여 거절: 슬롯 가득 참");
        // TODO: UI 토스트
    }

    /// <summary>
    /// 내 PhotonViewID
    /// </summary>
    public int TryGetViewID()
    {
        var pv = GetComponent<PhotonView>();
        return pv ? pv.ViewID : -1;
    }

    // ===== 안전장치/전파 수신 =====

    /// <summary>
    /// 룸 프로퍼티 변경 수신: 내 앵커의 ACTIVE=false 전파 시 즉시 복귀
    /// </summary>
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!_IsInEmote || _Anchor == null) return;

        int vid = _Anchor.photonView ? _Anchor.photonView.ViewID : -1;
        if (vid < 0) { SafeLeaveIfBroken(); return; }

        var key = EmoteManager.KEY_ACTIVE(vid);
        if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey(key))
        {
            var active = propertiesThatChanged[key] as bool?;
            if (active.HasValue && active.Value == false)
                DoLeaveAndReturn(); // 앵커 종료됨 → 즉시 복귀
        }
    }

    void LateUpdate()
    {
        if (!_IsInEmote) return;

        // 앵커 파괴/분실 감지
        if (_Anchor == null || _Anchor.photonView == null || PhotonView.Find(_Anchor.photonView.ViewID) == null)
        {
            SafeLeaveIfBroken();
            return;
        }

        // 소유자는 슬롯에 스냅 유지(원격은 PTV 보간)
        if (photonView.IsMine)
        {
            transform.SetPositionAndRotation(
                _Anchor.GetSlotWorldPos(_SlotIndex),
                _Anchor.GetSlotWorldRot(_SlotIndex)
            );
        }
    }

    void SafeLeaveIfBroken()
    {
        if (_IsInEmote) DoLeaveAndReturn();
    }

    /// <summary>
    /// 애니메이터에서 상태(클립) 길이 추정. 클립 이름 == 상태 이름을 우선 매칭.
    /// </summary>
    bool TryResolveStateLength(Animator anim, string stateName, int layer, out float length)
    {
        length = 0f;
        if (!anim || string.IsNullOrEmpty(stateName)) return false;

        var rac = anim.runtimeAnimatorController;
        if (!rac) return false;

        foreach (var clip in rac.animationClips)
        {
            if (!clip) continue;
            if (clip.name == stateName)
            {
                length = Mathf.Max(0.01f, clip.length);
                return true;
            }
        }
        return false;
    }


}
