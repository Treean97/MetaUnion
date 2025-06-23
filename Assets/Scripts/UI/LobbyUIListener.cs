using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUI;

    private void OnEnable()
    {
        GameEvents.OnRequestOpenLobbyUI += HandleLobbyUIActive;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestOpenLobbyUI -= HandleLobbyUIActive;
    }

    private void HandleLobbyUIActive()
    {
        _LobbyUI.SetActive(true);
    }
}
