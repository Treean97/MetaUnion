using UnityEngine;

public abstract class BuffEffectSO : ScriptableObject, IBuffEffect
{
    public abstract void Apply(PlayerStat playerStat, BuffDataSO buffData);
}
