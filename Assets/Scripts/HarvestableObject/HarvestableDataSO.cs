using UnityEngine;

[CreateAssetMenu(menuName = "HarvestableObject/HarvestableObject Data")]
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
