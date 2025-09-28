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
        FishingSequence.OnFishingStart += HandleCastStart;
        FishingSequence.OnFishingSuccess += HandleFishingSuccess;
        FishingSequence.OnFishingFail += HandleFishingFail;
    }

    void OnDisable()
    {

        FishingSequence.OnFishingStart -= HandleCastStart;
        FishingSequence.OnFishingSuccess -= HandleFishingSuccess;
        FishingSequence.OnFishingFail -= HandleFishingFail;
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
