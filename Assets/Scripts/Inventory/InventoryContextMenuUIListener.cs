using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContextMenuUIListener : MonoBehaviour
{
    [SerializeField] InventoryContextMenuUIManager _InventoryContextMenuUIManager;

    void OnEnable()
    {
        InventorySlot.OnRightClickInventorySlot += HandleSetActive;
    }

    void OnDisable()
    {
        InventorySlot.OnRightClickInventorySlot -= HandleSetActive;
    }

    void HandleSetActive(Dictionary<string, Action> options, Vector2 pos)
    {
        _InventoryContextMenuUIManager.SetContextMenu(options, pos);
        _InventoryContextMenuUIManager.gameObject.SetActive(true);
    }
}
