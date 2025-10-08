using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// - Emote 시작/종료 관제
/// - 룸 프로퍼티에 스냅샷 기록(CAS로 슬롯 점유/해제)
/// - SO 레지스트리(id -> EmoteSO) 제공(편의/에디팅)
/// </summary>
public sealed class EmoteManager : MonoBehaviourPunCallbacks
{
    public static EmoteManager _Inst { get; private set; }

    [Header("Emote Catalog (에디터에서 등록)")]
    [SerializeField] private List<EmoteSO> _Catalog = new();

    private readonly Dictionary<string, EmoteSO> _map = new();

    // ==== RoomProperty 키 규칙 ====
    // "EMOTE_<anchorViewId>": Hashtable { "id":string, "state":string, "layer":int, "len":double, "t0":double, "host":int, "occ":int[] }
    const string PROP_PREFIX = "EMOTE_";
    const string K_ID = "id";
    const string K_STATE = "state";
    const string K_LAYER = "layer";
    const string K_LEN = "len";
    const string K_T0 = "t0";
    const string K_HOST = "host";
    const string K_OCC = "occ";

    void Awake()
    {
        if (_Inst && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;

        _map.Clear();
        foreach (var so in _Catalog)
        {
            if (!so || string.IsNullOrWhiteSpace(so.ID)) continue;
            _map[so.ID] = so;
        }
    }

    public bool TryGetSO(string id, out EmoteSO so) => _map.TryGetValue(id, out so);

    /// <summary>외부에서 호출: 이모트 시작(주최)</summary>
    public void RequestStartEmote(EmoteSO so, Vector3 worldPos, Quaternion worldRot, int slotCount)
    {
        if (!PhotonNetwork.InRoom) { Debug.LogWarning("[EmoteManager] 방 밖."); return; }
        if (!so || !so.EmoteAnchor) { Debug.LogError("[EmoteManager] SO 또는 Anchor 프리팹 없음."); return; }
        slotCount = Mathf.Max(0, slotCount);

        double t0 = PhotonNetwork.Time; // 서버 기준 시작 시각

        object[] instData = new object[]
        {
            so.ID,             // 0
            so.StateName,      // 1
            so.Layer,          // 2
            so.Length,         // 3
            t0,                // 4
            PhotonNetwork.LocalPlayer.ActorNumber, // 5 host
            slotCount          // 6
        };

        GameObject go = PhotonNetwork.Instantiate(so.EmoteAnchor.name, worldPos, worldRot, 0, instData);
        var view = go.GetComponent<PhotonView>();
        if (!view)
        {
            Debug.LogError("[EmoteManager] Anchor 프리팹에 PhotonView 필요.");
            return;
        }

        // 룸 프로퍼티 스냅샷 기록
        var occ = new int[slotCount]; // 0:비어있음, 1:점유
        var table = new Hashtable
        {
            { K_ID,    so.ID },
            { K_STATE, so.StateName },
            { K_LAYER, so.Layer },
            { K_LEN,   (double)so.Length },
            { K_T0,    t0 },
            { K_HOST,  PhotonNetwork.LocalPlayer.ActorNumber },
            { K_OCC,   occ }
        };

        string key = PROP_PREFIX + view.ViewID;
        var roomProps = new Hashtable { { key, table } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    /// <summary>외부/주최 호출: 해당 앵커 이모트 종료</summary>
    public void RequestEndEmote(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.TryGetComponent<PhotonView>(out var pv)) return;

        // 주최자만 종료 가능
        if (PhotonNetwork.LocalPlayer.ActorNumber != anchor.HostActorNumber)
        {
            Debug.LogWarning("[EmoteManager] 주최자만 종료 가능.");
            return;
        }

        // 참여자 모두 강제 탈출 알림
        anchor.KickAllParticipants_LocalAndAnnounce();

        // 룸 프로퍼티 제거
        string key = PROP_PREFIX + pv.ViewID;
        if (PhotonNetwork.CurrentRoom?.CustomProperties?.ContainsKey(key) == true)
        {
            var unset = new Hashtable { { key, null } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(unset);
        }

        // 앵커 파괴
        if (pv.IsMine) PhotonNetwork.Destroy(pv);
        else photonView.RPC(nameof(RPC_RequestMasterDestroy), RpcTarget.MasterClient, pv.ViewID);
    }

    [PunRPC]
    void RPC_RequestMasterDestroy(int viewId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var pv = PhotonView.Find(viewId);
        if (pv) PhotonNetwork.Destroy(pv);
    }

    /// <summary>슬롯 점유 시도(CAS). 성공 시 index, 실패 시 -1</summary>
    public int TryOccupySlot(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.TryGetComponent<PhotonView>(out var pv)) return -1;

        string key = PROP_PREFIX + pv.ViewID;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return -1;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            if (!room.CustomProperties.TryGetValue(key, out var boxed) || boxed is not Hashtable table)
                return -1;

            if (!table.TryGetValue(K_OCC, out var occObj) || occObj is not int[] occ) return -1;

            int idx = Array.FindIndex(occ, v => v == 0);
            if (idx < 0) return -1;

            var next = (int[])occ.Clone();
            next[idx] = 1;

            var set = (Hashtable)table.Clone();  // 깊은 복사(Photon Hashtable도 Clone 제공)
            set[K_OCC] = next;
            var expected = table; // 현재 값 그대로 기대값으로 사용

            bool ok = room.SetCustomProperties(
                new Hashtable { { key, set } },
                new Hashtable { { key, expected } }
            );
            if (ok) return idx;
            
        }
        return -1;
    }

    /// <summary>슬롯 해제(CAS)</summary>
    public void ReleaseSlot(EmoteAnchor anchor, int slotIndex)
    {
        if (!anchor || !anchor.TryGetComponent<PhotonView>(out var pv)) return;

        string key = PROP_PREFIX + pv.ViewID;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            if (!room.CustomProperties.TryGetValue(key, out var boxed) || boxed is not Hashtable table)
                return;

            if (!table.TryGetValue(K_OCC, out var occObj) || occObj is not int[] occ) return;
            if (slotIndex < 0 || slotIndex >= occ.Length) return;
            if (occ[slotIndex] == 0) return;

            var next = (int[])occ.Clone();
            next[slotIndex] = 0;

            var set = (Hashtable)table.Clone();
            set[K_OCC] = next;
            var expected = table;

            bool ok = room.SetCustomProperties(
                new Hashtable { { key, set } },
                new Hashtable { { key, expected } }
                );

            if (ok) return;
        }
    }

