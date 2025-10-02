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
        _Map = new Dictionary<int, TMP_Text>();
        foreach (var ui in _CurrencyUIs)
        {
            if (ui.Currency == null || ui.Text == null) continue;         // 널 가드
            if (_Map.ContainsKey(ui.Currency.ID))
            {
                Debug.LogError($"[CurrencyUI] Duplicate id: {ui.Currency.ID}");
                continue;
            }
            _Map.Add(ui.Currency.ID, ui.Text);
        }
    }

    void OnEnable()
    {
        GameEvents.OnRequestUpdateCurrency += HandleUpdateCurrency;

        // 1) 켜지자마자 캐시값으로 즉시 표시(이벤트를 놓쳤어도 안전)
        var cm = CurrencyManager._Inst;
        if (cm != null)
        {
            foreach (var kv in _Map)
                kv.Value.text = cm.GetCached(kv.Key).ToString();
            // 2) 현재값 재브로드캐스트(옵션: 다른 UI도 동기화)
            cm.RebroadcastAll();
        }
        else
        {
            // 매니저가 아직 없다면 기본값
            foreach (var kv in _Map) kv.Value.text = "0";
        }
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
