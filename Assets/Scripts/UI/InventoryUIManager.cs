using System.Collections.Generic;
using System.Data.Common;
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

    void Start()
    {
        _MaxSlotCount = GameEvents.RaiseRequestInventorySlotCount();

        _Slots = new InventorySlot[_MaxSlotCount];

        for (int i = 0; i < _MaxSlotCount; i++)
        {
            var go = Instantiate(_SlotPrefab, _InventoryUIPanel);
            _Slots[i] = go.GetComponent<InventorySlot>();
        }

        // 한번 초기화
        HandleUpdateInventory();
    }

    void OnDestroy()
    {
        GameEvents.OnRequestUpdateInventory -= HandleUpdateInventory;
    }

    // 인벤토리 갱신
    void HandleUpdateInventory()
    {
        Debug.Log("Update Inventory");
        Dictionary<int, int> inventory = GameEvents.RaiseRequestInventoryStatus();

        foreach (var slot in _Slots)
        {
            slot.ClearSlot();
        }

        int index = 0;
        foreach (var item in inventory)
        {
            int id = item.Key;
            int amount = item.Value;

            _Slots[index].UpdateSlot(id, amount);
            index++;
        }
    }

}
