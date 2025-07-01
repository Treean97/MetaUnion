using UnityEngine;

public enum StatusType { Stun, Slow, Poison /*…*/ }

public interface IStatusEffect
{
    StatusType _Type { get; }
    void Apply(GameObject target);
    void UpdateEffect(GameObject target, float deltaTime);
    void Remove(GameObject target);
}


public class StunEffect : IStatusEffect
{
    public StatusType _Type => StatusType.Stun;

    public void Apply(GameObject target)
    {
        Debug.Log("상태이상 : 기절");
    }

    public void UpdateEffect(GameObject target, float deltaTime)
    {

    }

    public void Remove(GameObject target)
    {
        Debug.Log("상태이상 : 기절 해제");
    }

}