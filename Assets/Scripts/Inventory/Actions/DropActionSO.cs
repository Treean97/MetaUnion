using UnityEngine;

[CreateAssetMenu(fileName = "DropAction", menuName = "Item/Actions/DropAction")]
public class DropActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "Drop";

    public string Label => _Label;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {
        ItemManager._Inst.ItemDataPoolSO.TryGetItem(inventoryItem.ID, out var itemData);
        UIRouter._Inst.Open<ISetAmountUI>(ui => ui.SetUI(QuantityMode.Drop, itemData));
        // int amountToDrop = 1; 
        // if (inventoryItem.Amount < amountToDrop)
        // {
        //     GameEvents.RaiseShowWarning("Not enough item");
        //     return;
        // }

        // bool ok = GameEvents.RaiseRequestItemDrop(inventoryItem.ID, amountToDrop, user);
        // if (!ok) GameEvents.RaiseShowWarning("Can't Drop");
    }
}