    /// <summary>앵커 상태 조회(늦게 합류자 재생점 계산)</summary>
    public bool TryGetAnchorState(
    EmoteAnchor anchor,
    out string id, out string state, out int layer,
    out double len, out double t0, out int host)
    {
        id = state = default;
        layer = default; len = default; t0 = default; host = default;

        if (!anchor || !anchor.TryGetComponent<PhotonView>(out var pv)) return false;

        string key = PROP_PREFIX + pv.ViewID;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return false;

        if (!room.CustomProperties.TryGetValue(key, out var boxed) || boxed is not Hashtable table)
            return false;

        if (!table.TryGetValue(K_ID, out var vIdObj)) return false;
        if (!table.TryGetValue(K_STATE, out var vStObj)) return false;
        if (!table.TryGetValue(K_LAYER, out var vLyObj)) return false;
        if (!table.TryGetValue(K_LEN, out var vLnObj)) return false;
        if (!table.TryGetValue(K_T0, out var vT0Obj)) return false;
        if (!table.TryGetValue(K_HOST, out var vHoObj)) return false;

        id = vIdObj as string;
        state = vStObj as string;
        layer = vLyObj is int iLy ? iLy : System.Convert.ToInt32(vLyObj);
        len = vLnObj is double dLn ? dLn : System.Convert.ToDouble(vLnObj);
        t0 = vT0Obj is double dT0 ? dT0 : System.Convert.ToDouble(vT0Obj);
        host = vHoObj is int iHo ? iHo : System.Convert.ToInt32(vHoObj);
        return true;
    }
    
    // 인덱스로 SO 가져오기(테스트용)
    public EmoteSO GetSOByIndex(int index)
    {
        if (index < 0 || index >= _Catalog.Count) return null;
        return _Catalog[index];
    }

    // 프리팹에서 슬롯 수 추출(프리팹 자산의 EmoteAnchor 컴포넌트 직독)
    public int GetSlotCountFromPrefab(EmoteSO so)
    {
        if (!so || !so.EmoteAnchor) return 0;
        var anchor = so.EmoteAnchor.GetComponent<EmoteAnchor>();
        return anchor ? anchor.SlotCount : 0;
    }
}
