using UnityEngine;

[CreateAssetMenu(menuName = "SlotDataSO")]
public class SlotDataSO : ScriptableObject
{
    [SerializeField] int _ID;
    public int ID => _ID;
    [SerializeField] Sprite _Icon;
    public Sprite Icon => _Icon;
}
