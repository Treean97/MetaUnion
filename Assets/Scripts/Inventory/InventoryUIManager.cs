using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour, IInventoryUI
{
    [SerializeField] private RectTransform _InventoryUI;
    [SerializeField] private Vector2 _OpenPosition = new Vector2(0, -380);
    [SerializeField] private Vector2 _ClosePosition = new Vector2(0, -700);
    [SerializeField] private float _Duration = 0.5f;

    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private Transform _InventoryUIPanel;
    [SerializeField] private Image _DragImage;
    private InventorySlot[] _Slots;
    private int _MaxSlotCount;
    private bool _IsOpen;
    public bool IsOpen => _IsOpen;
    private Coroutine _CoMove;

    void Awake()
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
            var go = Instantiate(_SlotPrefab, _InventoryUIPanel);
            _Slots[i] = go.GetComponent<InventorySlot>();
            _Slots[i].Init(i);
        }

        // 한번 초기화
        HandleUpdateInventory();
    }

    void OnDestroy()
    {
        GameEvents.OnRequestUpdateInventory -= HandleUpdateInventory;
        InventorySlot.OnBeginDragSlot -= HandleBeginDragSlot;
        InventorySlot.OnEndDragSlot -= HandleEndDragSlot;
    }

    // 인벤토리 갱신
    void HandleUpdateInventory()
    {
        Debug.Log("Update Inventory");
        InventoryItem[] inventory = GameEvents.RaiseRequestInventoryStatus();

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

    public void Show() => MoveTo(_OpenPosition,  true);
    public void Hide() => MoveTo(_ClosePosition, false);    

    void MoveTo(Vector2 end, bool open)
    {
        if (_CoMove != null) { StopCoroutine(_CoMove); _CoMove = null; }

        // 항상 현재 위치에서 시작 → 중간 전환도 부드러움
        Vector2 start = _InventoryUI.anchoredPosition;
        _CoMove = StartCoroutine(CoMove(start, end, open));
    }

    IEnumerator CoMove(Vector2 start, Vector2 end, bool open)
    {
        float elapsed = 0f;

        while (elapsed < _Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _Duration);
            _InventoryUI.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        _InventoryUI.anchoredPosition = end;
        _IsOpen = open;
        _CoMove = null;
    }
}
