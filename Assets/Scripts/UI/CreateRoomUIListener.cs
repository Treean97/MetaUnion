using Unity.VisualScripting;
using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
    [SerializeField]
    private GameObject _CreateRoomUI;

    private void Awake()
    {
        GameEvents.OnRequestOpenCreateRoomUI += HandleCreateRoomActive;
    }

    private void OnDestroy()
    {
        GameEvents.OnRequestOpenCreateRoomUI += HandleCreateRoomActive;
    }

    private void HandleCreateRoomActive()
    {
        _CreateRoomUI.SetActive(true);
    }
}
