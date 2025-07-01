using UnityEngine;
using System;
using System.Collections.Generic;

public class StatusEffectManager : MonoBehaviour
{
    [Header("모든 StatusEffect SO 등록")]
    [SerializeField] private StatusEffectSO[] _AllEffects;

    // 타입→SO 매핑
    private Dictionary<StatusType, StatusEffectSO> _SOMap;

    private struct EffectState
    {
        public IStatusEffect Effect;
        public float         Remaining;
    }
    private readonly List<EffectState> _ActiveEffects = new();

    // 범용 이벤트 (필요시 구독)
    public event Action<StatusType> OnEffectApplied;
    public event Action<StatusType> OnEffectRemoved;

    void Awake()
    {
        _SOMap = new Dictionary<StatusType, StatusEffectSO>();
        foreach (var so in _AllEffects)
            _SOMap[so.type] = so;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = _ActiveEffects.Count - 1; i >= 0; i--)
        {
            var state = _ActiveEffects[i];
            state.Effect.UpdateEffect(gameObject, dt);
            state.Remaining -= dt;

            if (state.Remaining <= 0f)
            {
                OnEffectRemoved?.Invoke(state.Effect._Type);
                state.Effect.Remove(gameObject);
                _ActiveEffects.RemoveAt(i);
            }
            else
            {
                _ActiveEffects[i] = state;
            }
        }
    }

    /// <summary>
    /// 외부에서 타입과 지속시간을 주입해서 상태이상 적용
    /// </summary>
    public void AddEffect(StatusType type, float duration)
    {
        if (!_SOMap.TryGetValue(type, out var so))
        {
            Debug.LogWarning($"StatusEffectManager: 알 수 없는 타입 {type}");
            return;
        }

        // 이미 존재하면 남은 시간만 연장
        int idx = _ActiveEffects.FindIndex(s => s.Effect._Type == type);
        if (idx >= 0)
        {
            var exist = _ActiveEffects[idx];
            exist.Remaining = Mathf.Max(exist.Remaining, duration);
            _ActiveEffects[idx] = exist;
            return;
        }

        // 신규 적용
        var effect = so.CreateEffect();
        effect.Apply(gameObject);
        _ActiveEffects.Add(new EffectState { Effect = effect, Remaining = duration });
        OnEffectApplied?.Invoke(type);
    }

    public bool IsActive(StatusType type)
        => _ActiveEffects.Exists(s => s.Effect._Type == type);
}
