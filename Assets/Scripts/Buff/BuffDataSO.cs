using UnityEngine;

public enum BuffEffectType
{
    SpeedUp,
    JumpUp
}

public enum BuffValueType
{
    Add,
    Multiple
}

[CreateAssetMenu(menuName = "Buff/BuffData")]
public class BuffDataSO : ScriptableObject
{
    [SerializeField] BuffEffectType _BuffeffectType;
    public BuffEffectType BuffEffectType => _BuffeffectType;

    [SerializeField] BuffValueType _BuffValueType;
    public BuffValueType BuffValueType => _BuffValueType;
    [SerializeField] private float _Value;
    public float Value => _Value;
    [SerializeField] private float _Duration;
    public float Duration => _Duration;

    [SerializeField] Sprite _Icon;
    public Sprite Icon => _Icon;

    [SerializeField] BuffEffectSO _Effect;
    public BuffEffectSO Effect => _Effect;
}
