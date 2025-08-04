using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler,
 IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _Amount;
    [SerializeField] private GameObject _InventoryFocusUI;
    private ItemDataSO _ItemDataSO;
    public ItemDataSO ItemDataSO => _ItemDataSO;
    int _SlotIndex;
    Image _DragImage;

    public static event Action<ItemDataSO> OnPointerEnterInventorySlot;
    public static event Action OnPointerExitInventorySlot;

    public void Init(int index, Image dragImage)
    {
        _SlotIndex = index;
        _DragImage = dragImage;
    }

    public void UpdateSlot(InventoryItem inventory)
    {
        if (!ItemManager._Inst.ItemDataPoolSO.TryGetItem(inventory.ID, out _ItemDataSO))
        {
            Debug.Log($"{inventory.ID}Data is not exist");
            ClearSlot();
            return;
        }

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
        _DragImage.sprite = _ItemDataSO.Icon;
        _DragImage.gameObject.SetActive(true);
        _DragImage.transform.position = eventData.position;

        _Icon.enabled = false;
        _Amount.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _DragImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _DragImage.gameObject.SetActive(false);

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
}
