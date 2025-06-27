using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// 전역 접근용 싱글톤 매니저
public class ItemManager : MonoBehaviour
{
    public static ItemManager _Inst { get; private set; }

    [Header("Static Data")]
    [SerializeField] private CustomizeItemPoolSO _CustomizeItemPoolSO;

    // 런타임에 빠르게 조회할 딕셔너리
    private Dictionary<ItemType, List<CustomizeItemSO>> _ItemsByType;

    void Awake()
    {
        if (_Inst != null && _Inst != this) 
        {
            Destroy(gameObject);
            return;
        }
        _Inst = this;

        // SO에서 항목을 분류해 캐싱
        _ItemsByType = _CustomizeItemPoolSO
            .GetAllItems()
            .GroupBy(item => item.Type)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// 타입별 전체 아이템 리스트 조회
    /// </summary>
    public IReadOnlyList<CustomizeItemSO> GetItems(ItemType type)
    {
        return _ItemsByType.TryGetValue(type, out var list)
            ? list
            : Array.Empty<CustomizeItemSO>();
    }

    /// <summary>
    /// ID로 단일 아이템 조회
    /// </summary>
    public CustomizeItemSO GetItem(ItemType type, string id)
    {
        return GetItems(type).FirstOrDefault(i => i.ID == id);
    }
}
