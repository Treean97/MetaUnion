using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetAmountUIManager : MonoBehaviour
{    
    [SerializeField] private TMP_InputField _AmountInputField;
    [SerializeField] Image _Icon;
    [SerializeField] private Button _BuyBtn;
    [SerializeField] private Button _CloseBtn;

    private ItemDataSO _ItemData;

    public static event Func<ItemDataSO, int, bool> OnConfirmBuy;

    void Awake()
    {
        _AmountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        _AmountInputField.characterLimit = 3;
        _AmountInputField.onEndEdit.AddListener(ClampAmount);

        _BuyBtn.onClick.AddListener(OnClickBuyBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }

    void ClampAmount(string text)
    {
        if (int.TryParse(text, out int value) && value >= 1)
        {
            _AmountInputField.text = value.ToString();
        }
        else
        {
            _AmountInputField.text = "1";
        }
    }

    public void SetUI(ItemDataSO itemData)
    {
        _ItemData = itemData;
        _Icon.sprite = _ItemData.Icon;

        _AmountInputField.text = "1";
    }

    void OnClickBuyBtn()
    {
        if (!int.TryParse(_AmountInputField.text, out var amount))
        {
            amount = 1;
        }
        amount = Mathf.Max(1, amount);

        bool success = OnConfirmBuy?.Invoke(_ItemData, amount) ?? false;

        if (success)
        {
            gameObject.SetActive(false);
        }
    }

    void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }
}
