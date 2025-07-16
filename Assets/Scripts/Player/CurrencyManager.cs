using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private CurrencyDataPoolSO _CurrencyPoolSO;
    // id, amount
    private Dictionary<int, int> _Currencies;

    void Awake()
    {
        _Currencies = new Dictionary<int, int>();
        
        foreach (var currency in _CurrencyPoolSO.GetAllCurrencies())
        {
            _Currencies[currency.ID] = 0;
        }
    }
    void Start()
    {
        // 초기 값 세팅(세이브 로드 대비)
        foreach (var kv in _Currencies)
        {
            GameEvents.RaiseRequestUpdateCurrency(kv.Key, kv.Value);
        }
            
    }

    void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Gold+100");
            GameEvents.RaiseRequestCurrencyGain(10000, 100);
            GameEvents.RaiseRequestCurrencyGain(10001, 100);
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

    void HandleGain(int id, int amount)
    {
        if (!_Currencies.ContainsKey(id))
        {
            Debug.LogError("잘못된 Item ID 입니다");
            return;
        }

        _Currencies[id] += amount;
        GameEvents.RaiseRequestUpdateCurrency(id, _Currencies[id]);
        Debug.Log($"Add : {id}, {_Currencies[id]}");
    }

    bool HandleConsume(int id, int amount)
    {
        if (!_Currencies.ContainsKey(id))
        {
            Debug.LogError("잘못된 Item ID 입니다");
            return false;
        }

        if (_Currencies[id] < amount)
        {
            GameEvents.RaiseShowWarning($"{id}가 부족합니다!");
            return false;
        }

        _Currencies[id] -= amount;
        GameEvents.RaiseRequestUpdateCurrency(id, _Currencies[id]);
        Debug.Log($"Spend : {_Currencies[id]}");
        return true;
    }
    



}
