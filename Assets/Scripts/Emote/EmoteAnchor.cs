using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 슬롯 기반 위치/각도 제공, 최대 인원 = 슬롯 수.
/// 포톤 생성 시 EmoteSO 복구.
/// </summary>
public class EmoteAnchor : MonoBehaviourPun, IPunInstantiateMagicCallback, IInteractable
{
    [Header("Slots (인스펙터에서 수동 할당)")]
    [SerializeField] private List<Transform> _slots = new(); // Slot_0, Slot_1...
    private EmoteSO _EmoteSO;
    public EmoteSO EmoteSO => _EmoteSO;
    public int SlotCount => _slots?.Count ?? 0;

    ItemInfoSO _TempFocusInfo;

    void OnEnable()  => EmoteManager.RegisterAnchor(this);
    void OnDisable() => EmoteManager.UnregisterAnchor(this);

    public void Setup(EmoteSO so) => _EmoteSO = so;

    public Vector3 GetSlotWorldPos(int index)
    {
        if (_slots == null || index < 0 || index >= _slots.Count || !_slots[index])
            return transform.position;
        return _slots[index].position;
    }

    public Quaternion GetSlotWorldRot(int index)
    {
        if (_slots == null || index < 0 || index >= _slots.Count || !_slots[index])
            return transform.rotation;
        return _slots[index].rotation;
    }

    // ==== IInteractable ====
    public ItemInfoSO GetObjectInfo()
    {
        if (_TempFocusInfo == null)
        {
            _TempFocusInfo = ScriptableObject.CreateInstance<ItemInfoSO>();
            _TempFocusInfo.DisplayName = _EmoteSO ? _EmoteSO.DisplayName : "Emote";
            _TempFocusInfo.Description = "\"E\"를 눌러 이모트에 참여하세요";
        }
        return _TempFocusInfo;
    }

    public void OnFocus()
    {
        var lp = PlayerSetup._LocalPlayer ? PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>() : null;
        if (lp && lp.InEmote) { GameEvents.RaiseDefocus(); return; }
        GameEvents.RaiseFocus(GetObjectInfo());
    }

    public void OnDefocus() => GameEvents.RaiseDefocus();

    public void OnInteract()
    {
        var local = PlayerSetup._LocalPlayer ? PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>() : null;
        if (!local) { Debug.LogWarning("[Emote] 로컬 PlayerEmote 없음"); return; }

        bool iAmOwnerOfAnchor = photonView.IsMine;

        if (local.InEmote && ReferenceEquals(local.GetCurrentAnchor(), this))
        {
            if (iAmOwnerOfAnchor)
                EmoteManager._Inst?.StopEmote(this); // 주최자: 전체 종료
            else
                local.RequestLeaveViaRoomProp(); // 참여자: 내 슬롯 비우고 종료
            return;
        }

        // 이모트 중이 아니면 참여 시도(룸 프로퍼티 기반)
        local.RequestJoinViaRoomProp(this);
        OnDefocus();
    }

    // ==== 포톤 인스턴스 데이터로 SO 복구 ====
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView?.InstantiationData;
        if (data != null && data.Length > 0 && data[0] is string emoteId)
        {
            if (EmoteManager._Inst != null && EmoteManager._Inst.TryGetById(emoteId, out var so))
                _EmoteSO = so;
            else
                Debug.LogError($"[EmoteAnchor] EmoteSO 복구 실패: {emoteId}");
        }
    }

}
