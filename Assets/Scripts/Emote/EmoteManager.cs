using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// EmoteSO 리스트를 보관하고,
/// - 이모트 시작/종료(앵커 생성/삭제)
/// - 이모트 참여/나가기(슬롯 예약/해제)
/// 및 정규화 시간 계산을 담당.
/// </summary>
public class EmoteManager : MonoBehaviourPunCallbacks
{
    public static EmoteManager _Inst { get; private set; }

    [Header("Catalog")]
    [SerializeField] private EmoteSO[] _EmoteSOs;
    public EmoteSO[] EmoteSOs => _EmoteSOs;
    static readonly List<EmoteAnchor> _Anchors = new();
    public static IReadOnlyList<EmoteAnchor> Anchors => _Anchors;

    public static void RegisterAnchor(EmoteAnchor a)
    {
        if (a && !_Anchors.Contains(a)) _Anchors.Add(a);
    }

    public static void UnregisterAnchor(EmoteAnchor a)
    {
        if (a) _Anchors.Remove(a);
    }

    // ----- Room Property Keys (앵커 ViewID 네임스페이스) -----
    static string _KEY_ACTIVE(int vid) => $"_EMOTE_{vid}_ACTIVE";
    static string _KEY_EMOTE_ID(int vid) => $"_EMOTE_{vid}_ID";
    static string _KEY_START(int vid) => $"_EMOTE_{vid}_START";
    // CSV: "actor:viewId,actor:viewId,...", 빈 슬롯 "-1:-1"
    static string _KEY_SLOTS(int vid) => $"_EMOTE_{vid}_SLOTS";

