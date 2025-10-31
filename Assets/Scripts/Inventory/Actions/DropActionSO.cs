using UnityEngine;

[CreateAssetMenu(fileName = "DropAction", menuName = "Item/Actions/DropAction")]
public class DropActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "Drop";

    public string Label => _Label;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {        
        int amountToDrop = 1; 
        if (inventoryItem.Amount < amountToDrop)
        {
            GameEvents.RaiseShowWarning("Not enough item");
            return;
        }

        bool ok = GameEvents.RaiseRequestItemDrop(inventoryItem.ID, amountToDrop, user);
        if (!ok) GameEvents.RaiseShowWarning("Can't Drop");
    }
}
