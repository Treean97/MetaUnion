using UnityEngine;
using UnityEngine.UI;

public class SlotMachineSlotManager : MonoBehaviour
{
    private int _ID;
    [SerializeField] private Image _Icon;

    public void SetSlot(SlotMachineSlotDataSO slotData)
    {
        _ID = slotData.ID;
        _Icon.sprite = slotData.Icon;        
    }
}
