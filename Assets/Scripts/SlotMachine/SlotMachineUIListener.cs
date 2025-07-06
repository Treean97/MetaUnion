using UnityEngine;

public class SlotMachineUIListener : MonoBehaviour
{
    [SerializeField] private SlotMachineUIManager _SlotMachineUI;

    void Awake()
    {
        GameEvents.OnRequestOpenSlotMachineUI += HandleSetActive;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestOpenSlotMachineUI -= HandleSetActive;
    }

    void HandleSetActive()
    {
        _SlotMachineUI.gameObject.SetActive(true);
    }
    
}
