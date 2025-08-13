using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Pun.Demo.Cockpit;
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
        GameEvents.OnRequestItemSpend += HandleItemSpend;
        GameEvents.OnRequestInventorySlotCount += HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus += HandleInventoryStatus;
        GameEvents.OnRequestSwapSlot += HandleSwapSlot;
        GameEvents.OnRequestCheckItemAmount += HandleCheckItemAmount;
    }

    void OnDisable()
    {
        GameEvents.OnRequestItemGain -= HandleItemGain;
        GameEvents.OnRequestItemSpend += HandleItemSpend;
        GameEvents.OnRequestInventorySlotCount -= HandleInventorySlotCount;
        GameEvents.OnRequestInventoryStatus -= HandleInventoryStatus;
        GameEvents.OnRequestSwapSlot -= HandleSwapSlot;
        GameEvents.OnRequestCheckItemAmount -= HandleCheckItemAmount;
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

    bool HandleItemSpend(int id, int amount)
    {
        // 해당 아이템 슬롯 찾기
        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID == id && _Inventory[i].Amount >= amount)
            {
                _Inventory[i].Amount -= amount;

                // 슬롯이 비었으면 ID 초기화
                if (_Inventory[i].Amount == 0) _Inventory[i].ID = -1;
                return true;
            }
        }
        return false;
    }


    void HandleSwapSlot(int from, int to)
    {
        // 같은 슬롯일 경우
        if (from == to)
        {
            return;
        }

        // 인덱스 교환
        (_Inventory[from], _Inventory[to]) = (_Inventory[to], _Inventory[from]);

        GameEvents.RaiseRequestUpdateInventory();
    }

    bool HandleCheckItemAmount(int id, int amount)
    {
        // 추후 스택 제한을 넣는다는 가정
        int total = 0;

        for (int i = 0; i < _Inventory.Length; i++)
        {
            if (_Inventory[i].ID != id) continue;

            total += _Inventory[i].Amount;

            if (total >= amount)
            {
                Debug.Log("Item is enough");
                return true;
            }
                
        }

        // 인벤토리 비었음
        return false;
    }
}
