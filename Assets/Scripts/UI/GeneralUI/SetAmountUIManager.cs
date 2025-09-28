using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum QuantityMode { Buy, Sell }

public class SetAmountUIManager : MonoBehaviour, ISetAmountUI
{
    [SerializeField] private TMP_InputField _AmountInputField;
    [SerializeField] private Image _Icon;
    [SerializeField] private Button _ConfirmBtn;
    [SerializeField] private Button _CloseBtn;

    private ItemDataSO _ItemData;
    private QuantityMode _Mode;

    private bool _IsOpen;
    public bool IsOpen => _IsOpen;

    void Awake()
    {
        _AmountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        _AmountInputField.characterLimit = 3;

        _AmountInputField.onEndEdit.AddListener(ClampAmount);
        _ConfirmBtn.onClick.AddListener(OnClickConfirmBtn);
        _CloseBtn.onClick.AddListener(OnClickCloseBtn);
    }


    void SetMode(QuantityMode mode)
    {
        _Mode = mode;

        var btnText = _ConfirmBtn.GetComponentInChildren<TMP_Text>();

        // 테스트
        if (_Mode == QuantityMode.Buy)
        {
            btnText.text = "Buy";
        }
        else if(_Mode == QuantityMode.Sell)
        {
            btnText.text = "Sell";
        }
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

    public void SetUI(QuantityMode mode, ItemDataSO itemData)
    {
        SetMode(mode);
        _ItemData = itemData;
        _Icon.sprite = _ItemData.Icon;

        _AmountInputField.text = "1";
    }

    void OnClickConfirmBtn()
    {
        if (_ItemData == null) { gameObject.SetActive(false); return; }

        if (!int.TryParse(_AmountInputField.text, out var amount))
            amount = 1;
        amount = Mathf.Max(1, amount);

        bool success = false;
        switch (_Mode)
        {
            case QuantityMode.Buy:
                success = GameEvents.RaiseRequestPurchaseItem(
                    _ItemData.ID, amount, 10000, _ItemData.BuyPrice);
                break;

            case QuantityMode.Sell:
                success = GameEvents.RaiseRequestSellItem(
                    _ItemData.ID, amount, 10000, _ItemData.BuyPrice);
                break;
        }

        if (success)
        {
            Hide();
        }
        
    }

    void OnClickCloseBtn()
    {
        Hide();
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
