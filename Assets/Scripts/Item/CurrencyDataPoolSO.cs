using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyDataPool", menuName = "Item/CurrencyDataPool")]
public class CurrencyDataPoolSO : ScriptableObject
{
    [SerializeField] private List<ItemDataSO> _Currencies = new();

    private Dictionary<int, ItemDataSO> _CurrencyDic;
    private void Init()
    {
        if (_CurrencyDic != null) return;
        _CurrencyDic = _Currencies.ToDictionary(x => x.ID, x => x);
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
}
