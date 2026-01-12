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
        // 싱글톤
        if (_Inst != null && _Inst != this)
        {
            Destroy(this);
            return;
        }
        
        _Inst = this;

        // 객체 채우기
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
        if (!prefab) return null;

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
            
        // 큐 안에서 Destroy된 애들 걸러내기
        GameObject inst = null;
        int removedBroken = 0;

        while (queue.Count > 0 && !inst)
        {
            var candidate = queue.Dequeue();
            if (candidate)           // 아직 살아 있는 오브젝트
            {
                inst = candidate;
            }
            else                     // Destroy된 오브젝트
            {
                removedBroken++;
            }
        }

        if (removedBroken > 0)
        {
            Debug.LogWarning(
                $"[Pool] {prefab.name} 풀에서 Destroy된 인스턴스 {removedBroken}개를 제거했습니다."
            );
        }

        // 큐에 없으면 새로 생성
        if (!inst)
        {
            inst = Instantiate(prefab);
            if (!inst)
            {
                Debug.LogError($"[Pool] {prefab.name} Instantiate 실패");
                return null;
            }
        }    
        
        if (!inst.TryGetComponent<PooledObject>(out var po))
            AttachPooled(inst, prefab);
        else
        {
            po._Owner = this;
            po._Prefab = prefab;
        }

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
