using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// 룸 프로퍼티만으로 이모트 상태를 동기화/전파.
/// - 시작/종료: ACTIVE, ID, START, SLOTS 관리
/// - 참여/퇴장: SLOTS를 CAS로 갱신
/// - 반영: OnRoomPropertiesUpdate에서 로컬 적용(Reconcile)
/// - 지연합류/재접속: OnJoinedRoom에서 현재 프로퍼티 스캔 후 전체 동기화
/// </summary>
public class EmoteManager : MonoBehaviourPunCallbacks
{
    public static EmoteManager _Inst { get; private set; }

    [Header("Emote Catalog (인스펙터 등록)")]
    [SerializeField] private EmoteSO[] _EmoteSO;
    public EmoteSO[] EmoteSOs => _EmoteSO;
    private Dictionary<string, EmoteSO> _Map;

    // 현재 씬/룸에 존재하는 앵커 목록
    private static readonly List<EmoteAnchor> _Anchors = new();
    public static IReadOnlyList<EmoteAnchor> Anchors => _Anchors;
    public static void RegisterAnchor(EmoteAnchor a)   { if (a && !_Anchors.Contains(a)) _Anchors.Add(a); }
    public static void UnregisterAnchor(EmoteAnchor a) { if (a) _Anchors.Remove(a); }

    // vid -> (viewId -> slotIndex) 현재 우리가 알고 있는 상태
    private readonly Dictionary<int, Dictionary<int,int>> _present = new();

    void Awake()
    {
        if (_Inst && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;

        _Map = new Dictionary<string, EmoteSO>();
        if (_EmoteSO != null)
        {
            foreach (var so in _EmoteSO)
            {
                if (!so || string.IsNullOrEmpty(so.ID)) continue;
                _Map[so.ID] = so;
            }
        }
    }

    public bool TryGetById(string id, out EmoteSO so)
    {
        so = null;
        return !string.IsNullOrEmpty(id) && _Map != null && _Map.TryGetValue(id, out so);
    }

    // ----- Room Property Keys (앵커 ViewID 네임스페이스) -----
    static string K_ACTIVE(int vid) => $"_EMOTE_{vid}_ACTIVE";
    static string K_ID(int vid)     => $"_EMOTE_{vid}_ID";
    static string K_START(int vid)  => $"_EMOTE_{vid}_START";
    static string K_SLOTS(int vid)  => $"_EMOTE_{vid}_SLOTS";  // CSV "actor:viewId,actor:viewId,...", 빈 슬롯은 "-1:-1"

    public static double NOW => PhotonNetwork.Time;

    // ===== 유틸: CSV =====
    struct SlotEntry { public int actor; public int viewId; }
    static string BuildEmptyCsv(int count)
    {
        if (count <= 0) return "";
        var arr = new string[count];
        for (int i = 0; i < count; i++) arr[i] = "-1:-1";
        return string.Join(",", arr);
    }
    static SlotEntry[] ParseCsv(string csv)
    {
        if (string.IsNullOrEmpty(csv)) return Array.Empty<SlotEntry>();
        var parts = csv.Split(',');
        var res = new SlotEntry[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i].Split(':');
            int a = -1, v = -1;
            if (p.Length > 0) int.TryParse(p[0], out a);
            if (p.Length > 1) int.TryParse(p[1], out v);
            res[i].actor = a; res[i].viewId = v;
        }
        return res;
    }
    static string ToCsv(SlotEntry[] a)
    {
        var parts = new string[a.Length];
        for (int i = 0; i < a.Length; i++) parts[i] = $"{a[i].actor}:{a[i].viewId}";
        return string.Join(",", parts);
    }

    // ===== 시작(주최자) =====
    public EmoteAnchor StartEmoteHost(EmoteSO so, Vector3 pos, Quaternion rot, int autoReserveSlotForActor = -1, int autoReserveView = -1)
    {
        if (!so || !so.EmoteAnchor) { Debug.LogError("[Emote] SO/Anchor 누락"); return null; }

        var go = PhotonNetwork.Instantiate(so.EmoteAnchor.name, pos, rot, 0, new object[] { so.ID });
        var anchor = go.GetComponent<EmoteAnchor>();
        anchor.Setup(so);

        int vid = anchor.photonView.ViewID;

        // 초기 SLOTS
        var entries = ParseCsv(BuildEmptyCsv(Mathf.Max(1, anchor.SlotCount)));
        if (autoReserveSlotForActor > 0 && autoReserveView > 0 && entries.Length > 0)
        {
            entries[0].actor = autoReserveSlotForActor;
            entries[0].viewId = autoReserveView;
        }

        var hash = new Hashtable {
            { K_ACTIVE(vid), true },
            { K_ID(vid),     so.ID },
            { K_START(vid),  NOW },
            { K_SLOTS(vid),  ToCsv(entries) }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        return anchor;
    }

    // ===== 종료(주최자) =====
    public void StopEmote(EmoteAnchor anchor)
    {
        if (!anchor) return;
        int vid = anchor.photonView.ViewID;

        var room = PhotonNetwork.CurrentRoom;
        if (room != null)
        {
            room.SetCustomProperties(new Hashtable { { K_ACTIVE(vid), false } }); // ACTIVE=false → Reconcile에서 전원 로컬 종료
            room.SetCustomProperties(new Hashtable { { K_SLOTS(vid), BuildEmptyCsv(anchor.SlotCount) } });
        }

        PhotonNetwork.Destroy(anchor.gameObject);
    }

    // ===== 참여(슬롯 예약: CAS) =====
    public bool TryJoinSlot(EmoteAnchor anchor, int actorNumber, int viewId, out int pickedSlot)
    {
        pickedSlot = -1;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null || !anchor) return false;

        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(K_ACTIVE(vid), out var act) || !(bool)act) return false;
        if (!room.CustomProperties.TryGetValue(K_SLOTS(vid), out var slotsObj)) return false;

