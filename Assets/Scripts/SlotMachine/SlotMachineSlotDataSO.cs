using UnityEngine;

[CreateAssetMenu(menuName = "SlotMachineSlotDataSO")]
public class SlotMachineSlotDataSO : ScriptableObject
{
    [SerializeField] int _ID;
    public int ID => _ID;
    [SerializeField] Sprite _Icon;
    public Sprite Icon => _Icon;
}
