using TMPro;
using UnityEngine;

public class CurrencyUIManager : MonoBehaviour
{
    [SerializeField] TMP_Text _GoldText;

    void OnEnable()
    {
        GameEvents.OnChangeCurrency += HandleChangeCurrency;
    }

    void OnDisable()
    {
        GameEvents.OnChangeCurrency -= HandleChangeCurrency;
    }


    void HandleChangeCurrency(int amount)
    {
        _GoldText.text = amount.ToString();
    }
}
