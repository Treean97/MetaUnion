using UnityEngine;

public class RouletteUIListener : MonoBehaviour
{
    [SerializeField] RouletteUIManager _RouletteUIManager;

    void Awake()
    {
        GameEvents.OnRequestOpenRouletteUI += HandleSetActive;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestOpenRouletteUI -= HandleSetActive;
    }

    void HandleSetActive()
    {
        _RouletteUIManager.gameObject.SetActive(true);
    }
}
