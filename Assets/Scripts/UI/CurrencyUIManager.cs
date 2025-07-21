using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct CurrencyUI
{
    public ItemDataSO Currency;
    public TMP_Text Text;
}

public class CurrencyUIManager : MonoBehaviour
{
    [SerializeField] private List<CurrencyUI> _CurrencyUIs;

    private Dictionary<int, TMP_Text> _Map;

    void Awake()
    {
        // 에디터에서 설정한 리스트를 딕셔너리로 변환
        _Map = new Dictionary<int, TMP_Text>();
        foreach (var ui in _CurrencyUIs)
        {
            // 중복 키 검사
            if (_Map.ContainsKey(ui.Currency.ID))
            {
                Debug.LogError("Key Dup Error");
            }
            _Map[ui.Currency.ID] = ui.Text;
        }
            
    }

    void OnEnable()
    {
        GameEvents.OnRequestUpdateCurrency += HandleUpdateCurrency;
    }

    void OnDisable()
    {
        GameEvents.OnRequestUpdateCurrency -= HandleUpdateCurrency;
    }


    void HandleUpdateCurrency(int id, int amount)
    {
        if (_Map.TryGetValue(id, out var text))
        {
            text.text = amount.ToString();
        }
    }
}
