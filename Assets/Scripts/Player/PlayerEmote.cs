using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class PlayerEmote : MonoBehaviourPun
{
    [Header("Animator")]
    [SerializeField] private Animator _Animator;

    [Header("Movement Snap")]
    [SerializeField] private bool _SnapToSlot = true;
    [SerializeField] private bool _MatchRotationY = true;

    // 상태
    public bool IsInEmote { get; private set; }
    public bool IsLocal => photonView.IsMine;
    public EmoteAnchor CurrentAnchor { get; private set; }
    public int CurrentSlotIndex { get; private set; } = -1;

    // 메타(현재 참여 이모트)
    string _stateName;
    int    _layer;
    float  _length;

    void Awake()
    {
        if (!_Animator) _Animator = GetComponent<Animator>();
    }

    /// <summary>앵커 상호작용(외부 입력에서 호출)</summary>
    public void InteractAnchor(EmoteAnchor anchor)
    {
        if (!IsLocal || !anchor) return;
        anchor.Interact_AsParticipant(this);
    }

    /// <summary>주최 종료 버튼(주최자일 때만)</summary>
    public void InteractEndIfHost(EmoteAnchor anchor)
    {
        if (!IsLocal || !anchor) return;
        anchor.Interact_AsHostEnd();
    }

    /// <summary>참여(슬롯 확보 후 호출)</summary>
    public void JoinEmote(EmoteAnchor anchor, int slotIndex, Transform slot, float normalizedFromRoom)
    {
        if (!IsLocal) return;

        CurrentAnchor    = anchor;
        CurrentSlotIndex = slotIndex;
        IsInEmote        = true;

        // 1) SO 레지스트리 우선
        string state; int layer; float len;
        if (EmoteManager._Inst.TryGetSO(anchor.EmoteId, out var so) && so)
        {
            state = so.StateName;
            layer = so.Layer;
            len   = so.Length;
        }
        else
        {
            // 2) 룸 스냅샷 폴백
            state = anchor.AnimatorState;
            layer = anchor.AnimatorLayer;
            len   = (float)anchor.EmoteLength;
        }

        _stateName = state;
        _layer     = layer;
        _length    = Mathf.Max(0.01f, len);

        // 위치/회전 맞추기
        if (_SnapToSlot && slot) transform.position = slot.position;
        if (_MatchRotationY && slot)
        {
            var e = transform.eulerAngles;
            e.y = slot.eulerAngles.y;
            transform.rotation = Quaternion.Euler(e);
        }

        // 이어 재생
        float t = Mathf.Repeat(normalizedFromRoom, 1f);
        _Animator.Play(_stateName, _layer, t);
        _Animator.Update(0f); // 즉시 반영
    }

    /// <summary>탈출(상호작용으로 호출)</summary>
    public void LeaveEmote()
    {
        if (!IsLocal) return;

        // 슬롯 해제
        if (CurrentAnchor && CurrentSlotIndex >= 0)
            EmoteManager._Inst.ReleaseSlot(CurrentAnchor, CurrentSlotIndex);

        // 상태 초기화
        IsInEmote = false;
        CurrentSlotIndex = -1;
        CurrentAnchor = null;

        // 필요 시 Idle 등으로 복귀
        // _Animator.CrossFade("Idle", 0.1f, 0);
    }

    // 편의
    public int ActorNumber => PhotonNetwork.LocalPlayer?.ActorNumber ?? -1;
}
