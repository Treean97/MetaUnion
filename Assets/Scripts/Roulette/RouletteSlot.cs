using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteSlot : MonoBehaviour
{
    [SerializeField] private Image _Icon;
    [SerializeField] private Sprite _DefaultSprite;
    [SerializeField] private TMP_Text _AmountText;
    private ItemDataSO _ItemDataSO;
    public ItemDataSO ItemDataSO => _ItemDataSO;
    private int _Amount;
    public int Amount => _Amount;

    public void SetSlot(ItemDataSO itemDataSO, int amount)
    {
        _ItemDataSO = itemDataSO;
        _Icon.sprite = _ItemDataSO.Icon;
        _Amount = amount;
        _AmountText.text = _Amount.ToString();
        _AmountText.enabled = true;
    }   


    public void DefaultSet()
    {
        _Icon.sprite = _DefaultSprite;
        _AmountText.enabled = false;
    }

}
