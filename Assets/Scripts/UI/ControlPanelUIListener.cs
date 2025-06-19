using UnityEngine;

public class ControlPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _ControlPanelUI;

    void OnEnable()
    {
        GameEvents.OnOpenLobbyUI += HandleControlUIInactive;
    }

    void OnDisable()
    {
        GameEvents.OnOpenLobbyUI -= HandleControlUIInactive;
    }


    void HandleControlUIInactive()
    {
        _ControlPanelUI.SetActive(false);
    }

}
