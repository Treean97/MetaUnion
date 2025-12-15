using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CustomizeUIManager : MonoBehaviour, ICustomizeUI
{
    [SerializeField] private CustomizeItemPoolSO _ItemPool;
    [SerializeField] private CustomizePreivew _CustomizePreview;
    [SerializeField] private Transform _Contents;
    [SerializeField] private GameObject _SlotPrefab;
    [SerializeField] private List<Button> _CategoryBtns;

    private ItemType _CurType;

    public bool IsOpen => gameObject.activeSelf;

    private const string _PropKeyPrefix = "Customize_";

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

        // 프리뷰 카메라 타겟 변경
        _CustomizePreview.ChangeTarget(_CurType);

        RefreshStatesOfCurrentCategory();
    }

    void HandlePurchaseSueecss()
    {
        GameEvents.RaiseRequestUnlockedItems(_CurType);
    }


    private void HandleProvideItems(List<CustomizeItemSO> items)
    {
        Debug.Log($"[CustomizeUI] HandleProvideItems: 받은 아이템 {items.Count}개");
        foreach (Transform c in _Contents) Destroy(c.gameObject);

        foreach (var item in items)
        {
            var go = Instantiate(_SlotPrefab, _Contents);
            var slot = go.GetComponent<CustomizeItemSlot>();
            slot.Setup(item);

            bool equipped = IsEquippedByProps(item.Type, item.ID);
            slot.SetState(equipped);
        }
    }

    private bool IsEquippedByProps(ItemType type, string id)
    {
        var lp = PhotonNetwork.LocalPlayer;
        if (lp == null) return false;

        var props = lp.CustomProperties;
        if (props == null) return false;

        var key = _PropKeyPrefix + (int)type;
        if (!props.ContainsKey(key)) return false;

        // 값 타입이 int/str 섞여 올 수 있으니 문자열로 통일
        var cur = props[key]?.ToString();
        return cur == id;
    }


    private void HandleEquipItem(CustomizeItemSO item)
    {
        Debug.Log("[CustomizeUI] HandleEquipItem: 실제 EquipItem 호출, ID=" + item.ID);
        var player = PlayerSetup._LocalPlayer.GetComponent<PlayerCustomization>();


        // 현재 상태를 기준으로 이번 클릭의 '의도 상태'를 계산
        bool willEquip = !IsEquippedByProps(item.Type, item.ID);

        // 실제 적용
        if (willEquip) player.EquipItem(item);
        else player.UnEquipItem(item.Type);

        // 낙관적 UI 업데이트: 같은 타입 슬롯만 즉시 반영
        foreach (Transform c in _Contents)
        {
            var slot = c.GetComponent<CustomizeItemSlot>();
            if (!slot || slot.Type != item.Type) continue;

            // 방금 누른 아이템만 '해제'로, 나머지는 '착용'으로
            bool equipped = willEquip && slot.ID == item.ID;
            slot.SetState(equipped);
        }

    }

    private void RefreshStatesOfCurrentCategory()
    {
        foreach (Transform c in _Contents)
        {
            var slot = c.GetComponent<CustomizeItemSlot>();
            if (!slot) continue;
            slot.SetState(IsEquippedByProps(slot.Type, slot.ID));
        }
    }

    public void Show() { }

    public void Hide() { }

}
