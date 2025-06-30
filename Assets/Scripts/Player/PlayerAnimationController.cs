using UnityEngine;
using System.Collections;
using Photon.Pun;
using Controller;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator _Animator;
    [SerializeField] private AnimationClip _AttackClip;
    [SerializeField] private AnimationClip _HitClip;

    private static readonly int _IsAttack = Animator.StringToHash("IsAttack");
    private static readonly int _IsHit = Animator.StringToHash("IsHit");

    private Coroutine _AttackResetCoroutine;
    private Coroutine _HitResetCoroutine;

    public bool IsAttacking => _Animator.GetBool(_IsAttack);
    public bool IsHit => _Animator.GetBool(_IsHit);

    public void PlayAttack()
    {
        if (_AttackResetCoroutine != null)
            StopCoroutine(_AttackResetCoroutine);

        _Animator.SetBool(_IsAttack, true);
        _AttackResetCoroutine = StartCoroutine(ResetBoolAfter(_IsAttack, _AttackClip.length));
    }

    public void PlayHit()
    {
        if (_HitResetCoroutine != null)
            StopCoroutine(_HitResetCoroutine);

        _Animator.SetBool(_IsHit, true);
        _HitResetCoroutine = StartCoroutine(ResetBoolAfter(_IsHit, _HitClip.length));
    }

    private IEnumerator ResetBoolAfter(int hash, float time)
    {
        yield return new WaitForSeconds(time);
        _Animator.SetBool(hash, false);
    }
}
