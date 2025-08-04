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
    internal Animator _Animator;    
    internal PlayerInput _Input;
    internal PlayerStat  _Stat;

    private bool _CanAttack = true;
    private IWeaponState _CurrentState;

    // 상태 전환 메서드
    public void ChangeState(IWeaponState newState)
    {
        _CurrentState?.ExitState(this);
        _CurrentState = newState;
        _CurrentState.EnterState(this);

        Debug.Log($"ChageState : {_CurrentState}");
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

    void OnEnable()
    {
        _Input.OnAttack += HandleAttackEvent;
        _Input.OnWeaponChange += ChangeState;
    }
    void OnDisable()
    {
        _Input.OnAttack -= HandleAttackEvent;
        _Input.OnWeaponChange -= ChangeState;
    }


     private void HandleAttackEvent()
    {
        if (!_CanAttack) 
            return;

        // 입력이 허용될 때만 실행
        _CanAttack = false;
        _CurrentState?.ExecuteAttack(this);
    }

    internal IEnumerator ResetAttackFlag(float delay, System.Action onComplete)
    {        
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
        _CanAttack = true;
    }

    public void OnAttackFinished()
    {
        Debug.Log("Attack Finished");
        _CanAttack = true;
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
