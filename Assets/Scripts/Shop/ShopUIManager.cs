using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [Header("Buy Area")]
    [SerializeField] private CustomizeItemPoolSO _BuyItemPool;
    [SerializeField] private Transform _BuyContents;
    [SerializeField] private GameObject _BuySlotPrefab;
    [SerializeField] private List<Button> _CategoryBtns;

    private ItemType _CurType;

    [Header("Sell Area")]
    [SerializeField] private ItemDataPoolSO _SellItemPool;
    [SerializeField] private Transform _SellContents;
    [SerializeField] private GameObject _SellSlotPrefab;
    [SerializeField] private Button _CloseBtn;



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
        // UI 켜짐(입력제한)
        InputBlock.BlockInput();
        // 잠긴 아이템 리스트 요청
        GameEvents.OnProvideLockedItems += HandleProvideBuyItems;
        // 활성화될 때 현재 카테고리 요청
        GameEvents.RaiseRequestLockedItems(_CurType);
        // 상점 구매 시 카테고리 갱신을 위한 이벤트
        GameEvents.OnItemPurchaseSuccess += HandlePurchaseSueecss;
    }

    void OnDisable()
    {
        InputBlock.UnblockInput();
        GameEvents.OnProvideLockedItems -= HandleProvideBuyItems;
        GameEvents.OnItemPurchaseSuccess -= HandlePurchaseSueecss;
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
            var slot = go.GetComponent<ShopItemSlot>();
            if (slot == null)
            {
                Debug.LogError("[ShopUIManager] Slot Prefab에 ShopItemSlot 컴포넌트가 없습니다!");
                continue;
            }

            // 3) 슬롯 세팅 (이 안에서 버튼 클릭 리스너까지 모두 처리됨)
            slot.Setup(item);
        }
    }

    void HandleProvideSellItems(List<ItemDataSO> items)    
    {

    }


}