using UnityEngine;

[CreateAssetMenu(fileName = "UseAction", menuName = "Item/Actions/UseAction")]
public class UsePotionActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "Use";
    [SerializeField] private BuffDataSO _BuffData;

    public string Label => _Label;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {
        GameEvents.RaiseRequestApplyBuff(_BuffData, user);
        GameEvents.RaiseRequestItemSpend(inventoryItem.ID, 1);
    }

}
