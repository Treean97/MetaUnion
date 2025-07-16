using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private Transform _InventoryUIPanel;
    private InventorySlot[] _Slots;    
    private int _MaxSlotCount;

    
    void Awake()
    {
        GameEvents.OnRequestUpdateInventory += HandleUpdateInventory;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestUpdateInventory -= HandleUpdateInventory;
    }

    void Start()
    {
        _MaxSlotCount = GameEvents.RaiseRequestInventorySlotCount();

        _Slots = new InventorySlot[_MaxSlotCount];

        for (int i = 0; i < _MaxSlotCount; i++)
        {
            var go = Instantiate(_SlotPrefab, _InventoryUIPanel);
            _Slots[i] = go.GetComponent<InventorySlot>();
        }
    }


    // UI 여는 요청 시 갱신
    void HandleUpdateInventory()
    {
        List<ItemDataSO> inventory = GameEvents.RaiseRequestInvetoryStatus();

        foreach (var slot in _Slots)
        {
            slot.ClearSlot();
        }

        for (int i = 0; i < inventory.Count; i++)
        {
            _Slots[i].UpdateSlot(inventory[i]);
        }
    }

}
