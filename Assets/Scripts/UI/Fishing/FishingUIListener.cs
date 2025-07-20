using UnityEngine;

public class FishingUIListener : MonoBehaviour
{
    [SerializeField] private FishingUIManager _FishingUIManager;


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
        _FishingUIManager.gameObject.SetActive(true);
    }

}
