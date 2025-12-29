using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour,
IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler, IPointerClickHandler, IItemDataProvider
{
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _Amount;
    private ItemDataSO _ItemDataSO;
    public ItemDataSO ItemDataSO => _ItemDataSO;
    InventoryItem _InventoryItem;
    int _SlotIndex;

    public static event Action<ItemDataSO> OnBeginDragSlot;
    public static event Action OnEndDragSlot;
    public static event Action<Dictionary<string, Action>, Vector2> OnRightClickInventorySlot;

    public void Init(int index)
    {
        _SlotIndex = index;
    }

    public void UpdateSlot(InventoryItem inventory)
    {
        if (!ItemManager._Inst.ItemDataPoolSO.TryGetItem(inventory.ID, out _ItemDataSO))
        {
            Debug.LogError($"{inventory.ID}Data is not exist");
            ClearSlot();
            return;
        }

        _InventoryItem = inventory;

        Debug.Log($"Update Inventroy / {inventory.ID}, {inventory.Amount}");
        // 아이템 정보 저장
        ItemManager._Inst.ItemDataPoolSO.TryGetItem(inventory.ID, out _ItemDataSO);
        _Icon.sprite = _ItemDataSO.Icon;
        _Icon.enabled = true;
        _Amount.text = inventory.Amount.ToString();
        _Amount.enabled = true;
    }

    public void ClearSlot()
    {
        _ItemDataSO = null;
        _Icon.enabled = false;
        _Amount.text = "";
        _Amount.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnBeginDragSlot?.Invoke(_ItemDataSO);

    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragSlot?.Invoke();
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (dragged == null) return;
        
        GameEvents.RaiseRequestSwapSlot(dragged._SlotIndex, this._SlotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _ItemDataSO != null)
        {
            // 런타임 데이터에 접근 가능
            Debug.Log($"아이템 ID: {_InventoryItem.ID}, 수량: {_InventoryItem.Amount}");

            var menuOptions = _ItemDataSO.Actions.
                ToDictionary(action => action.Label,
                action => (Action)(() => action.Execute(_InventoryItem, PlayerSetup._LocalPlayer))
            );

            OnRightClickInventorySlot?.Invoke(menuOptions, eventData.position);
        }
        
    }

    public InfoDataSO GetItemData()
    {
        if (!_ItemDataSO) return null;
        return _ItemDataSO.InfoData;
    }
}
