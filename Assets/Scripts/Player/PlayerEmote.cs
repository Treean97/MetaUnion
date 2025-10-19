using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime; // ★ 추가
using UnityEngine;

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

    private Pooled2DAudioPlayer _Sfx2D;
    private string _BgmToken;

    private Vector3 _ReturnPos;
    private Quaternion _ReturnRot;

    // ===== 솔로 이모트 =====
    Coroutine _SoloWatchdog;
    string _SoloCurrentId;

    public bool InEmote => _IsInEmote;
    public int CurrentSlotIndex => _SlotIndex;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;

    void Awake() => _Anim = GetComponentInChildren<Animator>();

    #region 그룹
    // 참여 시도
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
        if (room != null && room.CustomProperties.TryGetValue(EmoteKeys._START(vid), out var startObj))
            start = (double)startObj;

        float emoteLen = Mathf.Max(0.01f, anchor.EmoteSO.Length);
        float t = (float)(((PhotonNetwork.Time - start) % emoteLen) / emoteLen);

        BeginJoin(anchor, slot, t);
    }

    public void RequestLeave()
    {
        if (!_Anchor) return;
        EmoteManager._Inst.LeaveEmote(_Anchor, this);
        photonView.RPC(nameof(RPC_ForceLeaveAndReturn), RpcTarget.All);
    }

    public void BeginJoin(EmoteAnchor anchor, int slotIndex, float normalizedTime)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        if (photonView.IsMine)
        {
            _ReturnPos = transform.position;
            _ReturnRot = transform.rotation;

            // 슬롯의 월드 포즈로 이동
            transform.SetPositionAndRotation(anchor.GetSlotWorldPos(slotIndex), anchor.GetSlotWorldRot(slotIndex));
            transform.SetParent(anchor.transform, true);
        }

        photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All,
            anchor.photonView.ViewID, slotIndex, Mathf.Clamp01(normalizedTime));
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
            _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime));
            _Anim.Update(0f);
        }

        StartEmoteAudio(normalizedTime);
    }

    public void DoLeaveAndReturn()
    {
        StopEmoteAudio();

        if (_IsInEmote && _Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        if (photonView.IsMine)
        {
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(_ReturnPos, _ReturnRot);
        }

        _IsInEmote = false;
        _Anchor = null;
        _SlotIndex = -1;
    }

    [PunRPC] public void RPC_ForceLeaveAndReturn() => DoLeaveAndReturn();

    public void OnJoinRejected_Full() => Debug.Log("[Emote] 참여 거절: 슬롯 가득 참");

    public int TryGetViewID()
    {
        var pv = GetComponent<PhotonView>();
        return pv ? pv.ViewID : -1;
    }

    // 룸 프로퍼티 수신
    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (!_IsInEmote || _Anchor == null) return;

        int vid = _Anchor.photonView ? _Anchor.photonView.ViewID : -1;
        if (vid < 0) { SafeLeaveIfBroken(); return; }

        var key = EmoteKeys._ACTIVE(vid);
        if (changed != null && changed.ContainsKey(key))
        {
            var active = changed[key] as bool?;
            if (active.HasValue && active.Value == false)
                DoLeaveAndReturn();
        }
    }

    void SafeLeaveIfBroken()
    {
        if (_IsInEmote) DoLeaveAndReturn();
    }

    //  오디오 제어(그룹)
    void StartEmoteAudio(float normalizedTime)
    {
        var anchor = _Anchor;
        var so = anchor ? anchor.EmoteSO : null;
        if (!so) return;

        _BgmToken = anchor ? $"emote-{anchor.photonView.ViewID}" : "emote";
        AudioManager._Inst?.BeginBGMMuteRuntime(_BgmToken, 0.1f);

        string key = so.SFXKey;
        if (string.IsNullOrEmpty(key) || AudioManager._Inst == null) return;

        float sfxLen;
        if (AudioManager._Inst.TryGetAudioLengthByKey(key, out sfxLen) && sfxLen > 0f)
        {
            float offsetSec = Mathf.Clamp01(normalizedTime) * sfxLen;
            _Sfx2D = AudioManager._Inst.Play2DLoopFromOffsetByKey(key, offsetSec);
        }
        else
        {
            _Sfx2D = AudioManager._Inst.Play2DLoopLocalPlayByKey(key);
        }
    }

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
    #endregion
    #region 솔로
    public void RequestStartSolo(EmoteSO so)
    {
        if (!photonView.IsMine) return;
        if (so == null || so.PlayMode != EmotePlayMode.Solo) return;
        if (_IsInEmote) return; // 그룹 이모트 중엔 금지 (기존 흐름 보호)

        // 이미 솔로 중: 같은 SO면 무시, 다른 SO면 교체
        if (_SoloWatchdog != null && _SoloCurrentId == so.ID) return;
        if (_SoloWatchdog != null) RequestStopSolo();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
            { EmoteKeys._SOLO_ID, so.ID },
            { EmoteKeys._SOLO_START, PhotonNetwork.Time }
        });

        PlaySoloLocal(so, 0f);

        _SoloWatchdog = StartCoroutine(Co_AutoStopSoloAfter(so.Length));
        _SoloCurrentId = so.ID;
    }

    /// <summary>싱글(솔로) 이모트 종료: 커스텀 프로퍼티 삭제 → 전체 동기화.</summary>
    public void RequestStopSolo()
    {
        if (!photonView.IsMine) return;
        if (_SoloWatchdog == null) return;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
            { EmoteKeys._SOLO_ID, null },
            { EmoteKeys._SOLO_START, null }
        });

        StopSoloLocal();
        _SoloCurrentId = null;
    }

    System.Collections.IEnumerator Co_AutoStopSoloAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        RequestStopSolo();
    }

    // 로컬 재생/정리
    void PlaySoloLocal(EmoteSO so, float normalizedTime)
    {
        if (_Anim)
        {
            _Anim.Play(so.StateName, so.Layer, Mathf.Clamp01(normalizedTime));
            _Anim.Update(0f);
        }

        _IsInEmote = true;

        // 오디오: 솔로 전용 토큰
        _BgmToken = $"solo-{photonView.ViewID}";
        StartSoloAudio(so.SFXKey, normalizedTime);
    }

    void StopSoloLocal()
    {
        StopSoloAudio();

        if (_Anim && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        if (_SoloWatchdog != null) { StopCoroutine(_SoloWatchdog); _SoloWatchdog = null; }

        _IsInEmote = false;
    }

    // 다른 클라가 내 솔로 상태를 복구할 수 있도록 플레이어 커스텀 프로퍼티 수신
    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        if (photonView == null || photonView.Owner != target) return;
        if (changedProps == null) return;

        bool touched = changedProps.ContainsKey(EmoteKeys._SOLO_ID) || changedProps.ContainsKey(EmoteKeys._SOLO_START);
        if (!touched) return;

        string id = target.CustomProperties.TryGetValue(EmoteKeys._SOLO_ID, out var idObj) ? idObj as string : null;
        double start = (target.CustomProperties.TryGetValue(EmoteKeys._SOLO_START, out var stObj) && stObj is double d) ? d : -1;

        if (string.IsNullOrEmpty(id) || start < 0)
        {
            // 종료 신호
            StopSoloLocal();
            _SoloCurrentId = null;
            return;
        }

        if (EmoteManager._Inst != null && EmoteManager._Inst.TryGetById(id, out var so))
        {
            float len = Mathf.Max(0.01f, so.Length);
            float t = (float)(((PhotonNetwork.Time - start) % len) / len);
            PlaySoloLocal(so, t);
            _SoloCurrentId = id;

            // 남은 시간으로 워치독 재설정
            if (_SoloWatchdog != null) StopCoroutine(_SoloWatchdog);
            _SoloWatchdog = StartCoroutine(Co_AutoStopSoloAfter(len * (1f - t)));
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();

        // 씬 활성화 시, 오너의 현 솔로 상태 즉시 복원(늦게 붙은 경우)
        if (photonView && photonView.Owner != null)
        {
            var owner = photonView.Owner;
            string id = owner.CustomProperties.TryGetValue(EmoteKeys._SOLO_ID, out var idObj) ? idObj as string : null;
            double start = (owner.CustomProperties.TryGetValue(EmoteKeys._SOLO_START, out var stObj) && stObj is double d) ? d : -1;

            if (!string.IsNullOrEmpty(id) && start >= 0 &&
                EmoteManager._Inst != null && EmoteManager._Inst.TryGetById(id, out var so))
            {
                float len = Mathf.Max(0.01f, so.Length);
                float t = (float)(((PhotonNetwork.Time - start) % len) / len);
                PlaySoloLocal(so, t);
                _SoloCurrentId = id;

                if (_SoloWatchdog != null) StopCoroutine(_SoloWatchdog);
                _SoloWatchdog = StartCoroutine(Co_AutoStopSoloAfter(len * (1f - t)));
            }
        }
    }

    public override void OnDisable()
    {
        base.OnDisable(); 

        if (_SoloWatchdog != null) { StopCoroutine(_SoloWatchdog); _SoloWatchdog = null; }
    }

    // 솔로용 오디오
    void StartSoloAudio(string sfxKey, float normalizedTime)
    {
        AudioManager._Inst?.BeginBGMMuteRuntime(_BgmToken, 0.1f);
        if (string.IsNullOrEmpty(sfxKey) || AudioManager._Inst == null) return;

        if (AudioManager._Inst.TryGetAudioLengthByKey(sfxKey, out var sfxLen) && sfxLen > 0f)
        {
            float offsetSec = Mathf.Clamp01(normalizedTime) * sfxLen;
            _Sfx2D = AudioManager._Inst.Play2DLoopFromOffsetByKey(sfxKey, offsetSec);
        }
        else
        {
            _Sfx2D = AudioManager._Inst.Play2DLoopLocalPlayByKey(sfxKey);
        }
    }

    void StopSoloAudio()
    {
        if (_Sfx2D != null) { _Sfx2D.StopAndReturn(); _Sfx2D = null; }
        if (!string.IsNullOrEmpty(_BgmToken))
        {
            AudioManager._Inst?.EndBGMMuteRuntime(_BgmToken, 0.1f);
            _BgmToken = null;
        }
    }
    #endregion
}
