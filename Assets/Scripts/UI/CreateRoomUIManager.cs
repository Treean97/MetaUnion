using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _RoomNameInput;
    [SerializeField] private TMP_Dropdown _MaxPlayerDropdown;
    [SerializeField] private Button _ConfirmButton;
    [SerializeField] private Button _CancelButton;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        _ConfirmButton.onClick.AddListener(OnConfirmClicked);
        _CancelButton.onClick.AddListener(OnCancelClicked);
        _RoomNameInput.onValueChanged.AddListener(CheckRoomNameInput);

        InitDropdown();
        ResetInput();
    }

    private void InitDropdown()
    {
        if (_MaxPlayerDropdown.options.Count == 0)
        {
            _MaxPlayerDropdown.ClearOptions();
            for (int i = 1; i <= 4; i++)
            {
                _MaxPlayerDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
        }
        _MaxPlayerDropdown.value = 0;
    }

    private void ResetInput()
    {
        _RoomNameInput.text = "";
        CheckRoomNameInput("");
    }

    private void OnConfirmClicked()
    {
        string roomName = _RoomNameInput.text.Trim();
        byte maxPlayers = (byte)(_MaxPlayerDropdown.value + 1);

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("방 이름을 입력하세요");
            GameEvents.RaiseShowWarning("Input the Room Name", 2f);
            return;
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options);

        gameObject.SetActive(false);        
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void CheckRoomNameInput(string input)
    {
        _ConfirmButton.interactable = !string.IsNullOrWhiteSpace(input);
    }
}
