using UnityEngine;

public class ControlPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _ControlPanelUI;

    void OnEnable()
    {
        UIEvents.OnOpenLobbyUI += ControlPanelUIInactive;
        UIEvents.OnLeaveRoom += ControlPanelUIActive;
    }

    void OnDisable()
    {
        UIEvents.OnOpenLobbyUI -= ControlPanelUIInactive;
        UIEvents.OnLeaveRoom -= ControlPanelUIActive;
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
