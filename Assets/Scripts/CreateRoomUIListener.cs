using Unity.VisualScripting;
using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField] private GameObject _CreateRoomPanel;

    private void OnEnable()
    {
        GameEvents.OnOpenCreateRoomUI += HandleOpenCreateRoomPopup;
    }

    private void OnDisable()
    {
        GameEvents.OnOpenCreateRoomUI -= HandleOpenCreateRoomPopup;
    }

    private void HandleOpenCreateRoomPopup()
    {
        _CreateRoomPanel.SetActive(true);
    }
}
