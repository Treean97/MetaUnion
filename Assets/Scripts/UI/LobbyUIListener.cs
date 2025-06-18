using UnityEngine;

public class LobbyUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _LobbyUIPanel;

    private void Awake()
    {
        GameEvents.OnSetActive += HandleLobbyUI;
    }

    private void OnDestroy()
    {
        GameEvents.OnSetActive -= HandleLobbyUI;
    }

    private void HandleLobbyUI(UIID uiId, bool enable)
    {
        if (uiId != UIID.Lobby)
        {
            return;
        }
        
        _LobbyUIPanel.SetActive(enable);
    }
}
