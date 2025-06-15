using UnityEngine;
using UnityEngine.UI;

public class StartBtnListener : MonoBehaviour
{
    [SerializeField] private Button _StartButton;

    private void Awake()
    {
        _StartButton.onClick.AddListener(() => UIEvents.RaiseOpenLobbyUI());
        _StartButton.onClick.AddListener(() => UIEvents.RaiseConnect());
    }

    void OnEnable()
    {
        UIEvents.OnStartBtnActive += ActivateButton;
        UIEvents.OnStartBtnInactive += DeactivateButton;
    }

    void OnDisable()
    {
        UIEvents.OnStartBtnActive -= ActivateButton;
        UIEvents.OnStartBtnInactive -= DeactivateButton;
    }
    
        private void ActivateButton() => _StartButton.interactable = true;
        private void DeactivateButton() => _StartButton.interactable = false;   
}
