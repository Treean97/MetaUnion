using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _Icon;
    private ItemDataSO _ItemDataSO;

    public void UpdateSlot(ItemDataSO itemData)
    {
        _ItemDataSO = itemData;
        _Icon.sprite = itemData.Icon;
        _Icon.enabled = true;
    }

    public void ClearSlot()
    {
        _ItemDataSO = null;
        _Icon.enabled = false;
    }

    
}
