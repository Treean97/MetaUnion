using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // id, count
    private Dictionary<int, int> _Inventory;
    [SerializeField] private int _MaxInventorySlot;

    void Awake()
    {
        _Inventory = new Dictionary<int, int>();
        
    }

    void OnEnable()
    {
        GameEvents.OnRequestItemGain += HandleItemGain;
        GameEvents.OnRequestInventorySlotCount += HandleInventorySlotCount;
    }

    void OnDisable()
    {
        GameEvents.OnRequestItemGain -= HandleItemGain;
        GameEvents.OnRequestInventorySlotCount -= HandleInventorySlotCount;
    }

    int HandleInventorySlotCount()
    {
        return _MaxInventorySlot;   
    } 

    void HandleItemGain(int id, int amount)
    {
        Debug.Log($"Item Gain");
        if (_Inventory.ContainsKey(id))
        {
            _Inventory[id] += amount;
        }
        else
        {
            _Inventory.Add(id, amount);
        }
    }

    void HandleItemSpend()
    {

    }
}
