using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _CreateRoomPanel;
    [SerializeField] private UIID _MyID = UIID.CreateRoom;

    private void OnEnable()
    {
        GameEvents.OnSetActive += HandleSetActiveCreateRoom;
    }

    private void OnDisable()
    {
        GameEvents.OnSetActive -= HandleSetActiveCreateRoom;
    }

    private void HandleSetActiveCreateRoom(UIID tUIID, bool tEnable)
    {
        if (tUIID != _MyID) return;
        _CreateRoomPanel.SetActive(tEnable);
    }
}
