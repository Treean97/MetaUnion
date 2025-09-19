using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
public static ObjectPoolManager _Inst { get; private set; }

    [System.Serializable]
    public class Entry
    {
        public GameObject Prefab;
        public int PreInstantiate = 0;
        public Transform Root; // 비우면 자동으로 생성
    }

    [SerializeField] private List<Entry> _Entries = new List<Entry>();

    private readonly Dictionary<int, Queue<GameObject>> _Pools = new();
    private readonly Dictionary<int, Transform> _Roots = new();

    void Awake()
    {
        // 씬 내에서 유일한 인스턴스인지 확인
        if (_Inst != null && _Inst != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _Inst = this;

        // 미리 채우기
        foreach (var entry in _Entries)
        {
            if (!entry.Prefab) continue;
            int key = entry.Prefab.GetInstanceID();

            if (!_Pools.ContainsKey(key)) _Pools[key] = new Queue<GameObject>();
            if (!_Roots.ContainsKey(key))
            {
                var r = entry.Root ? entry.Root : new GameObject($"[Pool] {entry.Prefab.name}").transform;
                r.SetParent(transform, false);
                _Roots[key] = r;
            }

            for (int i = 0; i < entry.PreInstantiate; i++)
            {
                var go = Instantiate(entry.Prefab, _Roots[key]);
                go.SetActive(false);
                AttachPooled(go, entry.Prefab);
                _Pools[key].Enqueue(go);
            }
        }
    }

    private void AttachPooled(GameObject go, GameObject prefab)
    {
        if (!go.TryGetComponent<PooledObject>(out var objectPool))
            objectPool = go.AddComponent<PooledObject>();

        objectPool._Owner = this;
        objectPool._Prefab = prefab;
    }

    // 컴포넌트 타입으로 빌려오기
    public T Rent<T>(T prefab, Transform parent = null) where T : Component
    {
        var go = Rent(prefab.gameObject, parent);
        return go ? go.GetComponent<T>() : null;
    }

    // GameObject로 빌려오기
    public GameObject Rent(GameObject prefab, Transform parent = null)
    {
        if (!prefab) return null;

        int key = prefab.GetInstanceID();
        if (!_Pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<GameObject>();
            _Pools[key] = queue;
        }
        
        if (!_Roots.ContainsKey(key))
        {
            var r = new GameObject($"[Pool] {prefab.name}").transform;
            r.SetParent(transform, false);
            _Roots[key] = r;
        }

        GameObject inst = queue.Count > 0 ? queue.Dequeue() : Instantiate(prefab);
        if (!inst.TryGetComponent<PooledObject>(out var po))
            AttachPooled(inst, prefab);
        else
        {
            po._Owner = this;
            po._Prefab = prefab; // 최신 프리팹 키 갱신
        }

        // 부모 설정 및 활성화
        inst.transform.SetParent(parent, false);
        inst.SetActive(true);
        return inst;
    }

    public void Return(GameObject go)
    {
        if (!go) return;

        if (!go.TryGetComponent<PooledObject>(out var pooledObject) || pooledObject._Prefab == null)
        {
            Destroy(go); // 누가 만든 건지 모르면 파괴
            return;
        }

        int key = pooledObject._Prefab.GetInstanceID();
        if (!_Pools.TryGetValue(key, out var queue))
        {
            queue = new Queue<GameObject>();
            _Pools[key] = queue;
        }
        if (!_Roots.TryGetValue(key, out var root))
        {
            root = new GameObject($"[Pool] {pooledObject._Prefab.name}").transform;
            root.SetParent(transform, false);
            _Roots[key] = root;
        }

        go.SetActive(false);
        go.transform.SetParent(root, false);
        queue.Enqueue(go);
    }
}
