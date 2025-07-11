using Photon.Pun;
using UnityEngine;

public interface IWeaponState
{
    void EnterState(AttackHandler handler);
    void ExecuteAttack(AttackHandler handler);
    void ExitState(AttackHandler handler);
}


public class HandState : IWeaponState
{
    private static readonly int _Trigger = Animator.StringToHash("HandAttackTrigger");
    
    public void EnterState(AttackHandler handler)
    {

    }

    public void ExecuteAttack(AttackHandler handler)
    {
        // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
        //     return;
        Debug.Log("hand attack");

        handler._Animator.SetTrigger(_Trigger);

        Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var dmgable))
            {
                int viewID = col.GetComponent<PhotonView>().ViewID;
                float dmg = handler._Stat.GetBaseStat(StatType.AttackPower);
                // 데미지
                handler.photonView.RPC(
                    nameof(handler.RPC_DealDamage),
                    RpcTarget.All,
                    viewID,
                    dmg);
                
                // 스턴
                handler.photonView.RPC(
                    nameof(handler.RPC_ApplyStatus),
                    RpcTarget.All,
                    viewID,
                    (int)StatusType.Stun,
                    handler._AttackStunDuration
                );
                break;
            }
        }
    }

    public void ExitState(AttackHandler handler) { }

}

public class AxeState : IWeaponState
{
    private static readonly int _Trigger = Animator.StringToHash("AxeAttackTrigger");

    public void EnterState(AttackHandler handler)
    {
    }

    public void ExecuteAttack(AttackHandler handler)
    {
        // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
        //     return;
        Debug.Log("axe attack");

        handler._Animator.SetTrigger(_Trigger);


        Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<IChoppable>(out var harvestable))
            {
                int viewID = col.GetComponent<PhotonView>().ViewID;
                float power = handler._Stat.GetBaseStat(StatType.AxePower);

                handler.photonView.RPC(
                    nameof(handler.RPC_HarvestChoppableResource),
                    RpcTarget.All,
                    viewID,
                    power);
                break;
            }
        }            
    }

    public void ExitState(AttackHandler handler) { }

}

public class PickaxeState : IWeaponState
{
    private static readonly int _Trigger = Animator.StringToHash("PickaxeAttackTrigger");
    public void EnterState(AttackHandler handler)
    {
        
    }

    public void ExecuteAttack(AttackHandler handler)
    {
        // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
        //     return;
        Debug.Log("pickaxe attack");

        handler._Animator.SetTrigger(_Trigger);

        Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<IMineable>(out var harvestable))
            {
                int viewID = col.GetComponent<PhotonView>().ViewID;
                float dmg = handler._Stat.GetBaseStat(StatType.PickaxePower);

                handler.photonView.RPC(
                    nameof(handler.RPC_HarvestMineableResource),
                    RpcTarget.All,
                    viewID,
                    dmg);
                break;
            }
        }
    }

    public void ExitState(AttackHandler handler) { }
    
}