using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUIPanel;

    private void Awake()
    {
        GameEvents.OnOpenLobbyUI += HandleOpenLobbyUI;
    }

    private void OnDestroy()
    {
        GameEvents.OnOpenLobbyUI -= HandleOpenLobbyUI;
    }

    private void HandleOpenLobbyUI()
    {
        _LobbyUIPanel.SetActive(true);
    }
}
