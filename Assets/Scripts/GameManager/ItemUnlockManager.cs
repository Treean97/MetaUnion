using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class ItemUnlockManager : MonoBehaviourPunCallbacks, ICloudSaveSection
{
    private readonly HashSet<string> _Unlocked = new();

    [Serializable]
    private class UnlockDTO
    {
        public List<string> ids = new();
    }

    public string Key => "unlock";

    public static event Action<CustomizeItemSO> OnItemUnlocked;

    // --- Unity lifecycle ---
    public override void OnEnable()
    {
        base.OnEnable();

        // 이벤트 구독
        GameEvents.OnRequestLockedItems   += HandleRequestLockedItems;
        GameEvents.OnRequestUnlockedItems += HandleRequestUnlockedItems;
        GameEvents.OnRequestUnlockItem    += TryPurchase;

        // 클라우드 섹션 등록(로그인 후 LoadAllCloud가 돌면 ApplyJson이 호출됨)
        SaveLoadManager._Inst?.RegisterCloud(this);
    }

    public override void OnDisable()
    {
        GameEvents.OnRequestLockedItems   -= HandleRequestLockedItems;
        GameEvents.OnRequestUnlockedItems -= HandleRequestUnlockedItems;
        GameEvents.OnRequestUnlockItem    -= TryPurchase;

        base.OnDisable();
    }

    // --- Queries for UI ---
    void HandleRequestLockedItems(ItemType type)
    {
        if (ItemManager._Inst.CustomizeItemPoolSO == null) return;

        var locked = ItemManager._Inst.CustomizeItemPoolSO
            .GetItems(type)
            .Where(i => !_Unlocked.Contains(i.ID))
            .ToList();

        GameEvents.RaiseProvideLockedItems(locked);
    }

    void HandleRequestUnlockedItems(ItemType type)
    {
        if (ItemManager._Inst.CustomizeItemPoolSO == null) return;

        var unlocked = ItemManager._Inst.CustomizeItemPoolSO
            .GetItems(type)
            .Where(i => _Unlocked.Contains(i.ID))
            .ToList();

        GameEvents.RaiseProvideUnlockedItems(unlocked);
    }

    // --- Purchase/Unlock flow ---
    public void TryPurchase(CustomizeItemSO item)
    {
        if (_Unlocked.Contains(item.ID)) return;

        // 재화 차감 실패 시 중단
        if (!GameEvents.RaiseRequestCurrencySpend(item.CurrencyType.ID, item.BuyPrice))
        {
            Debug.LogWarning("재화가 부족합니다.");
            return;
        }

        // 해금 적용
        _Unlocked.Add(item.ID);
        OnItemUnlocked?.Invoke(item);

        // 클라우드 저장
        SaveLoadManager._Inst?.SaveCloudSection(Key);

        // UI에 “해금 성공” 반영 필요 시
        GameEvents.RaiseItemPurchaseSuccess();
    }

    // --- Cloud Save Impl ---
    public string CaptureJson()
    {
        var dto = new UnlockDTO { ids = _Unlocked.OrderBy(x => x).ToList() };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        // 클라우드에서 불러온 값 반영
        _Unlocked.Clear();

        bool hadValidData = false;
        if (!string.IsNullOrEmpty(s))
        {
            try
            {
                var dto = JsonUtility.FromJson<UnlockDTO>(s);
                if (dto?.ids != null && dto.ids.Count > 0)
                {
                    hadValidData = true;
                    foreach (var id in dto.ids)
                    {
                        if (string.IsNullOrEmpty(id)) continue;
                        _Unlocked.Add(id);
                        var so = ItemManager._Inst?.GetCustomizeItem(id);
                        if (so != null) OnItemUnlocked?.Invoke(so);
                    }
                }
            }
            catch { /* 무시하고 기본값 처리로 진행 */ }
        }

        // 클라우드에 아무 것도 없으면 “기본 해금”을 적용하고 즉시 저장
        if (!hadValidData)
            EnsureDefaultsAndSave();
    }

    void EnsureDefaultsAndSave()
    {
        if (ItemManager._Inst?.CustomizeItemPoolSO == null)
        {
            Debug.LogError("[ItemUnlockManager] CustomizeItemPoolSO가 없습니다.");
            return;
        }

        bool changed = false;
        foreach (var item in ItemManager._Inst.CustomizeItemPoolSO.GetDefaultUnlockedItems())
        {
            if (_Unlocked.Add(item.ID))
            {
                OnItemUnlocked?.Invoke(item);
                changed = true;
            }
        }

        if (changed)
            SaveLoadManager._Inst?.SaveCloudSection(Key);
    }
}
