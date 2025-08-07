using UnityEngine;
using System.Collections.Generic;

public class PlayerStat : MonoBehaviour
{
    [Header("Default Stats")]
    [SerializeField] private PlayerStatsSO _StatsSO;
    // StatType → BaseValue 매핑
    private Dictionary<StatType, float> _BaseStats;    
    
    private List<StatModifier> _Modifiers = new();

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
        _Modifiers.RemoveAll(m => m.Duration > 0 && now >= m.ExpireTime);
    }

    public void AddModifier(StatModifier mod)
    {
        // 새로 추가
        if (mod.Duration > 0)
        {
            mod.ExpireTime = Time.time + mod.Duration;
        }
        
        _Modifiers.Add(mod);
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


    private void HandleMoveSpeedBuff(float value, float duration)
    {
        var mod = new StatModifier
        {
            Type = StatType.MoveSpeed,
            AddValue = 0f,
            MulFactor = value,
            Duration = duration
        };
        AddModifier(mod);


        Debug.Log("Speed Up");
    }

    private void HandleJumpBoostBuff(float value, float duration)
    {
        var mod = new StatModifier
        {
            Type = StatType.JumpPower,
            AddValue = 0f,
            MulFactor = value,
            Duration = duration
        };
        AddModifier(mod);
        
        Debug.Log("Jump Boost");
    }
    
}