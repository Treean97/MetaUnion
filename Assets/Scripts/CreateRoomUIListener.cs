using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _CreateRoomPanel;

    private void Awake()
    {
        UIEvents.OnOpenCreateRoomPopup += HandleOpenCreateRoomPopup;
    }

    private void OnDestroy()
    {
        UIEvents.OnOpenCreateRoomPopup -= HandleOpenCreateRoomPopup;
    }

    private void HandleOpenCreateRoomPopup()
    {
        _CreateRoomPanel.SetActive(true);
    }
}
