using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// UI/상호작용으로 이미 진행 중인 이모트에 참여/나가기.
/// - 참여 시: EmoteManager.JoinEmote로 슬롯 예약 → RPC로 전 클라 재생
/// - 나가기: EmoteManager.LeaveEmote로 해제 → 복귀
/// - 위치/각도 스냅은 '소유자만', 나머지는 Photon Transform View가 보간
/// - ⬇️ 오디오 정책:
///    * 이모트 입장 시: EmoteSO.SFXKey 로컬 루프 재생(진행도에 맞춘 오프셋), BGM 뮤트 토큰 ON
///    * 이모트 종료/이탈/앵커 파괴 시: SFX 정지, BGM 뮤트 토큰 OFF
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

    // ⬇️ 오디오 핸들
    private Pooled2DAudioPlayer _Sfx2D;   // 이모트 SFX 루프 플레이어(2D 전용)
    private string _BgmToken;             // BGM 런타임 뮤트 토큰 (앵커별로 고유하게)

    // 복귀 지점(소유자만)
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

        // 앵커의 시작 시간으로부터 정규화 진행도 산출 (SO.Length 기준)
        double start = PhotonNetwork.Time;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;
        if (room != null && room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            start = (double)startObj;

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

        EmoteManager._Inst.LeaveEmote(_Anchor, this);
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
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;
            transform.SetPositionAndRotation(anchor.GetSlotWorldPos(slotIndex), anchor.GetSlotWorldRot(slotIndex));
        }

        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All,
            anchor.photonView.ViewID, slotIndex, Mathf.Clamp01(normalizedTime));
    }

    /// <summary>
    /// 레거시 경로(내부에서 진행률 산출)
    /// </summary>
    public void BeginJoin(EmoteAnchor anchor, int slotIndex)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        double start = PhotonNetwork.Time;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;
        if (room != null && room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            start = (double)startObj;

        float emoteLen = Mathf.Max(0.01f, anchor.EmoteSO.Length);
        float t = (float)(((PhotonNetwork.Time - start) % emoteLen) / emoteLen);

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

        // 애니메이션: 정확한 진행도 반영
        if (_Anim)
        {
            _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime));
            _Anim.Update(0f);
        }

        // ⬇️ 오디오: 진행도 기반 오프셋으로 로컬 루프 재생 + BGM 뮤트
        StartEmoteAudio(normalizedTime);
    }

    /// <summary>
    /// 로컬 복귀(애니메이터/위치 되돌림) + 오디오 정리
    /// </summary>
    public void DoLeaveAndReturn()
    {
        // ⬇️ 오디오 종료/원복
        StopEmoteAudio();

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

    /// <summary>내 PhotonViewID</summary>
    public int TryGetViewID()
    {
        var pv = GetComponent<PhotonView>();
        return pv ? pv.ViewID : -1;
    }

    // ===== 룸 프로퍼티 수신 =====
    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (!_IsInEmote || _Anchor == null) return;

        int vid = _Anchor.photonView ? _Anchor.photonView.ViewID : -1;
        if (vid < 0) { SafeLeaveIfBroken(); return; }

        var key = EmoteManager.KEY_ACTIVE(vid);
        if (changed != null && changed.ContainsKey(key))
        {
            var active = changed[key] as bool?;
            if (active.HasValue && active.Value == false)
                DoLeaveAndReturn(); // 앵커 종료됨 → 즉시 복귀(+오디오 정리)
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

    // ===== 내부: 오디오 제어 =====

    /// <summary>
    /// 이모트 시작 시 SFX 루프 + BGM 뮤트.
    /// normalizedTime(0~1)을 SFX 길이에 매핑해 오프셋 재생.
    /// </summary>
    void StartEmoteAudio(float normalizedTime)
    {
        var anchor = _Anchor;
        var so = anchor ? anchor.EmoteSO : null;
        if (!so) return;

        // BGM 뮤트 토큰(앵커 ViewID로 고유화)
        _BgmToken = anchor ? $"emote-{anchor.photonView.ViewID}" : "emote";
        AudioManager._Inst?.BeginBGMMuteRuntime(_BgmToken, 0.1f);

        // SFX 키가 없으면 BGM 뮤트만 적용
        string key = so.SFXKey;
        if (string.IsNullOrEmpty(key) || AudioManager._Inst == null) return;

        // 진행도를 SFX 길이에 매핑 → 오프셋 계산
        float sfxLen;
        if (AudioManager._Inst.TryGetAudioLengthByKey(key, out sfxLen) && sfxLen > 0f)
        {
            float offsetSec = Mathf.Clamp01(normalizedTime) * sfxLen;
            _Sfx2D = AudioManager._Inst.Play2DLoopFromOffsetByKey(key, offsetSec);
        }
        else
        {
            // 길이 확인 불가 → 그냥 루프 시작(0초부터)
            _Sfx2D = AudioManager._Inst.Play2DLoopLocalPlayByKey(key);
        }
    }

    /// <summary>
    /// 이모트 종료/이탈/강제종료 시 SFX 정지 + BGM 원복.
    /// </summary>
    void StopEmoteAudio()
    {
        if (_Sfx2D != null)
        {
            _Sfx2D.StopAndReturn();
            _Sfx2D = null;
        }
        if (!string.IsNullOrEmpty(_BgmToken))
        {
            AudioManager._Inst?.EndBGMMuteRuntime(_BgmToken, 0.1f);
            _BgmToken = null;
        }
    }
}
