using UnityEngine;

[CreateAssetMenu(menuName = "HarvestableObject/HarvestableObject Data")]
public class HarvestableDataSO : ScriptableObject
{
    [SerializeField] float _Durability;
    public float Durability => _Durability;

    [SerializeField] DropItemTableSO _DropTable;
    public DropItemTableSO DropTable => _DropTable;
}
