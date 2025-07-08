using UnityEngine;
using System.Collections.Generic;

public class PlayerStat : MonoBehaviour
{
    [Header("Default Stats")]
    [SerializeField] private PlayerStatsSO _StatsSO;

    // StatType → BaseValue 매핑
    private Dictionary<StatType, float> _BaseStats;

    private void Awake()
    {
        // ScriptableObject로 정의된 기본 스탯값 로드
        _BaseStats = new Dictionary<StatType, float>();
        foreach (var entry in _StatsSO.DefaultStats)
        {
            _BaseStats[entry.Type] = entry.BaseValue;
        }
    }

    public float GetBaseStat(StatType type)
    {
        if (_BaseStats.TryGetValue(type, out var value))
            return value;
        Debug.LogWarning($"StatType {type}이(가) 기본 스탯에 없습니다.");
        return 0f;
    }
}