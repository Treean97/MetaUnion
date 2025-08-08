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
        GameEvents.RaiseRequestPurchaseItem(_ItemData.ID, int.Parse(_AmountInputField.text), 10000, _ItemData.Price);
        gameObject.SetActive(false);
    }

    void OnClickCloseBtn()
    {
        gameObject.SetActive(false);
    }
}
