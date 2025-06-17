using UnityEngine;

public class ControlPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _ControlPanelUI;

    void OnEnable()
    {
        GameEvents.OnOpenLobbyUI += ControlPanelUIInactive;
        GameEvents.OnLeaveRoom += ControlPanelUIActive;
    }

    void OnDisable()
    {
        GameEvents.OnOpenLobbyUI -= ControlPanelUIInactive;
        GameEvents.OnLeaveRoom -= ControlPanelUIActive;
    }


    void ControlPanelUIActive()
    {
        _ControlPanelUI.SetActive(true);
    }

    void ControlPanelUIInactive()
    {
        _ControlPanelUI.SetActive(false);
    }
}
