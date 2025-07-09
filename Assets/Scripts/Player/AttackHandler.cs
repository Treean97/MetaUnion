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
    [SerializeField] internal Transform _AttackPoint;
    [SerializeField] internal float _AttackRadius = 1f;

    [Header("Stun")]
    [SerializeField] internal float _AttackStunDuration = 1f;

    [Header("Animation")]
    [SerializeField] internal AnimationClip _AttackClip;
    internal Animator _Animator;    
    internal PlayerInput _Input;
    internal PlayerStat  _Stat;
    

    
    private IWeaponState _CurrentState;

    // 상태 전환 메서드
    public void ChangeState(IWeaponState newState)
    {
        _CurrentState?.ExitState(this);
        _CurrentState = newState;
        _CurrentState.EnterState(this);

        Debug.Log($"ChageState : {_CurrentState}");
    }

    void OnEnable()
    {
        _Input.OnAttack += () => _CurrentState?.ExecuteAttack(this);
        _Input.OnWeaponChange += ChangeState;
    }
    void OnDisable()
    {
        _Input.OnAttack -= () => _CurrentState?.ExecuteAttack(this);
        _Input.OnWeaponChange -= ChangeState;
    }

    void Awake()
    {
        _Input = GetComponent<PlayerInput>();
        _Stat = GetComponent<PlayerStat>();
        _Animator = GetComponent<Animator>();
    }

    void Start()
    {
        var handState = new HandState();
        ChangeState(handState);
    }

    internal IEnumerator ResetAttackFlag(float animationClipLength)
    {
        yield return new WaitForSeconds(animationClipLength);
        _Animator.SetBool("IsAttack", false);
    }

    [PunRPC]
    internal void RPC_DealDamage(int viewID, float dmg)
    {
        var pv = PhotonView.Find(viewID);
        pv?.GetComponent<IDamageable>()?.Damaged(dmg);
    }

    [PunRPC]
    internal void RPC_HarvestChoppableResource(int viewID, float power)
    {
        var pv = PhotonView.Find(viewID);
        pv?.GetComponent<IChoppable>()?.Chop(power);
    }

    [PunRPC]
    internal void RPC_HarvestMineableResource(int viewID, float power)
    {
        var pv = PhotonView.Find(viewID);
        pv?.GetComponent<IMineable>()?.Mine(power);
    }
    
    [PunRPC]
    internal void RPC_ApplyStatus(int viewID, int statusType, float duration)
    {
        var pv = PhotonView.Find(viewID);
        if (pv != null && pv.TryGetComponent<StatusEffectManager>(out var mgr))
        {
            // StunEffect 생성 후 적용
            mgr.AddEffect((StatusType)statusType, duration);
        }
    }

}
