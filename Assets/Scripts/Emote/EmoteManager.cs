using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class EmoteManager : MonoBehaviourPunCallbacks
{
    public static EmoteManager _Inst { get; private set; }
    [SerializeField] EmoteSO[] _EmoteSOs;
    public EmoteSO[] EmoteSOs => _EmoteSOs;

    // Room Property Keys (앵커 ViewID별 네임스페이스)
    static string _KEY_ACTIVE(int vid) => $"_EMOTE_{vid}_ACTIVE";
    static string _KEY_EMOTE_ID(int vid) => $"_EMOTE_{vid}_ID";
    static string _KEY_START(int vid) => $"_EMOTE_{vid}_START";
    // CSV: "actor:viewId,actor:viewId,..."  빈 슬롯은 "-1:-1"
    static string _KEY_SLOTS(int vid) => $"_EMOTE_{vid}_SLOTS_OWNER";

    public static double _NOW => PhotonNetwork.Time;

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;
    }

    static string BuildEmptyCsv(int count)
    {
        if (count <= 0) return "";
        var arr = new string[count];
        for (int i = 0; i < count; i++) arr[i] = "-1:-1";
        return string.Join(",", arr);
    }

    struct SlotEntry { public int actor; public int viewId; }
    static SlotEntry[] ParseCsv(string csv)
    {
        if (string.IsNullOrEmpty(csv)) return new SlotEntry[0];
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

    // === 시작 ===
    public EmoteAnchor StartEmote(EmoteSO so, Vector3 pos, Quaternion rot)
    {
        if (!so || !so.EmoteAnchor) { Debug.LogError("[Emote] SO/Anchor 누락"); return null; }

        // emoteID를 instantiationData로 함께 전송
        var go = PhotonNetwork.Instantiate(so.EmoteAnchor.name, pos, rot, 0, new object[] { so.ID });
        var anchor = go.GetComponent<EmoteAnchor>();
        anchor.Setup(so);

        int vid = anchor.photonView.ViewID;
        var hash = new Hashtable {
        { _KEY_ACTIVE(vid), true },
        { _KEY_EMOTE_ID(vid), so.ID },  // 디버그/복구용으로도 저장
        { _KEY_START(vid), _NOW },
        { _KEY_SLOTS(vid), BuildEmptyCsv(Mathf.Max(1, anchor.SlotCount)) }
        };

        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        return anchor;
    }

    // === 선착순 슬롯 예약(CAS): "빈 슬롯(-1:-1)을 내(actor:viewId)로" ===
    public bool TryReserveNextSlot(EmoteAnchor anchor, out int slotIndex, PlayerEmote who)
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

        // 중복 입장 방지: 이미 내가 들어가 있으면 거절
        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView = who.GetComponent<PhotonView>()?.ViewID ?? -1;
        if (myView < 0) { Debug.LogWarning("[Emote] PlayerEmote에 PhotonView 없음"); return false; }

        for (int i = 0; i < arr.Length; i++)
            if (arr[i].actor == myActor) return false;

        // 왼쪽부터 빈 슬롯 찾기
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
        return false; // 경합 실패
    }

    // === 자발적 이탈: 내 엔트리만 비움 ===
    public void FreeMySlotIfPossible(EmoteAnchor anchor, PlayerEmote who)
    {
        if (!anchor || !who) return;
        var room = PhotonNetwork.CurrentRoom;
        int vid = anchor.photonView.ViewID;

        if (!room.CustomProperties.TryGetValue(_KEY_SLOTS(vid), out var slotsObj)) return;

        string oldCsv = (string)slotsObj;
        var arr = ParseCsv(oldCsv);

        int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
        int myView = who.GetComponent<PhotonView>()?.ViewID ?? -1;

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
        room.SetCustomProperties(update, expected); // 실패해도 치명적 X
    }

    // === 정규화 시간 ===
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

    // === 외부 API ===
    public void RequestJoinSequential(EmoteAnchor anchor, PlayerEmote playerEmote)
    {
        if (!anchor || !playerEmote) return;
        if (!TryReserveNextSlot(anchor, out var slot, playerEmote))
        {
            playerEmote.OnJoinRejected_Full();
            return;
        }
        playerEmote.BeginJoin(anchor, slot);
    }

    public void RequestLeave(EmoteAnchor anchor, PlayerEmote playerEmote)
    {
        if (!anchor || !playerEmote) return;
        FreeMySlotIfPossible(anchor, playerEmote);
        playerEmote.DoLeaveAndReturn();
    }

    // === Stop: 룸 프로퍼티에서 참가자 목록 읽고 각자에게 RPC로 강제 퇴장 지시 ===
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
                    {
                        // 소유자에게만 실행 지시
                        pv.RPC(nameof(PlayerEmote.RPC_ForceLeaveAndReturn), pv.Owner);
                    }
                }
            }
        }

        // 비활성화 플래그 → 앵커 삭제
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { _KEY_ACTIVE(vid), false } });
        PhotonNetwork.Destroy(anchor.gameObject);
    }

    public bool TryGetById(string id, out EmoteSO so)
    {
        so = null;
        if (string.IsNullOrEmpty(id) || _EmoteSOs == null) return false;
        for (int i = 0; i < _EmoteSOs.Length; i++)
            if (_EmoteSOs[i] && _EmoteSOs[i].ID == id) { so = _EmoteSOs[i]; return true; }
        return false;
    }
}
