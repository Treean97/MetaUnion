# MetaUnion 샘플 코드

## 0. 개요
- 프로젝트: 유니티 기반 메타버스 멀티 게임
- 사용 기술: Unity, C#, Photon, Playfab, GitHub Action, GitHub

## 1. 클라우드 데이터
### 코드 목적 : 플레이어의 데이터를 Playfab 클라우드에 저장 및 불러오기 구현
### 코드 발췌 :
```csharp
    #region 클라우드 저장  
    public void LoadAllCloud()
    {
        if (!CloudReady) { Debug.LogWarning("Not logged in"); return; }

        var keys = new List<string>(_CloudSections.Keys);
        var req = new GetUserDataRequest { Keys = (keys.Count > 0 ? keys : null) };

        PlayFabClientAPI.GetUserData(req, r =>
        {
            _CloudLoaded.Clear();
            if (r.Data != null)
            {             
                foreach (var kv in r.Data)
                {
                    _CloudLoaded[kv.Key] = kv.Value?.Value;   
                }                    
            }

            foreach (var kv in _CloudSections)
            {
                if (_CloudLoaded.TryGetValue(
                    kv.Key, out var json) && !string.IsNullOrEmpty(json))
                {
                    kv.Value.ApplyJson(json);
                }
            }              
                    
            Debug.Log("LoadAllCloud Success");
        }, e => Debug.LogError($"LoadAllCloud fail: {e.GenerateErrorReport()}"));
    }


    // 단일 키 로드
    public void LoadCloud(string key, Action<string> ok = null, Action<string> err = null)
    {
        if (!CloudReady) { err?.Invoke("Not logged in"); return; }
        var req = new GetUserDataRequest { Keys = new List<string> { key } };
        PlayFabClientAPI.GetUserData(req, r =>
        {
            string v = null;
            if (r.Data != null && r.Data.TryGetValue(key, out var data))
            {
                v = data.Value;  
            } 
            _CloudLoaded[key] = v;

            // 등록된 섹션에 즉시 적용
            if (_CloudSections.TryGetValue(key, out var sec) && !string.IsNullOrEmpty(v))
            {
                sec.ApplyJson(v);
            }                
            ok?.Invoke(v);
        },
        e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
    }

    // 저장
    public void SaveCloud(string key, string json, Action ok = null, Action<string> err = null)
    {
        if (!CloudReady) { err?.Invoke("Not logged in"); return; }

        if (string.IsNullOrEmpty(json))
        {
            var delReq = new UpdateUserDataRequest { KeysToRemove = new List<string> { key } };
            PlayFabClientAPI.UpdateUserData(delReq, _ => { _CloudLoaded.Remove(key); ok?.Invoke(); },
                e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
            return;
        }

        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { key, json } },
            Permission = UserDataPermission.Private
        };
        PlayFabClientAPI.UpdateUserData(req, _ => { _CloudLoaded[key] = json; ok?.Invoke(); },
            e => { Debug.LogError(e.GenerateErrorReport()); err?.Invoke(e.GenerateErrorReport()); });
    }

    // 등록된 섹션을 이용한 저장
    public void SaveCloudSection(string key, Action ok = null, Action<string> err = null)
    {
        if (!_CloudSections.TryGetValue(key, out var sec))
        {
            err?.Invoke($"No cloud section: {key}");
            return;
        }
        var json = sec.CaptureJson();
        SaveCloud(key, json, ok, err);
    }
    #endregion
```

## 2. 메모리 최적화
### 코드 목적 : 오브젝트 풀 사용으로 생성과 파괴를 줄여 GC 완화
### 코드 발췌 :
```csharp
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

```

## 3. 데이터 분리 및 확장성 증진
### 코드 목적 : ScriptableObject 사용으로 유지보수 및 확장성 증진
### 코드 발췌 :
```csharp
    public class HarvestableDataSO : ScriptableObject
    {
        [Header("Stat")]
        [SerializeField] DamageTool _AvailableTool;
        public DamageTool AvailableTool => _AvailableTool;

        [SerializeField] float _Durability;
        public float Durability => _Durability;


        [Header("Drop")]
        [SerializeField] DropItemTableSO _DropTable;
        public DropItemTableSO DropTable => _DropTable;


        [Header("Respawn")]
        public GameObject[] Prefabs; // 재생성용
        public float RespawnSeconds = 30f;

        public GameObject PickRandomRespawnPrefab()
        {
            if (Prefabs == null || Prefabs.Length == 0) return null;
            int i = Random.Range(0, Prefabs.Length);
            return Prefabs[i];
        }
    }
```

## 4. 멀티 구현 및 동기화
### 코드 목적 : 데미지 처리를 마스터에 위임해 권한 일관성을 유지하고, 결과를 RPC로 전파
### 코드 발췌 :
```csharp
    public void Damaged(DamageInfo info)
    {
        if (_Dead || !_Data) return;

        // 도구 체크
        if ((_Data.AvailableTool & info.tool) == 0) return;

        // 사운드 효과
        AudioManager._Inst.PlayLocalByKey(_Hitkey);

        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(info.damage);
        }
        else
        {
            // 권한 일관성 위해 마스터에 위임
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, info.damage);
        }
    }

    void ApplyDamage(float dmg)
    {
        if (_Dead) return;

        // 데미지 팝업
        photonView.RPC(nameof(RPC_ShowPopup), RpcTarget.All, transform.position, (int)dmg);

        _Hp -= dmg;
        if (_Hp <= 0f)
        {
            _Dead = true;
            photonView.RPC(nameof(RPC_BroadcastDestroyed), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(dmg);
    }

    [PunRPC]
    void RPC_BroadcastDestroyed()
    {
        OnDestroyed?.Invoke(); 
    }

    [PunRPC]
    void RPC_ShowPopup(Vector3 pos, int amount)
    {
        DamagePopManager._Inst?.Show(pos, amount);
    }
```
