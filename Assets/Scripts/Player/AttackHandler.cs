// AttackHandler.cs
using UnityEngine;
using Photon.Pun;
using Controller;

[RequireComponent(typeof(PlayerStat), typeof(PhotonView))]
public class AttackHandler : MonoBehaviourPun
{
    [SerializeField] private Transform _AttackPoint;
    [SerializeField] private float     _AttackRadius = 1f;


    private PlayerInput _Input;
    private PlayerStat  _Stat;

    void Awake()
    {
        _Input = GetComponent<PlayerInput>();
        _Stat  = GetComponent<PlayerStat>();
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
