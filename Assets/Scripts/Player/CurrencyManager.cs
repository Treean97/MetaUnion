using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
class CurrencySnapshotDTO
{
    [Serializable] public class Entry { public int id; public int amount; }
    public List<Entry> items = new();
}

public class CurrencyManager : MonoBehaviour, ISaveSection
{
    [SerializeField] private CurrencyDataPoolSO _CurrencyPoolSO;
    // id, amount
    private Dictionary<int, int> _Currencies;

    public string Key => "currency";

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

        SaveLoadManager._Inst?.Register(this);

        // UI갱신
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
        if (!_Currencies.ContainsKey(id)) { Debug.LogError("잘못된 Currency ID"); return; }
        if (amount <= 0) return; // 음수/0 무시

        long next = (long)_Currencies[id] + amount;     // 오버플로 방지
        if (next > int.MaxValue) next = int.MaxValue;

        _Currencies[id] = (int)next;
        GameEvents.RaiseRequestUpdateCurrency(id, _Currencies[id]);
        SaveLoadManager._Inst?.RequestSaveSection(Key);
    }

    bool HandleConsume(int id, int amount)
    {
        if (!_Currencies.ContainsKey(id)) { Debug.LogError("잘못된 Currency ID"); return false; }
        if (amount <= 0) return true; // 0원은 통과, 음수 요청은 차단하고 false로 바꿔도 됨

        int cur = _Currencies[id];
        if (cur < amount)
        {
            // 부족 → 차감 금지, false
            return false;
        }

        _Currencies[id] = cur - amount;   // 음수 불가
        GameEvents.RaiseRequestUpdateCurrency(id, _Currencies[id]);
        SaveLoadManager._Inst?.RequestSaveSection(Key);
        return true;
    }

    public string CaptureJson()
    {
        var dto = new CurrencySnapshotDTO();
        foreach (var kv in _Currencies)
            dto.items.Add(new CurrencySnapshotDTO.Entry { id = kv.Key, amount = kv.Value });
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return;

        CurrencySnapshotDTO dto = null;
        try { dto = JsonUtility.FromJson<CurrencySnapshotDTO>(s); } catch { }
        if (dto?.items == null) return;

        // 풀에 존재하는 통화만 반영 (정의 안 된 ID 방지)
        foreach (var e in dto.items)
        {
            if (_Currencies.ContainsKey(e.id))
                _Currencies[e.id] = Mathf.Max(0, e.amount);
        }

        // UI/시스템에 현재값 브로드캐스트
        foreach (var kv in _Currencies)
            GameEvents.RaiseRequestUpdateCurrency(kv.Key, kv.Value);
    }


}
