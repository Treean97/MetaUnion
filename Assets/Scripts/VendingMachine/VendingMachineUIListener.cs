using UnityEngine;

public class VendingMachineUIListener : MonoBehaviour
{
    [SerializeField] VendingMachineUIManager _VendingMachineUIManager;

    void OnEnable()
    {
        GameEvents.OnRequestOpenVendingMachineUI += HandleSetActive;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenVendingMachineUI -= HandleSetActive;
    }

    void HandleSetActive()
    {
        _VendingMachineUIManager.gameObject.SetActive(true);
    }
}
