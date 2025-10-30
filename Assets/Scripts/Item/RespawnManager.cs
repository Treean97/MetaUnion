using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager _Inst { get; private set; }
    [SerializeField] private float _GlobalBreakFxSeconds = 2.0f; // 전역 파괴 연출 대기
    public static float GlobalBreakFxSeconds => _Inst ? _Inst._GlobalBreakFxSeconds : 2.0f;

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
        public System.Action DestroyedHandler;
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
        if (e.Destructible != null && e.DestroyedHandler != null)
            e.Destructible.OnDestroyed -= e.DestroyedHandler;

        e.Respawnable = resp;
        e.PV = pv;
        e.Destructible = d;

        if (e.Destructible != null)
        {
            e.DestroyedHandler = () => HandleDestroyed(e.Id);
            e.Destructible.OnDestroyed += e.DestroyedHandler;
        }
    }

    private void HandleDestroyed(int id)
    {
        // 구독 해제
        if (_entries.TryGetValue(id, out var e))
        {
            if (e.Destructible != null && e.DestroyedHandler != null)
                e.Destructible.OnDestroyed -= e.DestroyedHandler;
        }

        if (!PhotonNetwork.IsMasterClient) return;
        if (!_entries.TryGetValue(id, out var entry)) return;

        // 기존 즉시 파괴 → 코루틴으로 교체
        StartCoroutine(DestroyThenRespawnRoutine(entry));
    }


    private IEnumerator DestroyThenRespawnRoutine(Entry e)
    {
        // 파괴 연출
        if (_GlobalBreakFxSeconds > 0f)
            yield return new WaitForSeconds(_GlobalBreakFxSeconds);

        // 원본 파괴
        if (e.PV != null)
        {
            if (e.PV.InstantiationId > 0)
            {
                PhotonNetwork.Destroy(e.PV);
            }
            else
            {
                const string DespawnSceneObject = "RPC_DespawnSceneObject";
                e.PV.RPC(DespawnSceneObject, RpcTarget.AllBuffered);
            }
        }

        // 바인딩 끊기
        e.PV = null;
        e.Respawnable = null;
        e.Destructible = null;
        e.DestroyedHandler = null;

        // 리스폰 대기
        if (e.Delay > 0f)
            yield return new WaitForSeconds(e.Delay);

        // 리스폰
        if (!string.IsNullOrEmpty(e.PrefabName))
        {
            var go = PhotonNetwork.Instantiate(e.PrefabName, e.Pos, e.Rot);
            var resp = go.GetComponent<IRespawnable>();
            if (resp != null) Register(resp); // 즉시 재바인딩(선택)
        }
    }


}
