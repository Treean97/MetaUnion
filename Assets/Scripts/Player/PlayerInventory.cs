using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // id, count
    private Dictionary<int, int> _Inventory;

    void Awake()
    {
        _Inventory = new Dictionary<int, int>();
        GameEvents.OnRequestItemGain += HandleItemGain;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestItemGain -= HandleItemGain;
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
