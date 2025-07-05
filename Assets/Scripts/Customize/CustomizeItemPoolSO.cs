using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/CustomizeItemPool")]
public class CustomizeItemPoolSO : ScriptableObject
{
    // 인스펙터에 Type별로 접고 펼칠 수 있는 리스트
    [SerializeField] private List<CustomizeItemGroup> _ItemGroups;

    // 런타임에 빠르게 조회하기 위한 딕셔너리
    private Dictionary<ItemType, List<CustomizeItemSO>> _Groups;

    private void OnEnable()
    {
        _Groups = _ItemGroups
            .ToDictionary(g => g.Type, g => g.Items);
    }

    // UI 쪽에서 호출할 메서드
    public IReadOnlyList<CustomizeItemSO> GetItems(ItemType type)
    {
        if (_Groups == null) OnEnable();
        return _Groups.TryGetValue(type, out var list)
            ? list
            : new List<CustomizeItemSO>();
    }

    public ItemType GetFirstType()
    {
        if (_ItemGroups != null && _ItemGroups.Count > 0)
            return _ItemGroups[0].Type;
        throw new InvalidOperationException("그룹이 하나도 없습니다!");
    }

    public IEnumerable<CustomizeItemSO> GetAllItems()
    {
        // 모든 그룹의 Items 리스트를 하나의 시퀀스로 합쳐서 반환
        return _ItemGroups.SelectMany(group => group.Items);
    }

    public IEnumerable<CustomizeItemSO> GetDefaultUnlockedItems()
    => _ItemGroups.SelectMany(g => g.Items)
                  .Where(i => i.IsDefaultUnlocked);

}
