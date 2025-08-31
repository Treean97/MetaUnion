using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIManager : MonoBehaviour, ICustomizeUI
{
    [SerializeField] private CustomizeItemPoolSO _ItemPool;
    [SerializeField] private Transform _Contents;
    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private List<Button> _CategoryBtns;
    [SerializeField] private Button _CloseBtn;

    private ItemType _CurType;

    public bool IsOpen => gameObject.activeSelf;

    void Awake()
    {
        // 기본 카테고리 설정
        _CurType = _ItemPool.GetFirstType();

        // 카테고리 버튼 세팅
        for (int i = 0; i < _CategoryBtns.Count; i++)
        {
            int idx = i;
            _CategoryBtns[i].onClick.AddListener(() => ChangeCategory((ItemType)idx));
        }

        // 버튼
        _CloseBtn.onClick.AddListener(() => Hide());
    }

    void OnEnable()
    {
        // 해금된 아이템 리스트 받기
        GameEvents.OnProvideUnlockedItems += HandleProvideItems;
        // 장착 요청 받기
        GameEvents.OnRequestEquipItem += HandleEquipItem;
        // 아이템 구매 성공 시 리스트 갱신
        GameEvents.OnItemPurchaseSuccess += HandlePurchaseSueecss;

        // 리스트 요청
        GameEvents.RaiseRequestUnlockedItems(_CurType);        
    }

    void OnDisable()
    {
        GameEvents.OnProvideUnlockedItems -= HandleProvideItems;
        GameEvents.OnRequestEquipItem -= HandleEquipItem;
        GameEvents.OnItemPurchaseSuccess -= HandlePurchaseSueecss;
    }

    private void ChangeCategory(ItemType type)
    {
        if (_CurType == type) return;
        _CurType = type;
        Debug.Log("[CustomizeUI] ChangeCategory: 새 카테고리 = " + _CurType);
        GameEvents.RaiseRequestUnlockedItems(_CurType);
    }

    void HandlePurchaseSueecss()
    {
        GameEvents.RaiseRequestLockedItems(_CurType);
    }


    private void HandleProvideItems(List<CustomizeItemSO> items)
    {
        Debug.Log($"[CustomizeUI] HandleProvideItems: 받은 아이템 {items.Count}개");
        foreach (Transform c in _Contents) Destroy(c.gameObject);

        foreach (var item in items)
        {
            var slot = Instantiate(_SlotPrefab, _Contents);
            slot.GetComponent<CustomizeItemSlot>().Setup(item);

            // 슬롯 클릭 시 장착 요청
            var btn = slot.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                Debug.Log("[CustomizeUI] Slot 클릭: " + item.ID);
                GameEvents.RaiseRequestEquipItem(item);
            });
        }
    }

    private void HandleEquipItem(CustomizeItemSO item)
    {
        Debug.Log("[CustomizeUI] HandleEquipItem: 실제 EquipItem 호출, ID=" + item.ID);
        var player = PlayerSetup._LocalPlayer.GetComponent<PlayerCustomization>();
        player?.EquipItem(item);
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
