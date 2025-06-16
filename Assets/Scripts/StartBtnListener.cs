using UnityEngine;
using UnityEngine.UI;

public class StartBtnListener : MonoBehaviour
{
    [SerializeField] private Button _StartButton;

    private void Awake()
    {
        _StartButton.onClick.AddListener(() => GameEvents.RaiseOpenLobbyUI());
        _StartButton.onClick.AddListener(() => GameEvents.RaiseConnect());
    }

    void OnEnable()
    {
        GameEvents.OnStartBtnActive += ActivateButton;
        GameEvents.OnStartBtnInactive += DeactivateButton;
    }

    void OnDisable()
    {
        GameEvents.OnStartBtnActive -= ActivateButton;
        GameEvents.OnStartBtnInactive -= DeactivateButton;
    }
    
        private void ActivateButton() => _StartButton.interactable = true;
        private void DeactivateButton() => _StartButton.interactable = false;   
}
