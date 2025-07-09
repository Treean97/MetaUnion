using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private Dictionary<CurrencyType, int> _Currencies;

    void Awake()
    {
        _Currencies = new Dictionary<CurrencyType, int>();
        
        foreach (CurrencyType t in Enum.GetValues(typeof(CurrencyType)))
        {
            _Currencies[t] = 0;    // Gold, Silver 등 모두 0으로 초기화
        }
    }


    void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Gold+100");
            GameEvents.RaiseRequestCurrencyGain(CurrencyType.Gold, 100);
        }
    }

    void OnEnable()
    {
        GameEvents.OnRequestCurrencyGain += HandleGain;
        GameEvents.OnRequestCurrencySpend += HandleConsume;
    }

    void OnDisable()
    {
        GameEvents.OnRequestCurrencyGain -= HandleGain;
        GameEvents.OnRequestCurrencySpend -= HandleConsume;
    }

    void HandleGain(CurrencyType type, int amount)
    {
        _Currencies[type] += amount;
        GameEvents.RaiseRequestUpdateCurrency(type, _Currencies[type]);
        Debug.Log($"Add : {type}, {_Currencies[type]}");
    }

    bool HandleConsume(CurrencyType type, int amount)
    {
        if (_Currencies[type] < amount)
        {
            GameEvents.RaiseShowWarning($"{type}가 부족합니다!");
            return false;
        }

        _Currencies[type] -= amount;
        GameEvents.RaiseRequestUpdateCurrency(type, _Currencies[type]);
        Debug.Log($"Spend : {_Currencies[type]}");
        return true;
    }
    



}
