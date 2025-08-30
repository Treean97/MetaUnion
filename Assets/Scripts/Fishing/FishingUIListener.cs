using UnityEngine;

public class FishingUIListener : MonoBehaviour
{
    [SerializeField] private FishingManager _FishingSequence;


    void OnEnable()
    {
        GameEvents.OnRequestOpenFishingUI += HandleOpenUI;
    }

    void OnDisable()
    {
        GameEvents.OnRequestOpenFishingUI -= HandleOpenUI;
    }


    void HandleOpenUI()
    {   
        _FishingSequence.gameObject.SetActive(true);
        _FishingSequence.StartFishing();

    }

}
