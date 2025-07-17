using System.Collections.Generic;
using UnityEngine;

public class InventoryItem
{
    public int ID;
    public int Amount;
}

public class PlayerInventory : MonoBehaviour
{
    // id, count
    private InventoryItem[] _Inventory;
    [SerializeField] private int _MaxInventorySlot;

    void Awake()
    {
        _Inventory = new InventoryItem[_MaxInventorySlot];

        for (int i = 0; i < _MaxInventorySlot; i++)
            _Inventory[i] = new InventoryItem { ID = -1, Amount = 0 };  
            
    }

    void OnEnable()
    {
        GameEvents.OnRequestItemGain += HandleItemGain;
        GameEvents.OnRequestInventorySlotCount += HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus += HandleInventoryStatus;
    }

    void OnDisable()
    {
        GameEvents.OnRequestItemGain -= HandleItemGain;
        GameEvents.OnRequestInventorySlotCount -= HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus -= HandleInventoryStatus;
    }

    int HandleInventorySlotCount()
    {
        return _MaxInventorySlot;
    }

    InventoryItem[] HandleInventoryStatus()
    {
        return _Inventory;
    }

    bool HandleItemGain(int id, int amount)
    {
        Debug.Log($"Item Gain {id}, {amount}");

        // 1) 이미 있는 슬롯에 합산
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID == id)
            {
                _Inventory[i].Amount += amount;
                return true;
            }
        }
        // 2) 빈 슬롯에 새로 추가
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID < 0)
            {
                _Inventory[i].ID = id;
                _Inventory[i].Amount = amount;
                return true;
            }
        }
        // 3) 슬롯 가득 찬 경우 실패
        return false;
    
    }

    void HandleItemSpend()
    {

    }
}
