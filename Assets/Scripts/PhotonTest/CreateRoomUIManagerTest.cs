using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP용 네임스페이스
using Photon.Pun;
using Photon.Realtime;

public class CreateRoomUIManagerTest : MonoBehaviour
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
    }

    private void OnEnable()
    {
        // 드롭다운 초기화 (1~4명 선택 가능하게)
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
}
