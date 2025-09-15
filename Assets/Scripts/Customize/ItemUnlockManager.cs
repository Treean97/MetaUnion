using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class ItemUnlockManager : MonoBehaviourPunCallbacks, ISaveSection
{
    [Header("사용 가능 재화")]
    [SerializeField] private ItemDataSO _UseableCurrency;

    private readonly HashSet<string> _Unlocked = new();

    [Serializable]
    private class UnlockDTO
    {
        public List<string> ids = new();
    }


    public string Key => "unlock";

    public static event Action<CustomizeItemSO> OnItemUnlocked;

    void Start()
    {
        SaveLoadManager._Inst?.Register(this);

        // SO에 설정된 기본 해금 아이템만큼 루프        
        if (ItemManager._Inst.CustomizeItemPoolSO == null)
        {
            Debug.LogError("[ItemUnlockManager] _ItemPool이 할당되지 않았습니다!");
        }
        else
        {
            bool changed = false;
            foreach (var item in ItemManager._Inst.CustomizeItemPoolSO.GetDefaultUnlockedItems())
            {
                if (_Unlocked.Add(item.ID))
                {
                    OnItemUnlocked?.Invoke(item);
                    changed = true;
                }
            }
            if (changed) SaveLoadManager._Inst?.RequestSaveSection(Key);
        }

    }

    public override void OnEnable()
    {
        base.OnEnable();
        GameEvents.OnRequestLockedItems   += HandleRequestLockedItems;
        GameEvents.OnRequestUnlockedItems += HandleRequestUnlockedItems;
        GameEvents.OnRequestUnlockItem    += TryPurchase;
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
        if (ItemManager._Inst.CustomizeItemPoolSO == null) return;
        var locked = ItemManager._Inst.CustomizeItemPoolSO.GetItems(type)
                              .Where(i => !_Unlocked.Contains(i.ID))
                              .ToList();
        GameEvents.RaiseProvideLockedItems(locked);
    }

    void HandleRequestUnlockedItems(ItemType type)
    {
        if (ItemManager._Inst.CustomizeItemPoolSO == null) return;
        var unlocked = ItemManager._Inst.CustomizeItemPoolSO.GetItems(type)
                                .Where(i => _Unlocked.Contains(i.ID))
                                .ToList();
        GameEvents.RaiseProvideUnlockedItems(unlocked);
    }

    public void TryPurchase(CustomizeItemSO item)
    {
        if (_Unlocked.Contains(item.ID)) return;
        if (!GameEvents.RaiseRequestCurrencySpend(_UseableCurrency.ID, item.Price))
        {
            Debug.LogWarning("재화가 부족합니다.");
            return;
        }

        _Unlocked.Add(item.ID);
        OnItemUnlocked?.Invoke(item);
        // 저장
        SaveLoadManager._Inst?.RequestSaveSection(Key);
        GameEvents.RaiseItemPurchaseSuccess();
    }


    public string CaptureJson()
    {
        var dto = new UnlockDTO { ids = _Unlocked.OrderBy(x => x).ToList() };
        return JsonUtility.ToJson(dto);
    
    }

    public void ApplyJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return;

        UnlockDTO dto = null;
        try { dto = JsonUtility.FromJson<UnlockDTO>(s); } catch { }
        if (dto == null || dto.ids == null) return;

        _Unlocked.Clear();
        foreach (var id in dto.ids)
        {
            if (string.IsNullOrEmpty(id)) continue;
            _Unlocked.Add(id);

            // UI 즉시 갱신이 필요하다면 이벤트도 쏴줌
            var so = ItemManager._Inst?.GetCustomizeItem(id); // 없으면 null 허용
            if (so != null) OnItemUnlocked?.Invoke(so);
        }    
    }
}
