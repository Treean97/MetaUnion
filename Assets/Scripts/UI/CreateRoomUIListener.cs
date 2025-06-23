using Unity.VisualScripting;
using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField]
    private GameObject _CreateRoomUI;

    private void OnEnable()
    {
        GameEvents.OnRequestOpenCreateRoomUI += HandleCreateRoomActive;
    }

    private void OnDisable()
    {
        GameEvents.OnRequestOpenCreateRoomUI -= HandleCreateRoomActive;
    }

    private void HandleCreateRoomActive()
    {
        _CreateRoomUI.SetActive(true);
    }
}
