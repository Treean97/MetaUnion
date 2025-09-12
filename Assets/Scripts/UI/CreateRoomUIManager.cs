using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class CreateRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _RoomNameInput;
    [SerializeField] private TMP_Dropdown _MaxPlayerDropdown;
    [SerializeField] private Button _CreateButton;
    [SerializeField] private Button _CancelButton;

    private const string MAP_PROP = "map";

    private void OnEnable()
    {
        _CreateButton.onClick.AddListener(OnConfirmClicked);
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
        StartCoroutine(CreateButtonSequence());
    }

    IEnumerator WaitSequence(Button button)
    {
        var sequence = button.GetComponent<ButtonSequence>();
        if (sequence)
        {
            yield return sequence.RunSequence();
        }
    }

    IEnumerator CreateButtonSequence()
    {
        string roomName = _RoomNameInput.text.Trim();
        byte maxPlayers = (byte)(_MaxPlayerDropdown.value + 1);

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("방 이름을 입력하세요");
            GameEvents.RaiseShowWarning("Input the Room Name", 2f);
            yield break;
        }

        yield return WaitSequence(_CreateButton);

        // 맵 선택
        string mapName = ResolveMapName();

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true,

            CustomRoomPropertiesForLobby = new[] { MAP_PROP },

            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                {MAP_PROP, mapName}
            }
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
        _CreateButton.interactable = !string.IsNullOrWhiteSpace(input);
    }

    string ResolveMapName()
    {
        // 드롭다운 인덱스 사용
        // if (Launcher._Inst.GetGameSceneListSO != null && Launcher._Inst.GetGameSceneListSO.TryGetNameByIndex(_MapDropdown.value, out var name))
        //     return name;

        // 실패 시 랜덤/기본값
        return Launcher._Inst.GetGameSceneListSO.GetRandomName();
    }

}