    public static double _NOW => PhotonNetwork.Time;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;
    }

    // ===== 유틸 =====
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

    public bool TryGetById(string id, out EmoteSO so)
    {
        so = null;
        if (string.IsNullOrEmpty(id) || _EmoteSOs == null) return false;
        for (int i = 0; i < _EmoteSOs.Length; i++)
        {
            if (_EmoteSOs[i] && _EmoteSOs[i].ID == id) { so = _EmoteSOs[i]; return true; }
        }
        return false;
    }

    // ===== 이모트 시작(앵커 생성 + 룸 프로퍼티 세팅) =====
    public EmoteAnchor StartEmote(EmoteSO so, Vector3 pos, Quaternion rot, PlayerEmote autoJoinWho = null)
    {
        if (!so || !so.EmoteAnchor) { Debug.LogError("[Emote] SO/Anchor 누락"); return null; }

        // Photon prefab 이름 = EmoteSO.EmoteAnchor.name (Resources 또는 커스텀 풀 등록 필요)
        var go = PhotonNetwork.Instantiate(so.EmoteAnchor.name, pos, rot, 0, new object[] { so.ID });
        if (!go) { Debug.LogError("[Emote] Instantiate 실패"); return null; }

        var anchor = go.GetComponent<EmoteAnchor>();
        if (!anchor) { Debug.LogError("[Emote] EmoteAnchor 컴포넌트 없음"); return null; }
        anchor.Setup(so);

        int vid = anchor.photonView.ViewID;
        int slotCount = Mathf.Max(1, anchor.SlotCount);

        // 기본 CSV
        var arr = new string[slotCount];
        for (int i = 0; i < slotCount; i++) arr[i] = "-1:-1";

        // 생성자 자동 예약(0번 슬롯)
        if (autoJoinWho != null)
        {
            int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
            int myView = autoJoinWho.TryGetViewID(); // 없으면: autoJoinWho.GetComponent<PhotonView>()?.ViewID ?? -1;
            if (myView > 0) arr[0] = $"{myActor}:{myView}";
        }

        var hash = new Hashtable {
        { _KEY_ACTIVE(vid), true },
        { _KEY_EMOTE_ID(vid), so.ID },
        { _KEY_START(vid), _NOW },
        { _KEY_SLOTS(vid), string.Join(",", arr) }   // ← 이미 예약된 상태로 기록
    };
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        return anchor;
    }

    // ===== 이모트 종료(슬롯 참가자들에게 강제 종료 지시 + 앵커 삭제) =====
    public void StopEmote(EmoteAnchor anchor)
    {
        if (!anchor) return;
        int vid = anchor.photonView.ViewID;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(_KEY_SLOTS(vid), out var slotsObj))
        {
            var arr = ParseCsv((string)slotsObj);
            for (int i = 0; i < arr.Length; i++)
            {
                int targetViewId = arr[i].viewId;
                if (targetViewId > 0)
                {
                    var pv = PhotonView.Find(targetViewId);
                    if (pv != null)
                        pv.RPC(nameof(PlayerEmote.RPC_ForceLeaveAndReturn), pv.Owner); // 소유자에게 지시
                }
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { _KEY_ACTIVE(vid), false } });
        PhotonNetwork.Destroy(anchor.gameObject);
    }

    // ===== 참여(슬롯 예약: 선착순 CAS) =====
    public bool JoinEmote(EmoteAnchor anchor, PlayerEmote who, out int slotIndex)
    {
        slotIndex = -1;
        if (!anchor || !who) return false;

        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(_KEY_ACTIVE(vid), out var act) || !(bool)act)
            return false;

        if (!room.CustomProperties.TryGetValue(_KEY_SLOTS(vid), out var slotsObj))
            return false;

        string oldCsv = (string)slotsObj;
        var arr = ParseCsv(oldCsv);

        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView = who.TryGetViewID();

        if (myView < 0) { Debug.LogWarning("[Emote] PlayerEmote에 PhotonView 없음"); return false; }

        // 이미 들어가 있으면 거절
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor == myActor) return false;

        // 빈 슬롯 찾기
        int pick = -1;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor < 0 && arr[i].viewId < 0) { pick = i; break; }
        if (pick < 0) return false; // 만석

        arr[pick].actor = myActor;
        arr[pick].viewId = myView;

        string newCsv = ToCsv(arr);
        var expected = new Hashtable { { _KEY_SLOTS(vid), oldCsv } };
        var update = new Hashtable { { _KEY_SLOTS(vid), newCsv } };

        if (room.SetCustomProperties(update, expected))
        {
            slotIndex = pick;
            return true;
        }
        return false;
    }

    // ===== 나가기(내 엔트리만 해제) =====
    public void LeaveEmote(EmoteAnchor anchor, PlayerEmote who)
    {
        if (!anchor || !who) return;

        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(_KEY_SLOTS(vid), out var slotsObj)) return;

        string oldCsv = (string)slotsObj;
        var arr = ParseCsv(oldCsv);

        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView = who.TryGetViewID();

        bool changed = false;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].actor == myActor && arr[i].viewId == myView)
            {
                arr[i].actor = -1; arr[i].viewId = -1;
                changed = true; break;
            }
        }
        if (!changed) return;

        string newCsv = ToCsv(arr);
        var expected = new Hashtable { { _KEY_SLOTS(vid), oldCsv } };
        var update = new Hashtable { { _KEY_SLOTS(vid), newCsv } };
        room.SetCustomProperties(update, expected);
    }

    // ===== 정규화 시간(0~1) =====
    public static float GetNormalizedTime(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.EmoteSO) return 0f;
        int vid = anchor.photonView.ViewID;

        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(_KEY_START(vid), out var startObj))
            return 0f;

        double start = (double)startObj;
        double dt = _NOW - start;
        double len = anchor.EmoteSO.Length;
        return (float)((dt % len) / len);
    }

    [PunRPC]
    void RPC_StopEmoteByMC(int anchorViewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var pv = PhotonView.Find(anchorViewId);
        var anchor = pv ? pv.GetComponent<EmoteAnchor>() : null;
        if (anchor != null)
            StopEmote(anchor); // 네가 이미 가지고 있는 StopEmote 재사용
    }

    // 외부에서 호출: 아무 클라나 이걸 부르면 마스터가 실제 파괴를 수행
    public void RequestStopEmote(EmoteAnchor anchor)
    {
        if (!anchor || !photonView) return;
        photonView.RPC(nameof(RPC_StopEmoteByMC), RpcTarget.MasterClient, anchor.photonView.ViewID);
    }

    public void SyncEmotesForNewcomer()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (room == null) return;

        foreach (var anchor in Anchors) // ← 전역 검색 제거
        {
            int vid = anchor.photonView.ViewID;

            if (!room.CustomProperties.TryGetValue(_KEY_ACTIVE(vid), out var act) || !(bool)act)
                continue;

            if (!room.CustomProperties.TryGetValue(_KEY_SLOTS(vid), out var slotsObj))
                continue;

            var entries = ParseCsv((string)slotsObj);
            float norm = GetNormalizedTime(anchor);

            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                if (e.viewId <= 0) continue;

                var pv = PhotonView.Find(e.viewId);
                if (!pv) continue;

                var pe = pv.GetComponent<PlayerEmote>();
                if (!pe) continue;

                pe.ApplyEmoteLocal(anchor, i, norm); // 로컬 적용(신규 입장자 전용)
            }
        }
    }



}
