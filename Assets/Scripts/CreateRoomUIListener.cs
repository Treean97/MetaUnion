using Unity.VisualScripting;
using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _CreateRoomPanel;

    private void OnEnable()
    {
        UIEvents.OnOpenCreateRoomUI += HandleOpenCreateRoomPopup;
    }

    private void OnDisable()
    {
        UIEvents.OnOpenCreateRoomUI -= HandleOpenCreateRoomPopup;
    }

    private void HandleOpenCreateRoomPopup()
    {
        _CreateRoomPanel.SetActive(true);
    }
}
