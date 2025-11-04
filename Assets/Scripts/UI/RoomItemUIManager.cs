using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomItemUIManager : MonoBehaviour
{
    [Header("UI Set")]
    [SerializeField] private Button _RommItemBtn;
    [SerializeField] private TMP_Text _RoomNameText;
    [SerializeField] private TMP_Text _MapNameText;
    [SerializeField] private TMP_Text _PlayerCountText;

    [Header("Colors")]
    [SerializeField] private Color _JoinableColor;     // 입장 가능 색
    [SerializeField] private Color _UnjoinableColor;   // 입장 불가(정원 꽉참) 색

    private RoomInfo _RoomInfo;

    private const string MAP_PROP = "map";

    void OnEnable()
    {
        _RommItemBtn.onClick.AddListener(() => OnSelectRoom());
    }

    public void SetInfo(RoomInfo info)
    {
        _RoomInfo = info;
        _RoomNameText.text = info.Name;
        _PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";

        // 정원 여부에 따라 색상 적용
        bool isFull = info.PlayerCount >= info.MaxPlayers;
        ApplyJoinableColor(!isFull);
        ApplyMapInfo(info);
    }

    public void OnSelectRoom()
    {
        GameEvents.RaiseRoomSelect(_RoomInfo);
    }

    private void ApplyJoinableColor(bool joinable)
    {
        var g = _RommItemBtn ? _RommItemBtn.targetGraphic : null;
        if (g == null) return; // 버튼에 타겟 그래픽이 없으면 아무 것도 하지 않음

        g.color = joinable ? _JoinableColor : _UnjoinableColor;
    }

    private void ApplyMapInfo(RoomInfo info)
    {
        // RoomInfo.CustomProperties에서 map 이름 꺼내기
        string sceneName = null;

        if (info.CustomProperties != null &&
            info.CustomProperties.TryGetValue(MAP_PROP, out var mapObj))
        {
            sceneName = mapObj as string;
        }

        // 맵 정보가 아예 없으면 기본 표시
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("맵 정보가 없습니다.");
            _MapNameText.text = "-";
            return;
        }

        // SceneListSO 가져오기
        var launcher = Launcher._Inst;
        var sceneListSO = launcher != null ? launcher.GetGameSceneListSO : null;

        if (sceneListSO == null || sceneListSO._SceneList == null)
        {
            // 최소한 씬 이름이라도 보여주기
            _MapNameText.text = sceneName;
            return;
        }

        // SceneListSO에서 해당 씬 이름을 가진 엔트리 찾기
        SceneListSO.SceneEntry found = null;
        for (int i = 0; i < sceneListSO._SceneList.Count; i++)
        {
            var entry = sceneListSO._SceneList[i];
            if (entry != null && entry.SceneName == sceneName)
            {
                found = entry;
                break;
            }
        }

        // 결과 적용
        if (found != null)
        {
            // DisplayName이 비어 있으면 SceneName 사용
            var displayName = string.IsNullOrWhiteSpace(found.DisplayName)
                ? found.SceneName
                : found.DisplayName;

            _MapNameText.text = displayName;
        }
        else
        {
            // SceneListSO에 없는 맵이면 그냥 sceneName 그대로
            Debug.Log("SceneListSO에서 찾을 수 없습니다.");
            _MapNameText.text = sceneName;
        }    
    }
}