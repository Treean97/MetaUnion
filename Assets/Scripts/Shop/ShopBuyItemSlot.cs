using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 상점 내 아이템 슬롯을 담당하는 컴포넌트입니다.
/// </summary>
public class ShopBuyItemSlot : MonoBehaviour, IItemDataProvider
{
    [SerializeField] private Image _Icon;
    [SerializeField] private Image _CurrencyIcon;
    [SerializeField] private TMP_Text _PriceText;
    [SerializeField] private Button _BuyButton;

    private CustomizeItemSO _ItemSO;

    void Awake()
    {
        // 버튼 리스너 초기화 (중복 등록 방지)
        _BuyButton.onClick.RemoveAllListeners();
        _BuyButton.onClick.AddListener(OnPurchaseClicked);
    }

    /// <summary>
    /// 슬롯을 해당 아이템 정보로 설정합니다.
    /// </summary>
    public void SetSlot(CustomizeItemSO itemSO)
    {
        _ItemSO = itemSO;
        _Icon.sprite = itemSO.Sprite;
        var cur = _ItemSO.CurrencyType;
        _CurrencyIcon.sprite = cur.Icon;
        _PriceText.text = itemSO.BuyPrice.ToString();
    }

    /// <summary>
    /// 구매 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnPurchaseClicked()
    {
        if (_ItemSO == null) return;
        GameEvents.RaiseRequestUnlockItem(_ItemSO);
    }

    public InfoDataSO GetItemData()
    {
        return _ItemSO.InfoDataSO;
    }
}
