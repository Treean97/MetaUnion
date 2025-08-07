using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStat : MonoBehaviour
{
    [Header("Default Stats")]
    [SerializeField] private PlayerStatsSO _StatsSO;
    // StatType → BaseValue 매핑
    private Dictionary<StatType, float> _BaseStats;

    private List<StatModifier> _Modifiers = new();

    public static event Action<StatType, float> OnStatChanged;
    public static event Action<StatType, float> OnBuffAdded;

    private void Awake()
    {
        // ScriptableObject로 정의된 기본 스탯값 로드
        _BaseStats = new Dictionary<StatType, float>();
        foreach (var entry in _StatsSO.DefaultStats)
        {
            _BaseStats[entry.Type] = entry.BaseValue;
        }
    }

    void OnEnable()
    {
        GameEvents.OnRequestMoveSpeedBuff += HandleMoveSpeedBuff;
        GameEvents.OnRequestJumpBoostBuff += HandleJumpBoostBuff;
    }

    void OnDisable()
    {
        GameEvents.OnRequestMoveSpeedBuff -= HandleMoveSpeedBuff;
        GameEvents.OnRequestJumpBoostBuff -= HandleJumpBoostBuff;
    }

    void Update()
    {
        float now = Time.time;

        for (int i = 0; i < _Modifiers.Count; i++)
        {
            var m = _Modifiers[i];
            if (m.Duration > 0 && now >= m.ExpireTime)
            {
                _Modifiers.RemoveAt(i);
                OnStatChanged?.Invoke(m.Type, GetStat(m.Type));
            }
        }
    }

    public void AddModifier(StatModifier mod)
    {
        // 새로 추가
        if (mod.Duration > 0)
        {
            mod.ExpireTime = Time.time + mod.Duration;
        }

        _Modifiers.Add(mod);

        OnStatChanged?.Invoke(mod.Type, GetStat(mod.Type));
        OnBuffAdded?.Invoke(mod.Type, mod.Duration);
    }

    public float GetStat(StatType type)
    {
        float baseValue = GetBaseStat(type);
        float sumAdd = 0f;
        float mulProd = 1f;

        foreach (var m in _Modifiers)
        {
            if (m.Type != type)
                continue;

            sumAdd += m.AddValue;
            mulProd *= m.MulFactor;
        }

        return (baseValue + sumAdd) * mulProd;
    }


    public float GetBaseStat(StatType type)
    {
        if (_BaseStats.TryGetValue(type, out var value))
            return value;
        Debug.LogWarning($"StatType {type}이(가) 기본 스탯에 없습니다.");
        return 0f;
    }

    #region Potion Effect

    private void HandleMoveSpeedBuff(PotionValueType type, float value, float duration)
    {
        

        var mod1 = new StatModifier
        {
            Type = StatType.MoveSpeed,
            AddValue = type == PotionValueType.Add ? value : 0,    
            MulFactor = type == PotionValueType.Multiple ? value : 1,
            Duration = duration
        };
        AddModifier(mod1);

        var mod2 = new StatModifier
        {
            Type = StatType.RunSpeed,
            AddValue = type == PotionValueType.Add ? value : 0,    
            MulFactor = type == PotionValueType.Multiple ? value : 1,
            Duration = duration
        };
        AddModifier(mod2);

        Debug.Log($"Speed Up / {GetStat(StatType.MoveSpeed)}, {GetStat(StatType.RunSpeed)}");
    }

    private void HandleJumpBoostBuff(PotionValueType type, float value, float duration)
    {
        var mod = new StatModifier
        {
            Type = StatType.JumpPower,
            AddValue = type == PotionValueType.Add ? value : 0,    
            MulFactor = type == PotionValueType.Multiple ? value : 1,
            Duration = duration
        };
        AddModifier(mod);

        Debug.Log($"Jump Boost / {GetStat(StatType.JumpPower)}");
    }
    
    #endregion
}