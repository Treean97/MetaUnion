using Photon.Pun;
using UnityEngine;

public class PlayerEmote : MonoBehaviour
{    
    [Header("Blend")]
    [SerializeField] float _CrossFade = 0.1f;
    [SerializeField] string _IdleState = "Idle";

    Animator _Anim;
    Vector3 _ReturnPos;
    Quaternion _ReturnRot;

    EmoteAnchor _Anchor;
    int _SlotIndex = -1;
    bool _IsInEmote;

    public int CurrentSlotIndex => _SlotIndex;
    public bool InEmote => _IsInEmote;
    public EmoteAnchor GetCurrentAnchor() => _Anchor;


    void Awake()
    {
        _Anim = GetComponent<Animator>();
    }

    public void RequestJoinSequential(EmoteAnchor anchor)
        => EmoteManager._Inst?.RequestJoinSequential(anchor, this);

    public void BeginJoin(EmoteAnchor anchor, int slotIndex)
    {
        if (!anchor || !anchor.EmoteSO || _IsInEmote) return;

        _Anchor = anchor;
        _SlotIndex = slotIndex;

        _ReturnPos = transform.position;
        _ReturnRot = transform.rotation;

        var dest = anchor.GetSlotWorldPos(slotIndex);
        var drot = anchor.GetSlotWorldRot(slotIndex);
        transform.SetPositionAndRotation(dest, drot);

        float t = EmoteManager.GetNormalizedTime(anchor);
        var so = anchor.EmoteSO;
        _Anim.CrossFadeInFixedTime(so.StateName, _CrossFade, so.Layer, t);

        _IsInEmote = true;
    }

    public void RequestLeave()
        => EmoteManager._Inst?.RequestLeave(_Anchor, this);

    public void DoLeaveAndReturn()
    {
        // 원래 자리 복귀
        transform.SetPositionAndRotation(_ReturnPos, _ReturnRot);

        if (_IsInEmote && !string.IsNullOrEmpty(_IdleState))
            _Anim.CrossFadeInFixedTime(_IdleState, 0.08f, 0, 0f);

        _IsInEmote = false;
        _Anchor = null;
        _SlotIndex = -1;
    }

    [PunRPC]
    public void RPC_ForceLeaveAndReturn()
    {
        // 호스트가 Stop 할 때 호출됨(내 클라에서만 실행)
        DoLeaveAndReturn();
    }

    public void OnJoinRejected_Full()
    {
        Debug.Log("[Emote] 참여 거절: 슬롯이 가득 찼습니다.");
    }
}
