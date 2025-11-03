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
    [SerializeField] private AdvancedDropdown _MapDropdown;
    [SerializeField] private Button _CreateButton;
    

    private const string MAP_PROP = "map";

    private void OnEnable()
    {
        _CreateButton.onClick.AddListener(OnConfirmClicked);
        _RoomNameInput.onValueChanged.AddListener(CheckRoomNameInput);

        InitMaxPlayerDropdown();
        InitMapDropdown();

        ResetInput();
    }

    private void InitMaxPlayerDropdown()
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

    private void InitMapDropdown()
    {
        if (_MapDropdown == null)
            return;

        _MapDropdown.DeleteAllOptions();

        var sceneListSO = Launcher._Inst != null ? Launcher._Inst.GetGameSceneListSO : null;
        if (sceneListSO == null || sceneListSO._SceneList == null || sceneListSO._SceneList.Count == 0)
        {
            // 맵 리스트가 비어 있으면 기본 텍스트만 표시
            _MapDropdown.SetDefaultText();
            return;
        }

        // SceneListSO에 있는 엔트리들을 드롭다운에 추가
        for (int i = 0; i < sceneListSO._SceneList.Count; i++)
        {
            var entry = sceneListSO._SceneList[i];
            if (string.IsNullOrWhiteSpace(entry.SceneName))
                continue;

            // 이름 + 아이콘 함께 추가
            _MapDropdown.AddOptions(entry.SceneName, entry.SceneIcon);
        }

        // 첫 번째 항목을 기본 선택으로
        if (sceneListSO.Count > 0)
        {
            _MapDropdown.SelectOption(0);
        }
        else
        {
            _MapDropdown.SetDefaultText();
        }
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

        UIFX.Hide(gameObject);
    }

    private void CheckRoomNameInput(string input)
    {
        _CreateButton.interactable = !string.IsNullOrWhiteSpace(input);
    }

    string ResolveMapName()
    {
        var sceneListSO = Launcher._Inst.GetGameSceneListSO;
        if (sceneListSO == null)
            return null;

        int index = _MapDropdown.value;
        if (index < 0 || index >= _MapDropdown.optionsList.Count)
        {
            Debug.LogError("맵 드롭다운 인덱스 범위 오류");
            return sceneListSO.GetRandomName();
        }

        // ★ 드롭다운이 들고 있는 텍스트(= SceneName)를 그대로 사용
        string sceneName = _MapDropdown.optionsList[index].nameText;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("맵 이름이 비어 있습니다");
            return sceneListSO.GetRandomName();
        }

        return sceneName;
    }

}
