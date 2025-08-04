using UnityEngine;

public class InventoryFocusUIListener : MonoBehaviour
{
    [SerializeField] InventoryFocusUIManager _InventoryFocusUIManager;


    void OnEnable()
    {
        InventorySlot.OnPointerEnterInventorySlot += HandleFocus;
        InventorySlot.OnPointerExitInventorySlot += HandleDefocus;
    }

    void OnDisable()
    {
        InventorySlot.OnPointerEnterInventorySlot -= HandleFocus;
        InventorySlot.OnPointerExitInventorySlot -= HandleDefocus;
    }

    void HandleFocus(ItemDataSO itemDataSO)
    {
        if (itemDataSO == null)
        {
            return;
        }

        _InventoryFocusUIManager.Show(itemDataSO.ItemInfo);
        _InventoryFocusUIManager.gameObject.SetActive(true);
    }

    void HandleDefocus()
    {
        _InventoryFocusUIManager.gameObject.SetActive(false);
    }

}
