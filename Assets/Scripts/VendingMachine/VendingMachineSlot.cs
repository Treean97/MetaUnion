using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendingMachineSlot : MonoBehaviour,
IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _Icon;
    [SerializeField] private Image _CurrencyIcon;
    [SerializeField] private TMP_Text _PriceText;
    [SerializeField] private Button _BuyButton;
    private ItemDataSO _ItemDataSO;

    public static event Action<ItemDataSO> OnPointerEnterVendingMachineSlot;
    public static event Action OnPointerExitVendingMachineSlot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterVendingMachineSlot?.Invoke(_ItemDataSO);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitVendingMachineSlot?.Invoke();
    }

    public void SetSlot(ItemDataSO itemDataSO)
    {
        _ItemDataSO = itemDataSO;
        _Icon.sprite = itemDataSO.Icon;
        var cur = itemDataSO.CurrencyType;
        _CurrencyIcon.sprite = cur.Icon;
        _PriceText.text = itemDataSO.BuyPrice.ToString();

        _BuyButton.onClick.RemoveAllListeners();
        _BuyButton.onClick.AddListener(OnClickSlot);
    }

    void OnClickSlot()
    {
        UIRouter._Inst.Open<ISetAmountUI>(ui => ui.SetUI(QuantityMode.Buy, _ItemDataSO));
    }
}
