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
    public void EnterState(AttackHandler handler)
    {
        // 전환 시 할 일 (ex. 무기 변경, 애니메이션 컨트롤러 변경 등)
    }

    public void ExecuteAttack(AttackHandler handler)
    {
        var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        if (aniInfo.IsName(handler._AttackClip.name) && aniInfo.normalizedTime < 1f)
            return;

        handler._Animator.Play(handler._AttackClip.name);

        Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var dmgable))
            {
                int viewID = col.GetComponent<PhotonView>().ViewID;
                float dmg = handler._Stat.GetBaseStat(StatType.AttackPower);

                handler.photonView.RPC(nameof(handler.RPC_DealDamage), RpcTarget.All, viewID, dmg);
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

        handler.StartCoroutine(handler.ResetAttackFlag(handler._AttackClip.length));
    }

    public void ExitState(AttackHandler handler) { }

}

public class AxeState : IWeaponState
{
    public void EnterState(AttackHandler handler)
    {
        // 전환 시 할 일 (ex. 무기 변경, 애니메이션 컨트롤러 변경 등)
    }

    public void ExecuteAttack(AttackHandler handler)
    {
        var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        if (aniInfo.IsName(handler._AttackClip.name) && aniInfo.normalizedTime < 1f)
            return;

        handler._Animator.Play(handler._AttackClip.name);

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

        handler.StartCoroutine(handler.ResetAttackFlag(handler._AttackClip.length));
    }

    public void ExitState(AttackHandler handler) { }

}

public class PickaxeState : IWeaponState
{
    public void EnterState(AttackHandler handler)
    {
        // 전환 시 할 일 (ex. 무기 변경, 애니메이션 컨트롤러 변경 등)
    }

    public void ExecuteAttack(AttackHandler handler)
    {
        var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
        if (aniInfo.IsName(handler._AttackClip.name) && aniInfo.normalizedTime < 1f)
            return;

        handler._Animator.Play(handler._AttackClip.name);

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
        
        handler.StartCoroutine(handler.ResetAttackFlag(handler._AttackClip.length));
    }

    public void ExitState(AttackHandler handler) { }
    
}