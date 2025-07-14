using UnityEngine;

public abstract class StatusEffectSO : ScriptableObject
{
    public StatusType type;
    public abstract IStatusEffect CreateEffect();
}