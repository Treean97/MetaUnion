using System.Collections;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 피격 가능한 객체에 붙여 체력 관리 및 대미지 처리
/// </summary>
[RequireComponent(typeof(PlayerStat))]
[RequireComponent(typeof(Animator))]
public class HealthHandler : MonoBehaviourPun, IDamageable
{
    [Header("Animation")]
    [SerializeField] AnimationClip _HitClip;
    private Animator _Animator;

    private void Awake()
    {
        _Animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 대미지를 입을 때 호출
    /// </summary>
    /// <param name="amount">입힐 대미지</param>
    public void Damaged(float amount)
    {
        OnDamaged();
    }

    void OnDamaged()
    {
        // 피격 이펙트, 애니메이션 트리거
        if (!photonView.IsMine)
        {
            return;
        }
        _Animator.SetTrigger("HitTigger");
    }
    

}
