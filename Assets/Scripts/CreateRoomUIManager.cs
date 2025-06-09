using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _RoomNameInput;
    [SerializeField] private TMP_Dropdown _MaxPlayersDropdown;
    [SerializeField] private Button _ConfirmButton;
    [SerializeField] private Button _CancelButton;
    [SerializeField] private GameObject _CreateRoomPanel;  // 자기 패널 직접 참조 가능 (닫기용)

    private void Awake()
    {
        _ConfirmButton.onClick.AddListener(OnConfirmButtonClicked);
        _CancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnConfirmButtonClicked()
    {
        string roomName = _RoomNameInput.text;
        byte maxPlayers = (byte)(_MaxPlayersDropdown.value + 1);

        UIEvents.RaiseCreateRoom(roomName, maxPlayers);

        _CreateRoomPanel.SetActive(false);  // 팝업 닫기
    }

    private void OnCancelButtonClicked()
    {
        _CreateRoomPanel.SetActive(false);
    }
}
