using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUIPanel;

    private void Awake()
    {
        UIEvents.OnOpenLobbyUI += HandleOpenLobbyUI;
    }

    private void OnDestroy()
    {
        UIEvents.OnOpenLobbyUI -= HandleOpenLobbyUI;
    }

    private void HandleOpenLobbyUI()
    {
        _LobbyUIPanel.SetActive(true);
    }
}
