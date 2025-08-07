using UnityEngine;

[CreateAssetMenu (menuName = "BuffEffects/JumpBoostEffect")]
public class JumpBoostEffectSO : BuffEffectSO
{
    public override void Apply(PlayerStat playerStat, BuffDataSO buffData)
    {
        var mod = new StatModifier
        {
            Type = StatType.JumpPower,
            AddValue = buffData.BuffValueType == BuffValueType.Add ? buffData.Value : 0,
            MulFactor = buffData.BuffValueType == BuffValueType.Multiple ? buffData.Value : 1,
            Duration = buffData.Duration
        };
        playerStat.AddModifier(mod);
    }
}
