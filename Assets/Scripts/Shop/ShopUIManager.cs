using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour, IShopUI
{
    [Header("Buy Area")]
    [SerializeField] private CustomizeItemPoolSO _BuyItemPool;
    [SerializeField] private Transform _BuyContents;
    [SerializeField] private GameObject _BuySlotPrefab;
    [SerializeField] private List<Button> _CategoryBtns;

    private ItemType _CurType;

    [Header("Sell Area")]
    [SerializeField] private Transform _SellContents;
    [SerializeField] private GameObject _SellSlotPrefab;
    [SerializeField] private Button _CloseBtn;

    [SerializeField] private GameObject _SetAmountUI;

    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        // 초기 카테고리와 비활성화
        _CurType = _BuyItemPool.GetFirstType();

        // 카테고리 버튼 세팅
        for (int i = 0; i < _CategoryBtns.Count; i++)
        {
            int idx = i;
            _CategoryBtns[i].onClick.AddListener(() => ChangeCategory((ItemType)idx));
        }
        // 닫기 버튼
        _CloseBtn.onClick.AddListener(() => gameObject.SetActive(false));
    }

    void OnEnable()
    {
        // 잠긴 아이템 리스트 요청
        GameEvents.OnProvideLockedItems += HandleProvideBuyItems;
        // 활성화될 때 현재 카테고리 요청
        GameEvents.RaiseRequestLockedItems(_CurType);
        // 상점 구매 시 카테고리 갱신을 위한 이벤트
        GameEvents.OnItemPurchaseSuccess += HandlePurchaseSueecss;

        // 판매할 아이템 리스트 불러오기
        UpdateSellItems();

        // 판매 시 리스트 갱신
        GameEvents.OnRequestUpdateInventory += UpdateSellItems;

        UIRouter._Inst?.RegisterAs<IShopUI>(this);
    }

    void OnDisable()
    {
        GameEvents.OnProvideLockedItems -= HandleProvideBuyItems;
        GameEvents.OnItemPurchaseSuccess -= HandlePurchaseSueecss;
        GameEvents.OnRequestUpdateInventory -= UpdateSellItems;

        UIRouter._Inst?.UnregisterAs<IShopUI>(this);
    }

    void ChangeCategory(ItemType type)
    {
        if (_CurType == type) return;
        _CurType = type;
        GameEvents.RaiseRequestLockedItems(_CurType);
    }

    void HandlePurchaseSueecss()
    {
        GameEvents.RaiseRequestLockedItems(_CurType);
    }

    void HandleProvideBuyItems(List<CustomizeItemSO> items)
    {
        // 기존 슬롯 제거
        foreach (Transform c in _BuyContents)
            Destroy(c.gameObject);

        foreach (var item in items)
        {
            // 1) 프리팹 인스턴스 생성
            var go = Instantiate(_BuySlotPrefab, _BuyContents);

            // 2) ShopItemSlot 컴포넌트 꺼내기
            var slot = go.GetComponent<ShopBuyItemSlot>();

            // 3) 슬롯 세팅 (이 안에서 버튼 클릭 리스너까지 모두 처리됨)
            slot.Setup(item);
        }
    }

    void UpdateSellItems()
    {
        // 플레이어 인벤토리 불러오기
        InventoryItem[] inventory = GameEvents.RaiseRequestInventoryStatus();

        if (inventory == null) return;

        // 요소 초기화
        for (int i = 0; i < _SellContents.childCount; i++)
        {
            Destroy(_SellContents.GetChild(i).gameObject);
        }

        // 요소 생성
        for (int i = 0; i < inventory.Length; i++)
        {
            int id = inventory[i].ID;
            int amount = inventory[i].Amount;

            if (id < 0 || amount <= 0) continue;

            // 슬롯에 아이템 id, 수량 주입
            var obj = Instantiate(_SellSlotPrefab, _SellContents);
            obj.GetComponent<ShopSellItemSlot>().
            SetSlot(inventory[i].ID, inventory[i].Amount);

        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}