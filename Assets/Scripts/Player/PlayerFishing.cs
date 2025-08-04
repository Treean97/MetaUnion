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
        GameEvents.OnRequestOpenFishingUI += HandleStartFishing;
        FishingUIManager.OnFishingSuccess += HandleFishingSuccess;
        FishingUIManager.OnFishingFail += HandleFishingFail;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenFishingUI -= HandleStartFishing;
        FishingUIManager.OnFishingSuccess -= HandleFishingSuccess;
        FishingUIManager.OnFishingFail -= HandleFishingFail;
    }

    void HandleStartFishing()
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
