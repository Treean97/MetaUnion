using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct CurrencyUI
{
    public CurrencyType Type;
    public TMP_Text Text;
}

public class CurrencyUIManager : MonoBehaviour
{
    [SerializeField] private List<CurrencyUI> _CurrencyUIs;

    private Dictionary<CurrencyType, TMP_Text> _Map;

    void Awake()
    {
        // 에디터에서 설정한 리스트를 딕셔너리로 변환
        _Map = new Dictionary<CurrencyType, TMP_Text>();
        foreach (var ui in _CurrencyUIs)
            _Map[ui.Type] = ui.Text;
    }

    void OnEnable()
    {
        GameEvents.OnRequestUpdateCurrency += HandleChangeCurrency;
    }

    void OnDisable()
    {
        GameEvents.OnRequestUpdateCurrency -= HandleChangeCurrency;
    }


    void HandleChangeCurrency(CurrencyType type, int amount)
    {
        if (_Map.TryGetValue(type, out var text))
        {
            text.text = amount.ToString();
        }
    }
}
