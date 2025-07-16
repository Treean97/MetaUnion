using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _Amount;
    private ItemDataSO _ItemDataSO;

    public void UpdateSlot(int id, int amount)
    {
        Debug.Log($"Update Slot {id}, {amount}");
        // 아이템 정보 저장
        ItemManager._Inst.ItemDataPoolSO.TryGetItem(id, out _ItemDataSO);
        _Icon.sprite = _ItemDataSO.Icon;
        _Icon.enabled = true;
        _Amount.text = amount.ToString();        
        _Amount.enabled = true;
    }

    public void ClearSlot()
    {
        _ItemDataSO = null;
        _Icon.enabled = false;
        _Amount.text = "";
        _Amount.enabled = false;
    }

    
}
