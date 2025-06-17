using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP용 네임스페이스
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _RoomNameInput;
    [SerializeField] private TMP_Dropdown _MaxPlayerDropdown;
    [SerializeField] private Button _ConfirmButton;
    [SerializeField] private Button _CancelButton;

    [SerializeField] private GameObject _RootPanel; // 이 UI 전체를 담고 있는 부모 오브젝트

    private void Awake()
    {
        _ConfirmButton.onClick.AddListener(OnConfirmClicked);
        _CancelButton.onClick.AddListener(OnCancelClicked);

        _RoomNameInput.onValueChanged.AddListener(CheckRoomNameInput);
    }

    private void OnEnable()
    {
        if (_MaxPlayerDropdown.options.Count == 0)
        {
            _MaxPlayerDropdown.ClearOptions();
            for (int i = 1; i <= 4; i++)
            {
                _MaxPlayerDropdown.options.Add(new TMP_Dropdown.OptionData(i.ToString()));
            }
            _MaxPlayerDropdown.value = 0;
        }

        _RoomNameInput.text = "";
        CheckRoomNameInput(""); // 버튼 비활성화 초기화
    }

    private void OnConfirmClicked()
    {
        string roomName = _RoomNameInput.text.Trim();
        byte maxPlayers = (byte)(_MaxPlayerDropdown.value + 1); // Dropdown index 0 = 1명

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("⚠ 방 이름을 입력하세요");
            return;
        }

        RoomOptions options = new RoomOptions
        {
            // CustomRoomProperties를 이용해서 입장할 씬의 이름을 담을 수 있음 추후 Dropdown UI와 연동하면 좋을 듯
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options);
        _RootPanel.SetActive(false);
    }

    private void OnCancelClicked()
    {
        _RootPanel.SetActive(false);
    }

    private void CheckRoomNameInput(string input)
    {
        _ConfirmButton.interactable = !string.IsNullOrWhiteSpace(input);
    }
}
