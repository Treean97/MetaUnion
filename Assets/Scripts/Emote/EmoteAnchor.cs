using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EmoteAnchor : MonoBehaviourPun, IInteractable, IPunInstantiateMagicCallback
{
    [Header("Slots (인스펙터에서 수동 할당)")]
    [SerializeField] private List<Transform> _slots = new(); // Slot_0, Slot_1... 순서대로
    private EmoteSO _EmoteSO;
    public EmoteSO EmoteSO => _EmoteSO;
    public int SlotCount => _slots?.Count ?? 0;

    ItemInfoSO _TempFocusInfo;

    public void Setup(EmoteSO data)
    {
        _EmoteSO = data;
    }

    public ItemInfoSO GetObjectInfo()
    {
        if (_TempFocusInfo == null)
        {
            _TempFocusInfo = ScriptableObject.CreateInstance<ItemInfoSO>();
            _TempFocusInfo.DisplayName = _EmoteSO.DisplayName;
            _TempFocusInfo.Description = "\"E\"를 눌러 이모트에 참여하세요";
        }
        return _TempFocusInfo;
    }

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

    // 상호작용 헬퍼
    public void InteractJoin(PlayerEmote p) => EmoteManager._Inst?.RequestJoinSequential(this, p);
    public void InteractLeave(PlayerEmote p) => EmoteManager._Inst?.RequestLeave(this, p);

    public void OnDefocus() => GameEvents.RaiseDefocus();

    public void OnFocus()
    {
        var info = GetObjectInfo();
        GameEvents.RaiseFocus(info);
    }

    public void OnInteract()
    {
        var local = PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>();
        if (!local)
        {
            Debug.LogWarning("[Emote] 로컬 PlayerEmote를 찾을 수 없습니다.");
            return;
        }

        if (local.InEmote && ReferenceEquals(local.GetCurrentAnchor(), this))
            InteractLeave(local);
        else
            InteractJoin(local);
    }
    
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
