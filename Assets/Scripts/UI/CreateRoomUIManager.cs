using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class CreateRoomUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _RoomNameInput;
    [SerializeField] private AdvancedDropdown _MaxPlayerDropdown;
    [SerializeField] private AdvancedDropdown _MapDropdown;
    [SerializeField] private Button _CreateButton;
    
    private readonly List<string> _MapSceneNames = new List<string>();
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
        if (_MaxPlayerDropdown == null)
            return;

        // 이전 옵션 제거
        _MaxPlayerDropdown.DeleteAllOptions();

        // 1~4까지 텍스트 옵션 추가
        for (int i = 1; i <= 4; i++)
        {
            _MaxPlayerDropdown.AddOptions(i.ToString());
        }

        // 기본 선택값: 1인
        if (_MaxPlayerDropdown.optionsList.Count > 0)
        {
            _MaxPlayerDropdown.SelectOption(0); // index 0 → "1"
        }
        else
        {
            _MaxPlayerDropdown.SetDefaultText();
        }
    }

    private void InitMapDropdown()
    {
        if (_MapDropdown == null)
            return;

        _MapDropdown.DeleteAllOptions();
        _MapSceneNames.Clear();

        var sceneListSO = Launcher._Inst != null ? Launcher._Inst.GetGameSceneListSO : null;
        if (sceneListSO == null || sceneListSO._SceneList == null || sceneListSO._SceneList.Count == 0)
        {
            _MapDropdown.SetDefaultText();
            return;
        }

        // SceneListSO에 있는 엔트리들을 드롭다운에 추가
        for (int i = 0; i < sceneListSO._SceneList.Count; i++)
        {
            var entry = sceneListSO._SceneList[i];
            if (string.IsNullOrWhiteSpace(entry.SceneName))
                continue;

            // 드롭다운에는 표기용 이름 + 아이콘
            _MapDropdown.AddOptions(entry.DisplayName, entry.SceneIcon);

            // ★ 같은 순서로 실제 SceneName을 따로 저장
            _MapSceneNames.Add(entry.SceneName);
        }

        if (_MapSceneNames.Count > 0)
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

        if (_MapDropdown == null || _MapSceneNames.Count == 0)
            return sceneListSO.GetRandomName();

        int index = _MapDropdown.value;
        if (index < 0 || index >= _MapSceneNames.Count)
        {
            Debug.LogError("맵 드롭다운 인덱스 범위 오류");
            return sceneListSO.GetRandomName();
        }

        // 드롭다운 인덱스에 대응하는 실제 SceneName 사용
        string sceneName = _MapSceneNames[index];
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("맵 이름이 비어 있습니다");
            return sceneListSO.GetRandomName();
        }

        return sceneName;
    }

}
