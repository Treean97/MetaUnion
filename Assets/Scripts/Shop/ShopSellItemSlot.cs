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

    public event Action OnRequestSellItem;

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

    // 슬롯 누르면 수량 UI 표시(이벤트로 itemdata, amount 전달)
    // 수량 UI 스크립트에서 판매 수량 입력하면 
    void OnClickSlotBtn()
    {
        
    }
}
