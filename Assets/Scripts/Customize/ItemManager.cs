using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 전역 접근용 싱글톤 매니저
public class ItemManager : MonoBehaviour
{
    public static ItemManager _Inst { get; private set; }


    [Header("커스터마이징 아이템")]
    [SerializeField] private CustomizeItemPoolSO _CustomizeItemPoolSO;
    public CustomizeItemPoolSO CustomizeItemPoolSO => _CustomizeItemPoolSO;

    [Header("일반 아이템")]
    [SerializeField] private ItemDataPoolSO _ItemDataPoolSO;
    public ItemDataPoolSO ItemDataPoolSO => _ItemDataPoolSO;

    // 런타임에 빠르게 조회할 딕셔너리
    private Dictionary<ItemType, List<CustomizeItemSO>> _ItemsByType;
    private Dictionary<string, CustomizeItemSO> _ItemsById;


    void Awake()
    {
        if (_Inst != null && _Inst != this) { Destroy(this); return; }
        _Inst = this;

        var allItem = _CustomizeItemPoolSO.GetAllItems();

        // 타입 캐시 (카테고리 UI용)
        _ItemsByType = allItem
            .GroupBy(item => item.Type)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ID 단일 캐시
        _ItemsById = new Dictionary<string, CustomizeItemSO>(StringComparer.Ordinal);
        foreach (var it in allItem)
        {
            if (string.IsNullOrEmpty(it.ID))
            {
                Debug.LogWarning("[ItemManager] 빈 ID 아이템이 있습니다.");
                continue;
            }
            if (_ItemsById.ContainsKey(it.ID))
            {
                Debug.LogError($"[ItemManager] 중복 ID 발견: {it.ID} — 마지막 항목은 무시됩니다.");
                continue;
            }
            _ItemsById[it.ID] = it;
        }
    }

    /// <summary>
    /// 타입별 전체 아이템 리스트 조회
    /// </summary>
    public IReadOnlyList<CustomizeItemSO> GetCustomizeItems(ItemType type)
    {
        return _ItemsByType.TryGetValue(type, out var list)
            ? list
            : Array.Empty<CustomizeItemSO>();
    }


    // ID로 아이템 조회
    public CustomizeItemSO GetCustomizeItem(string id)
    {
        return id != null && _ItemsById.TryGetValue(id, out var so) ? so : null;
    }
}
