using Unity.VisualScripting;
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

}
