// AttackHandler.cs
using UnityEngine;
using Photon.Pun;
using Controller;
using System.Collections;

[RequireComponent(typeof(PlayerStat), typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class AttackHandler : MonoBehaviourPun
{        
    [Header("Animation")]
    internal Animator _Animator;    
    internal PlayerInput _Input;
    internal PlayerStat  _Stat;

    [Header("WeaponState")]
    public WeaponStateSO HandCfg;
    public WeaponStateSO AxeCfg;
    public WeaponStateSO PickaxeCfg;

    IWeaponState _State;
    private bool _CanAttack = true;

    public void Equip(WeaponStateSO cfg)
    {
        _State?.ExitState(this);
        _State = new MeleeToolState(cfg);
        _State.EnterState(this);
    }

    public void EquipHand() => Equip(HandCfg);
    public void EquipAxe() => Equip(AxeCfg);   
    public void EquipPickaxe() => Equip(PickaxeCfg);

    void Awake()
    {
        _Input = GetComponent<PlayerInput>();
        _Stat = GetComponent<PlayerStat>();
        _Animator = GetComponent<Animator>();

        if(_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed += EquipHand;
            _Input.OnSlot_1KeyPressed += EquipAxe;
            _Input.OnSlot_2KeyPressed += EquipPickaxe;
        }
        
    }

    void Start()
    {
        Equip(HandCfg);
    }

    void OnEnable()
    {
        _Input.OnAttack += HandleAttackEvent;
    }
    void OnDisable()
    {
        _Input.OnAttack -= HandleAttackEvent;
    }

    void OnDestroy()
    {
        if(_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed -= EquipHand;
            _Input.OnSlot_1KeyPressed -= EquipAxe;
            _Input.OnSlot_2KeyPressed -= EquipPickaxe;
        }
    }

    private void HandleAttackEvent()
    {
        if (!_CanAttack)
            return;

        // 입력이 허용될 때만 실행
        _CanAttack = false;
        _State?.ExecuteAttack(this);
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
    public void RPC_TryDamage(int viewID, byte tool, float power, Vector3 hitPos)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var pv = PhotonNetwork.GetPhotonView(viewID);
        if (!pv) return;

        if (!pv.TryGetComponent<IDamageable>(out var target)) return;

        var info = new DamageInfo { damage = power, tool = (DamageTool)tool };
        target.Damaged(info);
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
