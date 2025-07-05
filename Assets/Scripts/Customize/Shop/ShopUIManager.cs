using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    [SerializeField] private CustomizeItemPoolSO _ItemPool;
    [SerializeField] private Transform _Contents;
    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private List<Button> _CategoryBtns;
    [SerializeField] private Button _CloseBtn;

    private ItemType _CurType;

    void Awake()
    {
        // 초기 카테고리와 비활성화
        _CurType = _ItemPool.GetFirstType();
        gameObject.SetActive(false);

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
        GameEvents.OnProvideLockedItems += HandleProvideItems;
        // 활성화될 때 현재 카테고리 요청
        GameEvents.RaiseRequestLockedItems(_CurType);
    }

    void OnDisable()
    {
        GameEvents.OnProvideLockedItems -= HandleProvideItems;
    }

    void ChangeCategory(ItemType type)
    {
        if (_CurType == type) return;
        _CurType = type;
        GameEvents.RaiseRequestLockedItems(_CurType);
    }

    void HandleProvideItems(List<CustomizeItemSO> items)
{
    // 기존 슬롯 제거
    foreach (Transform c in _Contents)
        Destroy(c.gameObject);

    foreach (var item in items)
    {
        // 1) 프리팹 인스턴스 생성
        var go = Instantiate(_SlotPrefab, _Contents);

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

}