        string oldCsv = (string)slotsObj;
        var arr = ParseCsv(oldCsv);

        // 이미 들어가 있으면 거절
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor == actorNumber) return false;

        // 빈 슬롯
        int pick = -1;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor < 0 && arr[i].viewId < 0) { pick = i; break; }
        if (pick < 0) return false;

        arr[pick].actor = actorNumber;
        arr[pick].viewId = viewId;

        string newCsv = ToCsv(arr);
        var expected = new Hashtable { { K_SLOTS(vid), oldCsv } };
        var update   = new Hashtable { { K_SLOTS(vid), newCsv } };

        if (room.SetCustomProperties(update, expected))
        {
            pickedSlot = pick;
            return true; // 성공 → OnRoomPropertiesUpdate에서 로컬 적용
        }
        return false; // 경합 실패 → 호출 측에서 재시도 가능
    }

    // ===== 퇴장(슬롯 해제: CAS) =====
    public void LeaveSlot(EmoteAnchor anchor, int actorNumber, int viewId)
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null || !anchor) return;

        int vid = anchor.photonView.ViewID;
        if (!room.CustomProperties.TryGetValue(K_SLOTS(vid), out var slotsObj)) return;

        string oldCsv = (string)slotsObj;
        var arr = ParseCsv(oldCsv);

        bool changed = false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].actor == actorNumber && arr[i].viewId == viewId)
            {
                arr[i].actor = -1; arr[i].viewId = -1;
                changed = true; break;
            }
        }
        if (!changed) return;

        string newCsv = ToCsv(arr);
        var expected = new Hashtable { { K_SLOTS(vid), oldCsv } };
        var update   = new Hashtable { { K_SLOTS(vid), newCsv } };
        room.SetCustomProperties(update, expected); // 성공/실패 여부는 굳이 확인 불필요(경합 드뭄)
    }

    // ===== 정규화 시간(0~1) =====
    public static float GetNormalizedTime(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.EmoteSO) return 0f;
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return 0f;

        int vid = anchor.photonView.ViewID;
        if (!room.CustomProperties.TryGetValue(K_START(vid), out var startObj)) return 0f;

        double start = (double)startObj;
        double dt = NOW - start;
        double len = anchor.EmoteSO.Length;
        return (float)((dt % len) / len);
    }

    // ===== 프로퍼티 변경 감지 → 로컬 적용(Reconcile) =====
    public override void OnRoomPropertiesUpdate(Hashtable props)
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        for (int i = 0; i < _Anchors.Count; i++)
        {
            var a = _Anchors[i];
            if (!a || !a.photonView) continue;

            int vid = a.photonView.ViewID;
            bool changed = props.ContainsKey(K_SLOTS(vid)) || props.ContainsKey(K_ACTIVE(vid));
            if (!changed) continue;

            bool active = room.CustomProperties.TryGetValue(K_ACTIVE(vid), out var act) && (bool)act;
            string csv  = room.CustomProperties.TryGetValue(K_SLOTS(vid), out var so) ? (string)so : "";
            ReconcileAnchorState(a, active, csv);
        }
    }

    public override void OnJoinedRoom()
    {
        // 방에 들어오자마자 현재 존재하는 모든 앵커 상태를 스캔해 로컬 적용
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        for (int i = 0; i < _Anchors.Count; i++)
        {
            var a = _Anchors[i];
            if (!a || !a.photonView) continue;

            int vid = a.photonView.ViewID;
            bool active = room.CustomProperties.TryGetValue(K_ACTIVE(vid), out var act) && (bool)act;
            string csv  = room.CustomProperties.TryGetValue(K_SLOTS(vid), out var so) ? (string)so : "";

            ReconcileAnchorState(a, active, csv);
        }
    }

    void ReconcileAnchorState(EmoteAnchor anchor, bool active, string csv)
    {
        if (!anchor) return;

        int vid = anchor.photonView.ViewID;
        var entries = ParseCsv(csv);
        float norm  = GetNormalizedTime(anchor);

        if (!_present.TryGetValue(vid, out var old))
            old = _present[vid] = new Dictionary<int,int>();

        // 새 상태 구성(viewId -> slotIndex)
        var now = new Dictionary<int,int>();
        for (int i = 0; i < entries.Length; i++)
        {
            var e = entries[i];
            if (e.viewId > 0) now[e.viewId] = i;
        }

        // 추가된 viewId → 이모트 적용
        foreach (var kv in now)
        {
            if (old.ContainsKey(kv.Key)) continue;
            var pv = PhotonView.Find(kv.Key);
            var pe = pv ? pv.GetComponent<PlayerEmote>() : null;
            if (pe) pe.ApplyEmoteLocal(anchor, kv.Value, norm);
        }

        // 빠진 viewId → 로컬 강제 종료
        foreach (var kv in old)
        {
            if (now.ContainsKey(kv.Key)) continue;
            var pv = PhotonView.Find(kv.Key);
            var pe = pv ? pv.GetComponent<PlayerEmote>() : null;
            if (pe) pe.DoLeaveAndReturn();
        }

        _present[vid] = now;

        // ACTIVE=false면 남아있는 모든 참가자 로컬 종료
        if (!active && now.Count > 0)
        {
            foreach (var kv in now)
            {
                var pv = PhotonView.Find(kv.Key);
                var pe = pv ? pv.GetComponent<PlayerEmote>() : null;
                if (pe) pe.DoLeaveAndReturn();
            }
            _present[vid].Clear();
        }
    }
}
