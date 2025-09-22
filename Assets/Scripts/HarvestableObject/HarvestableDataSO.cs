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
    public GameObject Prefab;     // 재생성용
    public float RespawnSeconds = 30f;
}
