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
    public InfoDataSO GetObjectInfo()
    {
        return EmoteSO.InfoDataSO;
    }

    public void OnFocus()
    {
        // 로컬 플레이어가 이모트 중이면 포커스 UI를 강제로 끄고, 더 진행하지 않음
        var lp = PlayerSetup._LocalPlayer ? PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>() : null;
        if (lp && lp.InEmote)
        {
            GameEvents.RaiseDefocus();
            return;
        }
        var info = GetObjectInfo();
        GameEvents.RaiseFocus(info);
    }

    public void OnDefocus() => GameEvents.RaiseDefocus();

    public void OnInteract()
    {
        var local = PlayerSetup._LocalPlayer.GetComponent<PlayerEmote>();
        if (!local)
        {
            Debug.LogWarning("[Emote] 로컬 PlayerEmote 없음");
            return;
        }

        bool iAmOwnerOfAnchor = photonView.IsMine;

        if (local.InEmote && ReferenceEquals(local.GetCurrentAnchor(), this))
        {
            if (iAmOwnerOfAnchor)
            {
                // 앵커 소유자(생성자)만 전체 종료 권한
                EmoteManager._Inst?.StopEmote(this);
            }
            else
            {
                // 참여자이면 내 클라만 나가기
                local.RequestLeave();
            }
            return;
        }

        // 이모트 중이 아니면 참여 시도
        local.RequestJoinSequential(this);
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

    private void OnEnable()
    {
        // 앵커 소유자(=생성자)만 자동 종료 워치독 수행
        if (PhotonNetwork.InRoom && photonView.IsMine)
            StartCoroutine(Co_AutoStopAfterLength());
    }

    private IEnumerator Co_AutoStopAfterLength()
    {
        // EmoteSO/START 준비 대기
        int vid = photonView.ViewID;
        while (_EmoteSO == null) yield return null;

        double start = 0;
        
        while (true)
        {
            var room = PhotonNetwork.CurrentRoom;
            if (room == null) yield break;

            if (room.CustomProperties.TryGetValue(EmoteManager.KEY_START(vid), out var startObj))
            {
                start = (double)startObj;
                break;
            }
            yield return null;
        }

        double len = _EmoteSO.Length;
        while (PhotonNetwork.Time - start < len - 1e-3)
            yield return null;

        // 길이만큼 경과 → 소유자가 종료
        EmoteManager._Inst?.StopEmote(this);
    }
}
