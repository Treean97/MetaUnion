using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager _Inst{ get; private set; }

// 오브젝트별 등록 정보
    private class Entry
    {
        public int Id;
        public string PrefabName;
        public Vector3 Pos;
        public Quaternion Rot;
        public float Delay;

        public PhotonView PV;
        public IRespawnable Respawnable;
        public IDestructible Destructible;
    }

    private int _Seq = 1;
    private readonly Dictionary<int, Entry> _entries = new Dictionary<int, Entry>();

    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(gameObject); return; }
        _Inst = this;
    }

    /// <summary>리스폰 가능한 오브젝트가 스스로 호출(씬 배치/스폰 직후)</summary>
    public int Register(IRespawnable resp)
    {
        if (resp == null) return -1;

        var go = (resp as Component).gameObject;
        var pv = go.GetComponent<PhotonView>();
        var d  = go.GetComponent<IDestructible>();

        // 이미 같은 앵커/프리팹으로 등록된 항목이 있으면 재바인딩(중복 방지)
        foreach (var kv in _entries)
        {
            var e = kv.Value;
            if (e.PrefabName == resp.PrefabName &&
                Vector3.SqrMagnitude(e.Pos - resp.RespawnAnchor.position) < 0.0001f &&
                Quaternion.Dot(e.Rot, resp.RespawnAnchor.rotation) > 0.9999f)
            {
                // 재바인딩
                Bind(e, resp, pv, d);
                return e.Id;
            }
        }

        // 신규 등록
        var id = _Seq++;
        var entry = new Entry
        {
            Id = id,
            PrefabName = resp.PrefabName,
            Pos = resp.RespawnAnchor.position,
            Rot = resp.RespawnAnchor.rotation,
            Delay = Mathf.Max(0f, resp.RespawnDelay),
        };

        _entries[id] = entry;
        Bind(entry, resp, pv, d);

        return id;
    }

    private void Bind(Entry e, IRespawnable resp, PhotonView pv, IDestructible d)
    {
        // 기존 구독 해제
        if (e.Destructible != null)
            e.Destructible.OnDestroyed -= () => HandleDestroyed(e.Id);

        e.Respawnable = resp;
        e.PV = pv;
        e.Destructible = d;

        if (e.Destructible != null)
            e.Destructible.OnDestroyed += () => HandleDestroyed(e.Id);
    }

    private void HandleDestroyed(int id)
    {
        // 먼저 구독 해제(중복 방지)
        if (_entries.TryGetValue(id, out var e))
        {
            if (e.Destructible != null)
                e.Destructible.OnDestroyed -= () => HandleDestroyed(id);
        }

        // 마스터만 파괴/리스폰 스케줄
        if (!PhotonNetwork.IsMasterClient) return;
        if (!_entries.TryGetValue(id, out e)) return;

        // 파괴
        if (e.PV != null)
        {
            if (e.PV.InstantiationId > 0)
            {
                PhotonNetwork.Destroy(e.PV);
            }
            else
            {
                const string DespawnSceneObject = "RPC_DespawnSceneObject";
                // 씬 배치 PV → 전 클라 동일하게 로컬 파괴
                e.PV.RPC(DespawnSceneObject, RpcTarget.AllBuffered);
            }
        }

        // 바인딩 끊기
        e.PV = null;
        e.Respawnable = null;
        e.Destructible = null;

        // 리스폰 예약
        StartCoroutine(RespawnRoutine(id, e.PrefabName, e.Pos, e.Rot, e.Delay));
    }

    private IEnumerator RespawnRoutine(int id, string prefabName, Vector3 pos, Quaternion rot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!PhotonNetwork.IsMasterClient) yield break;
        if (string.IsNullOrEmpty(prefabName)) yield break;

        var go = PhotonNetwork.Instantiate(prefabName, pos, rot);

        // 새 개체가 Start에서 Register(this)를 호출해 다시 Entry에 바인딩됨.
        // (혹시 자동 등록을 쓰지 않는다면, 여기서 직접 Register 호출해도 됨)
        var resp = go.GetComponent<IRespawnable>();
        if (resp != null)
        {
            // 재등록을 즉시 보장하고 싶다면:
            Register(resp);
        }
    }

}
