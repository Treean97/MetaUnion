using Photon.Pun;
using UnityEngine;

public class Respawnable : MonoBehaviourPun, IRespawnable
{
    [Header("Respawn")]
    [SerializeField] private string _ResourcesRoot = "Respawnable"; // 예: Resources/Respawnable/<이름>.prefab
    [Header("Break FX")]
    [SerializeField] private float _BreakFxSeconds = 3.0f;
    private Transform _RespawnAnchor;
    private HarvestableObject _harvestable;
    private string _prefabPath;  // 최종 문자열 키: "Respawnable/<Prefab.name>"

    public string PrefabName     => _prefabPath;
    public float RespawnDelay => _harvestable && _harvestable.Data ? _harvestable.Data.RespawnSeconds : 30f;
    public Transform RespawnAnchor => _RespawnAnchor;
    public float BreakFxSeconds => _BreakFxSeconds;

    void Awake()
    {
        _harvestable = GetComponent<HarvestableObject>();
        if (!_RespawnAnchor) _RespawnAnchor = transform;
    }

    void Start()
    {
        var data = _harvestable ? _harvestable.Data : null;

        if (!data || data.Prefabs == null || data.Prefabs.Length == 0)
        {
            Debug.LogError($"[Respawnable] Data.Prefabs 비어있음: {name}");
            return;
        }

        // 마스터가 프리팹 결정 → 전파
        if (PhotonNetwork.IsMasterClient)
        {
            var picked = data.PickRandomRespawnPrefab();
            if (!picked)
            {
                Debug.LogError($"[Respawnable] PickRandomRespawnPrefab() 실패: {name}");
                return;
            }

            string prefabNameOnly = picked.name;
            string path = string.IsNullOrEmpty(_ResourcesRoot) ? prefabNameOnly : $"{_ResourcesRoot}/{prefabNameOnly}";

            // 존재 검증
            if (!Resources.Load<GameObject>(path))
            {
                Debug.LogError($"[Respawnable] Resources에서 프리팹을 찾을 수 없음: 'Assets/Resources/{path}.prefab'");
                return;
            }

            // 로컬 적용 + 동기화
            _prefabPath = path;
            photonView.RPC(nameof(RPC_SetPrefabPath), RpcTarget.Others, _prefabPath);
        }

        // 등록(로컬 매니저가 참조할 수 있게)
        RespawnManager._Inst?.Register(this);
    }

    [PunRPC]
    void RPC_SetPrefabPath(string path)
    {
        _prefabPath = path;
    }
    
    [PunRPC]
    void RPC_DespawnSceneObject()
    {
        Destroy(gameObject);
    }

}
