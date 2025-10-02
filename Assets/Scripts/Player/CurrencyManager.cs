using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class CurrencyManager : MonoBehaviour
{
    // 전역 인스턴스
    public static CurrencyManager _Inst { get; private set; }

    [SerializeField] private CurrencyDataPoolSO _CurrencyPoolSO;

    // 표시/선검사용 로컬 캐시 (id -> amount)
    private readonly Dictionary<int, int> _Cache = new();

    void Awake()
    {
        // 싱글톤 보장 + 씬 전환 유지
        if (_Inst && _Inst != this) { Destroy(this); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        // 캐시 초기화
        foreach (var cur in _CurrencyPoolSO.GetAllCurrencies())
            _Cache[cur.ID] = 0;
    }

    void OnEnable()
    {
        GameEvents.OnRequestCurrencyGain += HandleGain;
        GameEvents.OnRequestCurrencySpend += HandleSpend;
        PlayfabLoginManager.OnLoginSuccess += RefreshFromPlayFab;
    }

    void OnDisable()
    {
        GameEvents.OnRequestCurrencyGain -= HandleGain;
        GameEvents.OnRequestCurrencySpend -= HandleSpend;
        PlayfabLoginManager.OnLoginSuccess -= RefreshFromPlayFab;
    }

    // -------- 외부 공개 API --------

    /// <summary>로그인 성공 직후 1회 호출: 서버에서 최신 잔액 동기화</summary>
    public void RefreshFromPlayFab()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), res =>
        {
            foreach (var cur in _CurrencyPoolSO.GetAllCurrencies())
            {
                int v = 0;
                if (_CurrencyPoolSO.TryGetCode(cur.ID, out var code) &&
                    res.VirtualCurrency != null &&
                    res.VirtualCurrency.TryGetValue(code, out var got))
                    v = Mathf.Max(0, got);

                _Cache[cur.ID] = v;
                GameEvents.RaiseRequestUpdateCurrency(cur.ID, v); // UI 갱신
            }
        },
        err => Debug.LogError(err.GenerateErrorReport()));
    }

    /// <summary>현재 캐시된 잔액 조회(표시/선검사 용)</summary>
    public int GetCached(int id) => _Cache.TryGetValue(id, out var v) ? v : 0;

    // -------- 내부 처리 --------

    void HandleGain(int id, int amount)
    {
        if (amount <= 0) return;
        if (!_CurrencyPoolSO.TryGetCode(id, out var code)) { Debug.LogError($"[Currency] 코드 없음: {id}"); return; }

        var req = new ExecuteCloudScriptRequest {
            FunctionName = "addVC",
            FunctionParameter = new { code = code, amount = amount },
            GeneratePlayStreamEvent = false
        };

        PlayFabClientAPI.ExecuteCloudScript(req,
            r => ApplyFromFunctionResult(r.FunctionResult),
            e => Debug.LogError(e.GenerateErrorReport()));
    }

    bool HandleSpend(int id, int amount)
    {
        if (amount <= 0) return true;
        if (!_CurrencyPoolSO.TryGetCode(id, out var code)) { Debug.LogError($"[Currency] 코드 없음: {id}"); return false; }

        // UX용 선검사(서버 진실과 다를 수 있음)
        if (_Cache.TryGetValue(id, out var cur) && cur < amount) return false;

        var req = new ExecuteCloudScriptRequest {
            FunctionName = "subVC",
            FunctionParameter = new { code = code, amount = amount },
            GeneratePlayStreamEvent = false
        };

        PlayFabClientAPI.ExecuteCloudScript(req,
            r => ApplyFromFunctionResult(r.FunctionResult),
            e => Debug.LogError(e.GenerateErrorReport()));

        return true;
    }

    // CloudScript에서 { ok:true, vc:{ "GO":123, "DI":4 } } 형태로 내려주는 결과 적용
    void ApplyFromFunctionResult(object fr)
    {
        if (fr is Dictionary<string, object> root)
        {
            // ✅ ok 플래그 체크
            if (root.TryGetValue("ok", out var okObj) && okObj is bool ok && !ok)
            {
                Debug.LogWarning("[Currency] CloudScript ok=false. reason=" + (root.TryGetValue("reason", out var r) ? r : "unknown"));
                // 서버 상태로 재동기화
                RefreshFromPlayFab();
                return;
            }

            if (root.TryGetValue("vc", out var vcObj) && vcObj is Dictionary<string, object> vc)
            {
                foreach (var cur in _CurrencyPoolSO.GetAllCurrencies())
                {
                    int v = 0;
                    if (_CurrencyPoolSO.TryGetCode(cur.ID, out var code) && vc.TryGetValue(code, out var raw))
                        v = Mathf.Max(0, Convert.ToInt32(raw));

                    _Cache[cur.ID] = v;
                    GameEvents.RaiseRequestUpdateCurrency(cur.ID, v);
                }
                return;
            }
        }
        // 포맷 미스매치 → 풀 리프레시
        RefreshFromPlayFab();
    }
}
