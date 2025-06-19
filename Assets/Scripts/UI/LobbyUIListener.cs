using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUIPanel;

    private void Awake()
    {
        GameEvents.OnRequestOpenLobbyUI += HandleLobbyUIActive;
    }

    private void OnDestroy()
    {
        GameEvents.OnRequestOpenLobbyUI -= HandleLobbyUIActive;
    }

    private void HandleLobbyUIActive()
    {
        _LobbyUIPanel.SetActive(true);
    }
}
