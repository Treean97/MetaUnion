using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour,
IBeginDragHandler, IEndDragHandler, IDropHandler,
 IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _Amount;
    [SerializeField] private GameObject _InventoryFocusUI;
    private ItemDataSO _ItemDataSO;
    public ItemDataSO ItemDataSO => _ItemDataSO;
    InventoryItem _InventoryItem;
    int _SlotIndex;

    public static event Action<ItemDataSO> OnBeginDragSlot;
    public static event Action OnEndDragSlot;
    public static event Action<ItemDataSO> OnPointerEnterInventorySlot;
    public static event Action OnPointerExitInventorySlot;


    public void Init(int index)
    {
        _SlotIndex = index;
    }

    public void UpdateSlot(InventoryItem inventory)
    {
        if (!ItemManager._Inst.ItemDataPoolSO.TryGetItem(inventory.ID, out _ItemDataSO))
        {
            Debug.Log($"{inventory.ID}Data is not exist");
            ClearSlot();
            return;
        }

        _InventoryItem = inventory;

        Debug.Log($"Update Slot {inventory.ID}, {inventory.Amount}");
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

        _Icon.enabled = false;
        _Amount.enabled = false;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        OnEndDragSlot?.Invoke();

        _Icon.enabled = true;
        _Amount.enabled = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (dragged == null) return;

        GameEvents.RaiseRequestSwapSlot(dragged._SlotIndex, this._SlotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterInventorySlot?.Invoke(_ItemDataSO);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitInventorySlot?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && _ItemDataSO != null)
    {
        // 런타임 데이터에 접근 가능
        Debug.Log($"아이템 ID: {_InventoryItem.ID}, 수량: {_InventoryItem.Amount}");

        // // ItemDataSO에서 액션 목록 가져오기
        // var actions = _ItemDataSO.Actions;
        // foreach (var act in actions)
        // {
        //     ContextMenuUI.Instance.AddOption(act.Label, () => {
        //         act.Execute(_inventoryItem, Player.Instance.gameObject);
        //     });
        // }
        // ContextMenuUI.Instance.Show(transform.position);
    }
    }
}
