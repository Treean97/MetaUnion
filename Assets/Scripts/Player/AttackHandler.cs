// AttackHandler.cs
using UnityEngine;
using Photon.Pun;
using Controller;
using System.Collections;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerStat), typeof(PhotonView))]
[RequireComponent(typeof(Animator))]
public class AttackHandler : MonoBehaviourPun
{
    [Serializable]
    public struct ClipEntry
    {
        public string Name;
        public AnimationClip Clip;
    }

    [Header("Attack Point")]
    [SerializeField] internal Transform _AttackPoint;
    [SerializeField] internal float _AttackRadius = 1f;

    [Header("Stun")]
    [SerializeField] internal float _AttackStunDuration = 1f;
        
    [Header("Animation")]
    [SerializeField]
    private List<ClipEntry> _AttackClips = new List<ClipEntry>();

    // 런타임에 사용될 딕셔너리
    private Dictionary<string, AnimationClip> _ClipDict;
    internal Animator _Animator;    
    internal PlayerInput _Input;
    internal PlayerStat  _Stat;

    private bool _CanAttack = true;

    internal void HandleAttackInput()
    {
        if (!_CanAttack) return;
        _CanAttack = false;
        _CurrentState?.ExecuteAttack(this);
    }
    
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

        BuildClipDictionary();
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

    private void BuildClipDictionary()
    {
        _ClipDict = new Dictionary<string, AnimationClip>(StringComparer.Ordinal);
        foreach (var entry in _AttackClips)
        {
            if (string.IsNullOrEmpty(entry.Name) || entry.Clip == null)
            {
                Debug.LogWarning($"[AttackHandler] 잘못된 ClipEntry: Name='{entry.Name}', Clip={(entry.Clip == null ? "null" : entry.Clip.name)}");
                continue;
            }
            _ClipDict[entry.Name] = entry.Clip;
        }
    }

     public AnimationClip GetClip(string key)
    {
        if (_ClipDict != null && _ClipDict.TryGetValue(key, out var clip))
            return clip;

        Debug.LogError($"[AttackHandler] Clip '{key}'을(를) 찾을 수 없습니다!");
        return null;
    }


    internal IEnumerator ResetAttackFlag(float delay, System.Action onComplete)
    {        
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
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
