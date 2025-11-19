using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher _Inst { get; private set; }    

    [Header("설정")]
    [SerializeField] private string _GameVersion = "1";

    [Header("게임 맵 데이터")]
    [SerializeField] private SceneListSO _GameSceneListSO;
    public SceneListSO GetGameSceneListSO => _GameSceneListSO;
    public object GameSceneListSO { get; internal set; }
    private const string MAP_PROP = "map";

    // private Dictionary<string, RoomInfo> _CachedRoomList = new Dictionary<string, RoomInfo>();

    // RoomInfo _RoomInfo;

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        // 동시 씬 전환 금지
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    #region 연결 흐름

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            PhotonNetwork.GameVersion = _GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected: {cause}");

    }

    #endregion

    #region 방 목록 갱신

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        GameEvents.RaiseRoomListUpdate(roomList);
    }


    #endregion

    #region 방 생성 및 입장

    // 방 입장 시도
    public void RequestJoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
    }

    // 입장 성공 호출
    public override void OnJoinedRoom()
    {
        // 제일 먼저 포톤 메시지 큐를 멈춘다
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMessageQueueRunning)
        {
            PhotonNetwork.IsMessageQueueRunning = false;
            Debug.Log("[Launcher] Pause Photon Message Queue on JoinedRoom");
        }

        // 그 다음 게임 씬 로딩 시작
        if (PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(MAP_PROP, out object obj))
        {
            string mapName = (string)obj;

            // 너가 쓰는 씬 로더 호출
            SceneLoadManager._Inst.SceneLoad(mapName);
            // 또는 LoadingManager._Inst?.LoadScene(mapName);

            Debug.Log($"Load Scene: {mapName}");
        }

        // 방 입장 이벤트
        GameEvents.RaiseJoinRoomSuccess();

        Debug.Log("방 입장 완료");
    }

    // 입장 실패 호출
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 입장 실패: {message}");

        GameEvents.RaiseShowWarning("Fail to Join Room", 2f);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성 완료");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패: {message}");

        if (returnCode == ErrorCode.GameIdAlreadyExists) // 32766
        {
            GameEvents.RaiseShowWarning("This room name already exists.", 2f);
        }
        else
        {
            GameEvents.RaiseShowWarning($"Failed to create room", 2f);
        }
    }

    #endregion

}
