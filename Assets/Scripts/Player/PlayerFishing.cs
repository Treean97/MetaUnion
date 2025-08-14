using UnityEngine;

public class PlayerFishing : MonoBehaviour
{
    private Animator _Animator;

    void Awake()
    {
        _Animator = gameObject.GetComponent<Animator>();
    }

    void OnEnable()
    {
        FishingSequence.OnCastStarted += HandleCastStart;
        FishingSequence.OnFishingSucceeded += HandleFishingSuccess;
        FishingSequence.OnFishingFailed += HandleFishingFail;
    }

    void OnDisable()
    {

        FishingSequence.OnCastStarted -= HandleCastStart;
        FishingSequence.OnFishingSucceeded -= HandleFishingSuccess;
        FishingSequence.OnFishingFailed -= HandleFishingFail;
    }

    void HandleCastStart()
    {
        _Animator.SetTrigger("FishingTrigger");
    }

    void HandleFishingSuccess()
    {
        _Animator.SetTrigger("FishingSuccessTrigger");
    }

    void HandleFishingFail()
    {
        _Animator.SetTrigger("FishingFailTrigger");
    }

}
