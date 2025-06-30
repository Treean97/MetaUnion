using System.Collections;
using UnityEngine;

/// <summary>
/// 피격 가능한 객체에 붙여 체력 관리 및 대미지 처리
/// </summary>
[RequireComponent(typeof(PlayerStat))]
[RequireComponent(typeof(Animator))]
public class HealthHandler : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    private PlayerStat _PlayerStat;
    private float _CurHealth;

    [Header("Animation")]
    [SerializeField] AnimationClip _HitClip;
    private Animator _Animator;

    private void Awake()
    {
        _PlayerStat = GetComponent<PlayerStat>();
        _CurHealth = _PlayerStat.GetBaseStat(StatType.Health);
        _Animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 대미지를 입을 때 호출
    /// </summary>
    /// <param name="amount">입힐 대미지</param>
    public void Damaged(float amount)
    {
        _CurHealth = Mathf.Max(_CurHealth - amount, 0f);
        OnDamaged();

        if (_CurHealth <= 0f)
        {
            Die();
        }
            
    }

    /// <summary>
    /// 대미지 직후 처리 (이펙트, 사운드 등)
    /// </summary>
    public void OnDamaged()
    {
        // 예: 피격 이펙트, 애니메이션 트리거
        Debug.Log($"{gameObject.name} took damage. Remaining health: {_CurHealth}");

        _Animator.SetBool("IsHit", true);
        StartCoroutine(ResetHitFlag(_HitClip.length));
    }
    
    IEnumerator ResetHitFlag(float animationClipLength)
    {
        yield return new WaitForSeconds(animationClipLength);
        _Animator.SetBool("IsHit", false);
    }

    /// <summary>
    /// 체력 0 이하 시 사망 처리
    /// </summary>
    private void Die()
    {
        // 예: 사망 애니메이션
        Debug.Log($"{gameObject.name} died.");
    }
}
