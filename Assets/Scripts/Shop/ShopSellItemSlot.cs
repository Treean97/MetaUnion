using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSellItemSlot : MonoBehaviour
{
    [SerializeField] Image _Icon;
    [SerializeField] TMP_Text _AmountText;
    [SerializeField] TMP_Text _PriceText;
    [SerializeField] Button _SlotBtn;
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

        UpdateUI();
    }

    public void UpdateUI()
    {   
        _Icon.sprite = _ItemData.Icon;
        _AmountText.text = _Amount.ToString();
        _PriceText.text = _ItemData.Price.ToString();
    }

    void OnClickSlotBtn()
    {
        GameEvents.RaiseRequestOpenSetAmountUI(QuantityMode.Sell, _ItemData);
    }
}
