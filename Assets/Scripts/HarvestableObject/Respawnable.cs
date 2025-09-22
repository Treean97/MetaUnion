using Photon.Pun;
using UnityEngine;

public class Respawnable : MonoBehaviour, IRespawnable
{
    [Header("Respawn")]
    [SerializeField] private string _ResourcesRoot = "Respawnable"; // 예: Resources/Respawnable/<이름>.prefab
    private Transform _RespawnAnchor;
    private HarvestableObject _harvestable;
    private string _prefabPath;  // 최종 문자열 키: "Respawnable/<Prefab.name>"

    public string PrefabName     => _prefabPath;
    public float  RespawnDelay   => _harvestable && _harvestable.Data ? _harvestable.Data.RespawnSeconds : 30f;
    public Transform RespawnAnchor => _RespawnAnchor;

    void Awake()
    {
        _harvestable = GetComponent<HarvestableObject>();
        if (!_RespawnAnchor) _RespawnAnchor = transform;
    }

    void Start()
    {
        var data = _harvestable.Data;
        if (!data || !data.Prefab)
        {
            Debug.LogError($"[Respawnable] Data.Prefab 비어있음: {name}");
            return;
        }

        // 1) 프리팹 이름으로 Resources 경로 구성
        string prefabNameOnly = data.Prefab.name;
        _prefabPath = string.IsNullOrEmpty(_ResourcesRoot) ? prefabNameOnly : $"{_ResourcesRoot}/{prefabNameOnly}";

        // 2) 존재 검증(에디터·런타임 공통 가능)
        var res = Resources.Load<GameObject>(_prefabPath);
        if (!res)
        {
            Debug.LogError($"[Respawnable] Resources에서 네트워크 프리팹을 찾을 수 없음: '{_prefabPath}'. " +
                           $"경로가 'Assets/Resources/{_prefabPath}.prefab' 인지 확인.");
            return;
        }

        // 3) 등록
        RespawnManager._Inst?.Register(this);
    }

    [PunRPC]
    void RPC_DespawnSceneObject()
    {
        if (this && gameObject) Destroy(gameObject);
    }
}
