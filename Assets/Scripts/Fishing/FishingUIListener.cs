using UnityEngine;

public class FishingUIListener : MonoBehaviour
{
    [SerializeField] private FishingSequence _FishingSequence;


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
        // 인풋 차단
        InputBlock.BlockInput();

        _FishingSequence.gameObject.SetActive(true);
        _FishingSequence.StartFishing();

    }

}
