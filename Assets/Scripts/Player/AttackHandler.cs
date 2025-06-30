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

        // OverlapSphere로 전방 원형 범위 내 적 탐지
        Collider[] hits = Physics.OverlapSphere(
            _AttackPoint.position,
            _AttackRadius
        );
        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var atk))
            {
                float dmg = _Stat.GetBaseStat(StatType.AttackPower);

                photonView.RPC(nameof(RPC_DealDamage),
                               RpcTarget.All,
                               col.GetComponent<PhotonView>().ViewID,
                               dmg);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_AttackPoint.position, _AttackRadius);
    }
}
