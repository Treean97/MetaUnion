using Unity.VisualScripting;
using UnityEngine;

public class CreateRoomUIListener : MonoBehaviour
{
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
        gameObject.SetActive(true);
    }
}
