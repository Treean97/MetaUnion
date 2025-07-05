using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class ItemUnlockManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private CustomizeItemPoolSO _ItemPool;
    private readonly HashSet<string> _Unlocked = new();

    public static event Action<CustomizeItemSO> OnItemUnlocked;

    void Start()
    {
        // 1) 이전 저장여부 확인
        bool hadSaved = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Unlocked");

        // 2) 네트워크/로컬에 남은 해금 정보 로드
        LoadUnlockedFromProperties();

        // 3) SO에 설정된 기본 해금 아이템만큼 루프
        if (_ItemPool == null)
        {
            Debug.LogError("[ItemUnlockManager] _ItemPool이 할당되지 않았습니다!");
        }
        else
        {
            foreach (var item in _ItemPool.GetDefaultUnlockedItems())
            {
                if (_Unlocked.Add(item.ID))
                    OnItemUnlocked?.Invoke(item);
            }
        }

        // 4) 처음 실행이면 네트워크에 기본 해금 플래그 저장
        if (!hadSaved)
            SaveUnlockedToPhoton();
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
        if (_ItemPool == null) return;
        var locked = _ItemPool.GetItems(type)
                              .Where(i => !_Unlocked.Contains(i.ID))
                              .ToList();
        GameEvents.RaiseProvideLockedItems(locked);
    }

    void HandleRequestUnlockedItems(ItemType type)
    {
        if (_ItemPool == null) return;
        var unlocked = _ItemPool.GetItems(type)
                                .Where(i => _Unlocked.Contains(i.ID))
                                .ToList();
        GameEvents.RaiseProvideUnlockedItems(unlocked);
    }

    public void TryPurchase(CustomizeItemSO item)
    {
        if (_Unlocked.Contains(item.ID)) return;
        if (!GameEvents.RaiseRequestSpendCurrency(item.Price))
        {
            Debug.LogWarning("재화가 부족합니다.");
            return;
        }

        _Unlocked.Add(item.ID);
        OnItemUnlocked?.Invoke(item);
        SaveUnlockedToPhoton();
    }

    void SaveUnlockedToPhoton()
    {
        var csv = string.Join(",", _Unlocked);
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable { { "Unlocked", csv } }
        );
    }

    void LoadUnlockedFromProperties()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties
            .TryGetValue("Unlocked", out var raw))
        {
            foreach (var id in raw.ToString()
                                 .Split(',')
                                 .Where(s => !string.IsNullOrEmpty(s)))
            {
                _Unlocked.Add(id);
            }
        }
    }
}
