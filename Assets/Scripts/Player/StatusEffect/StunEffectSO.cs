using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffect/Stun")]
public class StunEffectSO : StatusEffectSO
{
    public override IStatusEffect CreateEffect()
    {
        return new StunEffect();        
    }
}
