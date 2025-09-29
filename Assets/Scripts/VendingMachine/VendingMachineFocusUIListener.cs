using UnityEngine;

public class VendingMachineFocusUIListener : MonoBehaviour
{
    [SerializeField] VendingMachineFocusUIManager _VendingMachineFocusUIManager;


    // void OnEnable()
    // {
    //     VendingMachineSlot.OnPointerEnterVendingMachineSlot += HandleFocus;
    //     VendingMachineSlot.OnPointerExitVendingMachineSlot += HandleDefocus;
    // }

    // void OnDisable()
    // {
    //     VendingMachineSlot.OnPointerEnterVendingMachineSlot -= HandleFocus;
    //     VendingMachineSlot.OnPointerExitVendingMachineSlot -= HandleDefocus;
    // }

    void HandleFocus(ItemDataSO itemDataSO)
    {
        if (itemDataSO == null)
        {
            return;
        }

        _VendingMachineFocusUIManager.Show(itemDataSO);
        _VendingMachineFocusUIManager.gameObject.SetActive(true);
    }

    void HandleDefocus()
    {
        _VendingMachineFocusUIManager.gameObject.SetActive(false);
    }
}
