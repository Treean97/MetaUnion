using Photon.Pun;
using UnityEngine;

public interface IWeaponState
{
    void EnterState(AttackHandler handler);
    void ExecuteAttack(AttackHandler handler);
    void ExitState(AttackHandler handler);
}

public sealed class MeleeToolState : IWeaponState
{
    readonly WeaponStateSO _cfg;
    int _trigger;

    public MeleeToolState(WeaponStateSO cfg)
    {
        _cfg = cfg;
        _trigger = Animator.StringToHash(_cfg.AniTriggerName);
    }

    public void EnterState(AttackHandler h)
    {
        Debug.Log($"상태 변경 : {_cfg.Tool}");
    }
     

    public void ExecuteAttack(AttackHandler h)
    {
        if (!h.photonView.IsMine) return;

        h._Animator.SetTrigger(_trigger);

        var attackPoint = h.transform.TransformPoint(_cfg.AttackOffset);
        var hits = Physics.OverlapSphere(attackPoint, _cfg.Radius /*, _cfg.LayerMask*/);

        foreach (var col in hits)
        {
            // 내 자식은 스킵
            if (col.transform.IsChildOf(h.transform)) continue;

            // 부모에서 찾기(자식 콜라이더를 맞아도 OK)
            var pv = col.GetComponentInParent<PhotonView>();
            var dmg = col.GetComponentInParent<IDamageable>();
            if (pv == null || dmg == null) continue;

            // 데미지 계산
            float baseDmg = h._Stat.GetStat(_cfg.DamageStat);
            float varPct = h._Stat.GetStat(StatType.DamageVariance);
            float finalDmg = Random.Range(baseDmg * ((100 - varPct) / 100f), baseDmg);

            var pos = col.bounds.center;

            // 서버에 적용 요청(툴 포함)
            h.photonView.RPC(nameof(AttackHandler.RPC_TryDamage),
                             RpcTarget.MasterClient,
                             pv.ViewID, (byte)_cfg.Tool, finalDmg, pos);

            if (_cfg.ApplyStatus)
            {
                h.photonView.RPC(nameof(AttackHandler.RPC_ApplyStatus),
                                 RpcTarget.All, pv.ViewID, (int)_cfg.StatusType, _cfg.StatusDuration);
            }
            break; // 첫 타겟만
        }
    }

    public void ExitState(AttackHandler h) { }
}


// public class HandState : IWeaponState
// {
//     private static readonly int _Trigger = Animator.StringToHash("HandAttackTrigger");
    
//     public void EnterState(AttackHandler handler)
//     {

//     }

//     public void ExecuteAttack(AttackHandler handler)
//     {
//         // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
//         // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
//         //     return;
//         Debug.Log("hand attack");

//         handler._Animator.SetTrigger(_Trigger);

//         Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);
//         foreach (var col in hits)
//         {
//             if (col.TryGetComponent<IDamageable>(out var dmgable))
//             {
//                 int viewID = col.GetComponent<PhotonView>().ViewID;
//                 float dmg = handler._Stat.GetStat(StatType.AttackPower);
//                 // 데미지
//                 handler.photonView.RPC(
//                     nameof(handler.RPC_DealDamage),
//                     RpcTarget.All,
//                     viewID,
//                     dmg);
                
//                 // 스턴
//                 handler.photonView.RPC(
//                     nameof(handler.RPC_ApplyStatus),
//                     RpcTarget.All,
//                     viewID,
//                     (int)StatusType.Stun,
//                     handler._AttackStunDuration
//                 );
//                 break;
//             }
//         }
//     }

//     public void ExitState(AttackHandler handler) { }

// }

// public class AxeState : IWeaponState
// {
//     private static readonly int _Trigger = Animator.StringToHash("AxeAttackTrigger");

//     public void EnterState(AttackHandler handler)
//     {
//     }

//     public void ExecuteAttack(AttackHandler handler)
//     {
//         // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
//         // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
//         //     return;
//         Debug.Log("axe attack");

//         handler._Animator.SetTrigger(_Trigger);


//         Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);

//         foreach (var col in hits)
//         {
//             if (col.TryGetComponent<IDamageable>(out var harvestable))
//             {
//                 int viewID = col.GetComponent<PhotonView>().ViewID;
//                 float dmg = handler._Stat.GetStat(StatType.AxePower);

//                 handler.photonView.RPC(
//                     nameof(handler.RPC_DealDamage),
//                     RpcTarget.All,
//                     viewID,
//                     dmg);
//                 break;
//             }
//         }            
//     }

//     public void ExitState(AttackHandler handler) { }

// }

// public class PickaxeState : IWeaponState
// {
//     private static readonly int _Trigger = Animator.StringToHash("PickaxeAttackTrigger");
//     public void EnterState(AttackHandler handler)
//     {
        
//     }

//     public void ExecuteAttack(AttackHandler handler)
//     {
//         // var aniInfo = handler._Animator.GetCurrentAnimatorStateInfo(0);
//         // if (aniInfo.IsName(_AttackClip.name) && aniInfo.normalizedTime < 1f)
//         //     return;
//         Debug.Log("pickaxe attack");

//         handler._Animator.SetTrigger(_Trigger);

//         Collider[] hits = Physics.OverlapSphere(handler._AttackPoint.position, handler._AttackRadius);
//         foreach (var col in hits)
//         {
//             if (col.TryGetComponent<IDamageable>(out var harvestable))
//             {
//                 int viewID = col.GetComponent<PhotonView>().ViewID;
//                 float dmg = handler._Stat.GetStat(StatType.PickaxePower);

//                 handler.photonView.RPC(
//                     nameof(handler.RPC_DealDamage),
//                     RpcTarget.All,
//                     viewID,
//                     dmg);
//                 break;
//             }
//         }
//     }

//     public void ExitState(AttackHandler handler) { }
    
// }