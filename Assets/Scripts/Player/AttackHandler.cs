// AttackHandler.cs
using UnityEngine;
using Photon.Pun;
using Controller;
using System.Collections;

[RequireComponent(typeof(PlayerStat), typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class AttackHandler : MonoBehaviourPun
{
    [Header("Attack Point")]
    [SerializeField] private Transform _AttackPoint;
    [SerializeField] private float _AttackRadius = 1f;

    [Header("Stun")]
    [SerializeField] private float _AttackStunDuration = 1f;

    [Header("Animation")]
    [SerializeField] AnimationClip _AttackClip;

    
    private PlayerInput _Input;
    private PlayerStat  _Stat;
    private Animator _Animator;
    private int _AttackLayerIndex;

    void Awake()
    {
        _Input = GetComponent<PlayerInput>();
        _Stat = GetComponent<PlayerStat>();
        _Animator = GetComponent<Animator>();
        _AttackLayerIndex = _Animator.GetLayerIndex("Attack");
    }

    void OnEnable()
    {
        _Input.OnAttack += ExecuteAttack;
    }

    void OnDisable()
    {
        _Input.OnAttack -= ExecuteAttack;
    }

    private void ExecuteAttack()
    {
        var stateInfo = _Animator.GetCurrentAnimatorStateInfo(_AttackLayerIndex);
        var clips = _Animator.GetCurrentAnimatorClipInfo(_AttackLayerIndex);
        if (clips.Length > 0 && clips[0].clip == _AttackClip && stateInfo.normalizedTime < 1f)
        {
            return;
        }

        // 애니메이션 발동
        _Animator.SetBool("IsAttack", true);

        Collider[] hits = Physics.OverlapSphere(_AttackPoint.position, _AttackRadius);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var dmgable))
            {
                int viewID = col.GetComponent<PhotonView>().ViewID;
                float dmg  = _Stat.GetBaseStat(StatType.AttackPower);

                // 데미지 RPC
                photonView.RPC(
                    nameof(RPC_DealDamage),
                    RpcTarget.All,
                    viewID,
                    dmg
                );

                // **기절 RPC** (StatusType.Stun, 지속시간 전달)
                photonView.RPC(
                    nameof(RPC_ApplyStatus),
                    RpcTarget.All,
                    viewID,
                    (int)StatusType.Stun,
                    _AttackStunDuration
                );

                break;  // 한 명만 공격
            }
        }

        StartCoroutine(ResetAttackFlag(_AttackClip.length));
    }

    IEnumerator ResetAttackFlag(float animationClipLength)
    {
        yield return new WaitForSeconds(animationClipLength);
        _Animator.SetBool("IsAttack", false);
    }

    [PunRPC]
    void RPC_DealDamage(int viewID, float dmg)
    {
        var pv = PhotonView.Find(viewID);
        pv?.GetComponent<IDamageable>()?.Damaged(dmg);
    }
    
    [PunRPC]
    void RPC_ApplyStatus(int viewID, int statusType, float duration)
    {
        var pv = PhotonView.Find(viewID);
        if (pv != null && pv.TryGetComponent<StatusEffectManager>(out var mgr))
        {
            // StunEffect 생성 후 적용
            mgr.AddEffect((StatusType)statusType, duration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_AttackPoint.position, _AttackRadius);
    }
}
