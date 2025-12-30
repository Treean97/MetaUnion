// AttackHandler.cs
using UnityEngine;
using Photon.Pun;
using Controller;
using System.Collections;
using System;

[RequireComponent(typeof(PlayerStat), typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class AttackHandler : MonoBehaviourPun, ILeftClickConsumer
{
    [Header("Animation")]
    internal Animator _Animator;
    internal PlayerInput _Input;
    internal PlayerStat _Stat;

    [Header("WeaponState")]
    public WeaponStateSO HandCfg;
    public WeaponStateSO AxeCfg;
    public WeaponStateSO PickaxeCfg;

    IWeaponState _State;
    private bool _CanAttack = true;

    public event Action OnAttackStart;
    public event Action OnAttackEnd;

    IDisposable _Token;

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

        // 로컬만 입력 관련 구독
        if (_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed += EquipHand;
            _Input.OnSlot_1KeyPressed += EquipAxe;
            _Input.OnSlot_2KeyPressed += EquipPickaxe;

            // 낚시 시작 시 손 장착 유지
            FishingSequence.OnFishingStart += HandleFishingStart;
        }
    }

    void OnEnable()
    {
        // 로컬만 디스패처 등록
        if (!photonView.IsMine) return;

        // LeftClickDispatcher가 씬에 존재해야 함 (없으면 null 반환)
        _Token = LeftClickDispatcher._Inst?.Push(this);
    }

    void OnDisable()
    {
        _Token?.Dispose();
        _Token = null;
    }

    void Start()
    {
        Equip(HandCfg);
    }

    void OnDestroy()
    {
        if (_Input && photonView.IsMine)
        {
            _Input.OnSlot_0KeyPressed -= EquipHand;
            _Input.OnSlot_1KeyPressed -= EquipAxe;
            _Input.OnSlot_2KeyPressed -= EquipPickaxe;

            // _Input.OnAttack -= HandleAttackInput;

            FishingSequence.OnFishingStart -= HandleFishingStart;
        }
    }

    // 좌클릭 소비
    public bool ConsumeLeftClick()
    {
        // PlayerInput에서 이미 "UI 위 클릭 / Attack 락"을 걸러서 Dispatch를 호출하지만,
        // 여기서도 안전하게 한 번 더 체크해도 됨.
        if (!photonView.IsMine) return false;
        if (InputBlockManager.IsLocked(InputLock.Attack)) return false;

        if (!_CanAttack) return false;
        if (_State == null) return false;

        HandleAttackInput();
        return true; // 소비
    }

    private void HandleAttackInput()
    {
        if (!_CanAttack)
            return;

        _CanAttack = false;
        _State?.ExecuteAttack(this);
    }

    public void AnimEvent_AttackStart()
    {
        OnAttackStart?.Invoke();
    }

    public void AnimEvent_AttackEnd()
    {
        OnAttackEnd?.Invoke();
        _CanAttack = true;
    }

    void HandleFishingStart()
    {
        Equip(HandCfg);
    }

    internal IEnumerator ResetAttackFlag(float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
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
            mgr.AddEffect((StatusType)statusType, duration);
        }
    }
}
