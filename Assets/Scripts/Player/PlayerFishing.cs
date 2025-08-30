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
        FishingManager.OnCastStarted += HandleCastStart;
        FishingManager.OnFishingSucceeded += HandleFishingSuccess;
        FishingManager.OnFishingFailed += HandleFishingFail;
    }

    void OnDisable()
    {

        FishingManager.OnCastStarted -= HandleCastStart;
        FishingManager.OnFishingSucceeded -= HandleFishingSuccess;
        FishingManager.OnFishingFailed -= HandleFishingFail;
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
