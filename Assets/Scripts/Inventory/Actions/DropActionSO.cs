using UnityEngine;

[CreateAssetMenu(fileName = "DropAction", menuName = "Item/Actions/DropAction")]
public class DropActionSO : ScriptableObject, IInventoryAction
{
    [SerializeField] private string _Label = "Drop";

    public string Label => _Label;

    public void Execute(InventoryItem inventoryItem, GameObject user)
    {
        Debug.Log(inventoryItem.ID);
        
        bool success =
        GameEvents.RaiseRequestItemDrop(inventoryItem.ID, inventoryItem.Amount, user);       

        if (!success)
        {
            GameEvents.RaiseShowWarning("Can't Drop");
        }
    }
}
