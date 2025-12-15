using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class ItemUnlockManager : MonoBehaviourPunCallbacks, ICloudSaveSection
{
    // 클라우드/구매로 해금된 아이템 ID
    private readonly HashSet<string> _Unlocked = new();

    [Serializable]
    private class UnlockDTO
    {
        public List<string> ids = new();
    }

    public string Key => "unlock";

    public static event Action<CustomizeItemSO> OnItemUnlocked;

    // 언락 여부 판정
    bool IsUnlocked(CustomizeItemSO item)
    {
        if (item == null) return false;

        // SO 자체가 기본 해금이면, 항상 언락으로 간주
        if (item.IsDefaultUnlocked)
            return true;

        // 그 외에는 클라우드/구매로 해금된 목록 기준
        return _Unlocked.Contains(item.ID);
    }

    public override void OnEnable()
    {
        base.OnEnable();

        GameEvents.OnRequestLockedItems   += HandleRequestLockedItems;
        GameEvents.OnRequestUnlockedItems += HandleRequestUnlockedItems;
        GameEvents.OnRequestUnlockItem    += TryPurchase;

        // 클라우드 섹션 등록
        SaveLoadManager._Inst?.RegisterCloud(this);
    }

    public override void OnDisable()
    {
        GameEvents.OnRequestLockedItems   -= HandleRequestLockedItems;
        GameEvents.OnRequestUnlockedItems -= HandleRequestUnlockedItems;
        GameEvents.OnRequestUnlockItem    -= TryPurchase;

        base.OnDisable();
    }

    void HandleRequestLockedItems(ItemType type)
    {
        var pool = ItemManager._Inst?.CustomizeItemPoolSO;
        if (pool == null) return;

        var locked = pool
            .GetItems(type)
            .Where(i => !IsUnlocked(i))   // 기본 해금 + 구매 해금 모두 제외
            .ToList();

        GameEvents.RaiseProvideLockedItems(locked);
    }

    void HandleRequestUnlockedItems(ItemType type)
    {
        var pool = ItemManager._Inst?.CustomizeItemPoolSO;
        if (pool == null) return;

        var unlocked = pool
            .GetItems(type)
            .Where(IsUnlocked)            // 기본 해금 + 구매 해금 모두 포함
            .ToList();

        GameEvents.RaiseProvideUnlockedItems(unlocked);
    }

    public void TryPurchase(CustomizeItemSO item)
    {
        if (item == null) return;

        // 이미 언락(기본 언락 포함)이면 구매할 필요 없음
        if (IsUnlocked(item)) return;

        // 재화 차감 실패 시 중단
        if (!GameEvents.RaiseRequestCurrencySpend(item.CurrencyType.ID, item.BuyPrice))
        {
            Debug.LogWarning("재화가 부족합니다.");
            GameEvents.RaiseShowWarning("재화가 부족합니다.");
            return;
        }

        // 해금 적용 (구매로 언락된 것만 HashSet에 추가)
        if (_Unlocked.Add(item.ID))
        {
            OnItemUnlocked?.Invoke(item);
        }

        // 클라우드 저장
        SaveLoadManager._Inst?.SaveCloudSection(Key);

        // UI 갱신
        GameEvents.RaiseItemPurchaseSuccess();
    }

    public string CaptureJson()
    {
        var dto = new UnlockDTO
        {
            // 기본 언락은 SO 플래그로 처리하므로,
            // 여기엔 "추가로 구매해서 해금된" 것만 있어도 상관 없음
            ids = _Unlocked.OrderBy(x => x).ToList()
        };
        return JsonUtility.ToJson(dto);
    }

    public void ApplyJson(string s)
    {
        _Unlocked.Clear();

        if (!string.IsNullOrEmpty(s))
        {
            try
            {
                var dto = JsonUtility.FromJson<UnlockDTO>(s);
                if (dto?.ids != null)
                {
                    foreach (var id in dto.ids)
                    {
                        if (string.IsNullOrEmpty(id)) continue;
                        if (_Unlocked.Add(id))
                        {
                            var so = ItemManager._Inst?.GetCustomizeItem(id);
                            if (so != null) OnItemUnlocked?.Invoke(so);
                        }
                    }
                }
            }
            catch
            {
                // 파싱 실패 시 기본값으로 진행
            }
        }

        // 항상 기본 해금 아이템 섞어 넣고, 변경되면 저장
        EnsureDefaultsAndSave();
    }

    void EnsureDefaultsAndSave()
    {
        var pool = ItemManager._Inst?.CustomizeItemPoolSO;
        if (pool == null)
        {
            Debug.LogError("[ItemUnlockManager] CustomizeItemPoolSO가 없습니다.");
            return;
        }

        bool changed = false;

        // SO에 IsDefaultUnlocked가 체크된 아이템들
        foreach (var item in pool.GetDefaultUnlockedItems())
        {
            if (item == null || string.IsNullOrEmpty(item.ID)) continue;

            // 기본 언락은 무조건 언락에 포함되도록 보정
            if (_Unlocked.Add(item.ID))
            {
                OnItemUnlocked?.Invoke(item);
                changed = true;
            }
        }

        if (changed)
        {
            SaveLoadManager._Inst?.SaveCloudSection(Key);
        }
    }
}
