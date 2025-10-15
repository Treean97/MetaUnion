using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSellItemSlot : MonoBehaviour, IItemDataProvider
{
    [SerializeField] private Image _Icon;
    [SerializeField] private TMP_Text _AmountText;
    [SerializeField] private Image _CurrencyIcon;
    [SerializeField] private TMP_Text _PriceText;
    [SerializeField] private Button _SlotBtn;
    private ItemDataSO _ItemData;
    private int _Amount;

    public static event Action OnRequestSellItem;

    void Awake()
    {
        _SlotBtn.onClick.AddListener(OnClickSlotBtn);
    }

    public void SetSlot(int id, int amount)
    {
        ItemManager._Inst.ItemDataPoolSO.TryGetItem(id, out _ItemData);
        _Amount = amount;

        _Icon.sprite = _ItemData.Icon;
        _CurrencyIcon.sprite = _ItemData.CurrencyType.Icon;
        _AmountText.text = _Amount.ToString();
        _PriceText.text = _ItemData.SellPrice.ToString();
    }

    void OnClickSlotBtn()
    {
        UIRouter._Inst.Open<ISetAmountUI>(ui => ui.SetUI(QuantityMode.Sell, _ItemData));
    }

    public InfoDataSO GetItemData()
    {
        return _ItemData.InfoData;
    }
}
