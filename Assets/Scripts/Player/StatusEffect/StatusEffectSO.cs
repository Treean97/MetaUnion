using UnityEngine;


public abstract class StatusEffectSO : ScriptableObject
{
    public StatusType type;
    public abstract IStatusEffect CreateEffect();
}

[CreateAssetMenu(menuName = "StatusEffect/Stun")]
public class StunEffectSO : StatusEffectSO
{
    public override IStatusEffect CreateEffect()
    {
        return new StunEffect();
    }
}
