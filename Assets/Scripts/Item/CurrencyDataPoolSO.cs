using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyDataPool", menuName = "Item/CurrencyDataPool")]
public class CurrencyDataPoolSO : ScriptableObject
{
    [Header("통화 목록")]
    [SerializeField] private List<ItemDataSO> _Currencies = new();

    [Serializable]
    public class CodeEntry
    {
        public ItemDataSO Item;         // ItemDataSO.ID 와 동일하게
        public string Code;     // PlayFab VC 코드(대문자 2~3자) 예: "GO", "DI"
    }
    
    [Header("ID / PlayFab 통화코드 매핑")]
    [SerializeField] private List<CodeEntry> _CodeEntries = new();

    // 딕셔너리 매핑
    private Dictionary<int, ItemDataSO> _CurrencyDic;
    private Dictionary<int, string> _CodeDic;

    private void Init()
    {
        if (_CurrencyDic == null)
            _CurrencyDic = _Currencies.ToDictionary(x => x.ID, x => x);

        if (_CodeDic == null)
            _CodeDic = _CodeEntries
                .Where(e => !string.IsNullOrEmpty(e.Code))
                .ToDictionary(e => e.Item.ID, e => e.Code);
    }

    public bool TryGetCurrency(int id, out ItemDataSO data)
    {
        Init();
        return _CurrencyDic.TryGetValue(id, out data);
    }

    public IReadOnlyList<ItemDataSO> GetAllCurrencies()
    {
        Init();
        return _Currencies;
    }

    // 내부 ID > PlayFab 통화 코드
    public bool TryGetCode(int id, out string code)
    {
        Init();
        return _CodeDic.TryGetValue(id, out code);
    }
}
