using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour, IInventoryUI
{
    [SerializeField] private UISlider _UISlider;
    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private Transform _InventoryUIContent;
    [SerializeField] private Image _DragImage;
    private InventorySlot[] _Slots;
    private int _MaxSlotCount;
    public bool IsOpen => _UISlider != null && _UISlider.IsOpen;

    void OnEnable()
    {
        GameEvents.OnRequestUpdateInventory += HandleUpdateInventory;
        InventorySlot.OnBeginDragSlot += HandleBeginDragSlot;
        InventorySlot.OnEndDragSlot += HandleEndDragSlot;
    }

    void Start()
    {
        _MaxSlotCount = GameEvents.RaiseRequestInventorySlotCount();

        _Slots = new InventorySlot[_MaxSlotCount];

        for (int i = 0; i < _MaxSlotCount; i++)
        {
            var go = Instantiate(_SlotPrefab, _InventoryUIContent);
            _Slots[i] = go.GetComponent<InventorySlot>();
            _Slots[i].Init(i);
        }
    }

    void OnDisable()
    {
        GameEvents.OnRequestUpdateInventory -= HandleUpdateInventory;
        InventorySlot.OnBeginDragSlot -= HandleBeginDragSlot;
        InventorySlot.OnEndDragSlot -= HandleEndDragSlot;
    }

    // 인벤토리 갱신
    void HandleUpdateInventory()
    {
        Debug.Log("Update Inventory");
        var inventory = GameEvents.RaiseRequestInventoryStatus();
        if (inventory == null || _Slots == null) return; // 널가드

        foreach (var slot in _Slots)
        {
            slot.ClearSlot();
        }

        int count = Mathf.Min(inventory.Length, _Slots.Length);
        for (int i = 0; i < count; i++)
        {
            // 빈 슬롯이 아니면
            if (inventory[i].ID >= 0)
            {
                _Slots[i].UpdateSlot(inventory[i]);
            }
        }
    }

    void HandleBeginDragSlot(ItemDataSO itemDataSO)
    {
        if (itemDataSO == null)
            return;

        _DragImage.sprite = itemDataSO.Icon;
        _DragImage.gameObject.SetActive(true);
    }

    void HandleEndDragSlot()
    {
        _DragImage.gameObject.SetActive(false);
    }

    public void Show()   { if (_UISlider) _UISlider.Show(); }
    public void Hide()   { if (_UISlider) _UISlider.Hide(); }
    public void Toggle() { if (_UISlider) _UISlider.Toggle(); }
}
