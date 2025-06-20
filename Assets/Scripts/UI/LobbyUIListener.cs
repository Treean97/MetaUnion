using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUI;

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
        _LobbyUI.SetActive(true);
    }
}
