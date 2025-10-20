using Controller;
using ExitGames.Client.Photon;
using Photon.Pun;
using PlayFab.Internal;
using UnityEngine;

/// <summary>
/// UI/상호작용으로 이미 진행 중인 이모트에 참여/나가기.
/// - 참여 시: EmoteManager.JoinEmote로 슬롯 예약 → RPC로 전 클라 재생(애니메이션만 동기)
/// - 오디오: 각 클라이언트 로컬에서만 처리(BGM 런타임 음소거 + SFXKey 루프/오프셋 재생)
/// - 종료/탈출: 로컬 SFX 정지 + BGM 음소거 해제
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

    // ==== 로컬 오디오 전용 ====
    private Pooled2DAudioPlayer _Sfx2D;  // 로컬에서만 렌트/반납
    private string _BgmToken;            // 런타임 BGM 음소거 토큰

    public bool InEmote => _IsInEmote;
    public int CurrentSlotIndex => _SlotIndex;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;

    bool _pendingSnap;
    Vector3 _snapPos;
    Quaternion _snapRot;

    

    private void Awake()
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

        double start = PhotonNetwork.Time;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (room != null && room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            start = (double)startObj;

        // 진행도는 '이모트 자체 길이(SO.Length)'로 계산
        float emoteLen = Mathf.Max(0.01f, anchor.EmoteSO.Length);
        float t = (float)(((PhotonNetwork.Time - start) % emoteLen) / emoteLen);

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
    /// 사전 계산된 진행률로 합류 시작
    /// </summary>
    public void BeginJoin(EmoteAnchor anchor, int slotIndex, float normalizedTime)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        if (photonView.IsMine)
        {
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;

            // 먼저 필드 세팅
            _Anchor = anchor;
            _SlotIndex = slotIndex;

            // 이모트 동안 회전 고정
            GetComponent<MoveHandler>()?.LockTurn();

                // // 슬롯 포즈로 1회 스냅
                // var slotPos = anchor.GetSlotWorldPos(slotIndex);
                // var slotRot = anchor.GetSlotWorldRot(slotIndex);
                // transform.SetPositionAndRotation(slotPos, slotRot);

            _snapPos = anchor.GetSlotWorldPos(slotIndex);
            _snapRot = anchor.GetSlotWorldRot(slotIndex);
            _pendingSnap = true;     
        }
    

        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All, anchor.photonView.ViewID, slotIndex, Mathf.Clamp01(normalizedTime));
    }

    [PunRPC]
    public void RPC_PlayEmote(int anchorViewId, int slotIndex, float normalizedTime)
    {
        var pv = PhotonView.Find(anchorViewId);
        var anchor = pv ? pv.GetComponent<EmoteAnchor>() : null;
        if (!anchor || !anchor.EmoteSO)
        {
            SafeLeaveIfBroken();
            return;
        }

        _Anchor = anchor;
        _SlotIndex = slotIndex;
        _IsInEmote = true;

        var so = anchor.EmoteSO;

        // === 애니메이션 동기 ===
        if (_Anim)
        {
            _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime));
            _Anim.Update(0f); // 즉시 평가
        }

        if (photonView.IsMine)
        {
            StartLocalEmoteAudio(so, normalizedTime, anchorViewId);    
        }
        
    }

    /// <summary>
    /// 로컬 오디오 시작: BGM 런타임 음소거 + SFXKey 루프 재생(진행지점 오프셋)
    /// </summary>
    private void StartLocalEmoteAudio(EmoteSO so, float normalizedTime, int anchorViewId)
    {
        // 이전 잔여 오디오 정리
        StopLocalEmoteAudio();

        var am = AudioManager._Inst;
        if (am == null) return;

        // 고유 토큰 생성(로컬에서만 의미 있음)
        _BgmToken = $"EMOTE_BGM_{photonView.ViewID}_{anchorViewId}";
        am.BeginBGMMuteRuntime(_BgmToken, 0.08f);

        // SFXKey 없으면 SFX 생략(요구사항: BGM은 이모트 중 항상 끔)
        string key = so.SFXKey;
        if (string.IsNullOrEmpty(key)) return;

        // 진행 지점 → 초 단위 오프셋 계산
        float offsetSec;
        if (am.TryGetAudioLengthByKey(key, out var sfxLen) && sfxLen > 0.0001f)
        {
            // SFX 고유 길이를 기준으로 페이즈 맞춤
            offsetSec = Mathf.Repeat(normalizedTime * sfxLen, sfxLen);
        }
        else
        {
            // SFX 길이 모르면 이모트 길이 기준으로 보정
            offsetSec = Mathf.Repeat(normalizedTime * so.Length, Mathf.Max(0.01f, so.Length));
        }

        // 2D 루프 + 오프셋 재생 (로컬)
        _Sfx2D = am.Play2DLoopFromOffsetByKey(key, offsetSec);
    }

    /// <summary>
    /// 로컬 오디오 정리: SFX 정지 + BGM 음소거 해제
    /// </summary>
    private void StopLocalEmoteAudio()
    {
        if (_Sfx2D != null)
        {
            _Sfx2D.StopAndReturn();
            _Sfx2D = null;
        }

        if (!string.IsNullOrEmpty(_BgmToken))
        {
            AudioManager._Inst?.EndBGMMuteRuntime(_BgmToken, 0.08f);
            _BgmToken = null;
        }
    }

    /// <summary>
    /// 로컬 복귀(애니메이터/위치 되돌림 + 오디오 복구)
    /// </summary>
    public void DoLeaveAndReturn()
    {       
        // 오디오 먼저 정리
        StopLocalEmoteAudio();

        if (_IsInEmote && _Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        if (photonView.IsMine)
            transform.SetPositionAndRotation(_ReturnPos, _ReturnRot);

        _Anchor = null;
        _SlotIndex = -1;
        _IsInEmote = false;
        GetComponent<MoveHandler>().UnlockTurn();
    }

    [PunRPC]
    public void RPC_ForceLeaveAndReturn() => DoLeaveAndReturn();

    public void OnJoinRejected_Full()
    {
        GameEvents.RaiseShowWarning("이모트의 정원이 가득 찼습니다.");
    }

    /// <summary>내 PhotonViewID</summary>
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
        if (vid < 0)
        {
            SafeLeaveIfBroken();
            return;
        }

        var key = EmoteManager.KEY_ACTIVE(vid);
        if (propertiesThatChanged != null && propertiesThatChanged.ContainsKey(key))
        {
            var active = propertiesThatChanged[key] as bool?;
            if (active.HasValue && active.Value == false)
                DoLeaveAndReturn(); // 앵커 종료됨 → 즉시 복귀(오디오 포함)
        }
    }

    private void LateUpdate()
    {
        if (_pendingSnap)
        {
            transform.SetPositionAndRotation(_snapPos, _snapRot);
            _pendingSnap = false;
        }
        
        if (!_IsInEmote) return;

        // 앵커 파괴/분실 감지
        if (_Anchor == null || _Anchor.photonView == null || PhotonView.Find(_Anchor.photonView.ViewID) == null)
        {
            SafeLeaveIfBroken();
            return;
        }

        // // 소유자는 슬롯에 스냅 유지(원격은 PTV 보간)
        // if (photonView.IsMine)
        // {
        //     transform.SetPositionAndRotation(
        //         _Anchor.GetSlotWorldPos(_SlotIndex),
        //         _Anchor.GetSlotWorldRot(_SlotIndex)
        //     );
        // }
    }

    private void SafeLeaveIfBroken()
    {
        if (_IsInEmote) DoLeaveAndReturn();
    }
}
