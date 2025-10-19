using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class EmoteManager : MonoBehaviourPunCallbacks
{
    public static EmoteManager _Inst { get; private set; }

    [Header("Catalog")]
    [SerializeField] private EmoteSO[] _EmoteSOs;
    public EmoteSO[] EmoteSOs => _EmoteSOs;

    public static double _NOW => PhotonNetwork.Time;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;
    }

    // 유틸
    struct SlotEntry
    {
        public int actor;
        public int viewId;
    }

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

    // 이모트 시작
    public EmoteAnchor StartEmote(EmoteSO so, Vector3 pos, Quaternion rot, PlayerEmote emoteOwner)
    {
        if (!PhotonNetwork.InRoom) { Debug.LogError("[Emote] Not in room"); return null; }
        if (!so || !so.EmoteAnchor) { Debug.LogError("[Emote] SO/Anchor 누락"); return null; }
        if (emoteOwner == null || !emoteOwner.photonView || !emoteOwner.photonView.IsMine)
        {
            Debug.LogError("[Emote] emoteOwner must be local & non-null");
            return null;
        }

        var go = PhotonNetwork.Instantiate(so.EmoteAnchor.name, pos, rot, 0, new object[] { so.ID });
        if (!go) { Debug.LogError("[Emote] Instantiate 실패"); return null; }

        var anchor = go.GetComponent<EmoteAnchor>();
        if (!anchor) { Debug.LogError("[Emote] EmoteAnchor 컴포넌트 없음"); return null; }
        anchor.Setup(so);

        int vid = anchor.photonView.ViewID;
        int slotCount = Mathf.Max(1, anchor.SlotCount);

        var arr = new string[slotCount];
        for (int i = 0; i < slotCount; i++) arr[i] = "-1:-1";

        bool ownerReserved = false;
        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView  = emoteOwner.TryGetViewID();
        if (myView > 0) { arr[0] = $"{myActor}:{myView}"; ownerReserved = true; }

        var hash = new Hashtable {
            { EmoteKeys._ACTIVE(vid), true },
            { EmoteKeys._EMOTE_ID(vid), so.ID },
            { EmoteKeys._START(vid), _NOW },
            { EmoteKeys._SLOTS(vid), string.Join(",", arr) }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

        if (ownerReserved && !emoteOwner.InEmote)
        {
            float t = GetNormalizedTime(anchor);
            emoteOwner.BeginJoin(anchor, 0, t);
        }
        else
        {
            Debug.LogWarning("[Emote] Owner slot not reserved or already in emote. Skipped auto-join.");
        }

        return anchor;    
    }

    // 이모트 종료
    public void StopEmote(EmoteAnchor anchor)
    {
        if (!anchor) return;
        int vid = anchor.photonView.ViewID;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(EmoteKeys._SLOTS(vid), out var slotsObj))
        {
            var arr = ParseCsv((string)slotsObj);
            for (int i = 0; i < arr.Length; i++)
            {
                int targetViewId = arr[i].viewId;
                if (targetViewId > 0)
                {
                    var pv = PhotonView.Find(targetViewId);
                    if (pv != null)
                        pv.RPC(nameof(PlayerEmote.RPC_ForceLeaveAndReturn), pv.Owner);
                }
            }
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { EmoteKeys._ACTIVE(vid), false } });
        PhotonNetwork.Destroy(anchor.gameObject);
    }

    // 참여
    public bool JoinEmote(EmoteAnchor anchor, PlayerEmote who, out int slotIndex)
    {
        slotIndex = -1;
        if (!anchor || !who) return false;

        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(EmoteKeys._ACTIVE(vid), out var act) || !(bool)act)
            return false;

        if (!room.CustomProperties.TryGetValue(EmoteKeys._SLOTS(vid), out var slotsObj))
            return false;

        string oldCsv = (string)slotsObj;
        var arr = ToEntries(oldCsv);

        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView = who.TryGetViewID();

        if (myView < 0) { Debug.LogWarning("[Emote] PlayerEmote에 PhotonView 없음"); return false; }

        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor == myActor) return false;

        int pick = -1;
        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor < 0 && arr[i].viewId < 0) { pick = i; break; }
        if (pick < 0) return false;

        arr[pick].actor = myActor;
        arr[pick].viewId = myView;

        string newCsv = FromEntries(arr);
        var expected = new Hashtable { { EmoteKeys._SLOTS(vid), oldCsv } };
        var update   = new Hashtable { { EmoteKeys._SLOTS(vid), newCsv } };

        if (room.SetCustomProperties(update, expected))
        {
            slotIndex = pick;
            return true;
        }
        return false;

        // local helpers
        static SlotEntry[] ToEntries(string csv) => ParseCsv(csv);
        static string FromEntries(SlotEntry[] a) => ToCsv(a);
    }

    // 나가기 
    public void LeaveEmote(EmoteAnchor anchor, PlayerEmote who)
    {
        if (!anchor || !who) return;

        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(EmoteKeys._SLOTS(vid), out var slotsObj)) return;

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
        var expected = new Hashtable { { EmoteKeys._SLOTS(vid), oldCsv } };
        var update   = new Hashtable { { EmoteKeys._SLOTS(vid), newCsv } };
        room.SetCustomProperties(update, expected);
    }

    // 정규화 시간
    public static float GetNormalizedTime(EmoteAnchor anchor)
    {
        if (!anchor || !anchor.EmoteSO) return 0f;
        int vid = anchor.photonView.ViewID;

        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(EmoteKeys._START(vid), out var startObj))
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
        if (anchor != null) StopEmote(anchor);
    }

    public void RequestStopEmote(EmoteAnchor anchor)
    {
        if (!anchor || !photonView) return;
        photonView.RPC(nameof(RPC_StopEmoteByMC), RpcTarget.MasterClient, anchor.photonView.ViewID);
    }
}
