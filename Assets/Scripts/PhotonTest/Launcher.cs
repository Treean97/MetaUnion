// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Photon.Pun;
// using Photon.Realtime;


// namespace Com.MyCompany.MyGame //중복 이름 스크립트로 인한 다른 에셋과 개발자의 코드 충돌을 막기 위함
// {
//     public class Launcher : MonoBehaviourPunCallbacks
//     {
//         #region Public Fields

//         [Tooltip("The Ui Panel to let the user enter name, connect and play")]
//         [SerializeField]
//         private GameObject controlPanel;
//         [Tooltip("The UI Label to inform the user that the connection is in progress")]
//         [SerializeField]
//         private GameObject progressLabel;

//         #endregion

//         #region Private Serializable Fields

//         /// <summary>
//         /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
//         /// </summary>
//         [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
//         [SerializeField]
//         private byte maxPlayersPerRoom = 4;

//         #endregion



//         #region Private Fields


//         /// <summary>
//         /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
//         /// </summary>
//         string gameVersion = "1";  //게임의 버전을 나타내는 변수
//         bool isConnecting;

//         #endregion


//         #region MonoBehaviour CallBacks

//         /// <summary>
//         /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
//         /// </summary>
//         void Awake()
//         { 
//             PhotonNetwork.AutomaticallySyncScene = true; //플레이어 동기화 기능, 마스터 클라이언트가 룸내의 모든 클라이언트에게 로드해야 할 레벨정의
//         }

//         /// <summary>
//         /// MonoBehaviour method called on GameObject by Unity during initialization phase.
//         /// </summary>
//         void Start()
//         {
//             // 나중에 이벤트로 연결
//             progressLabel.SetActive(false);
//             controlPanel.SetActive(true);

//             // Connect(); //유니티 시작했을 때 Connect 함수를 실행, 서버 접속 후 방 부여하기
//         }


//         #endregion


//         #region Public Methods

//         /// <summary>
//         /// Start the connection process.
//         /// - If already connected, we attempt joining a random room
//         /// - if not yet connected, Connect this application instance to Photon Cloud Network
//         /// </summary>
//         public void Connect()
//         {
//             isConnecting = true;

//             // 나중에 이벤트로 연결
//             progressLabel.SetActive(true);
//             controlPanel.SetActive(false);

//             if (PhotonNetwork.IsConnected) //Photon 네트워크와 연결
//             {
//                 PhotonNetwork.JoinRandomRoom(); //랜덤 방 부여
//             }
//             else //Photon 네트워크와 연결 X
//             {
//                 PhotonNetwork.GameVersion = gameVersion; //포톤 게임버전 설정
//                 PhotonNetwork.ConnectUsingSettings(); //Photon을 이용한 온라인 연결
//             }
//         }


//         #endregion


//         #region MonoBehaviourPunCallbacks Callbacks 


//         //서버 연결이 되었는 지 확인하는 함수 
//         public override void OnConnectedToMaster() //방장이 서버에 접속했을 때 
//         {
//             if (isConnecting)
//             {
//                 // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
//                 PhotonNetwork.JoinRandomRoom();
//             }

//             Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
//         }


//         public override void OnDisconnected(DisconnectCause cause) //연결이 안되었을 때 
//         {
//             Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause); //casue라는 변수 원인 출력
//             // 나중에 이벤트로 연결
//             progressLabel.SetActive(false);
//             controlPanel.SetActive(true);
//         }

//         public override void OnJoinRandomFailed(short returnCode, string message) //랜덤 방에 연결 실패했을 때
//         {
//             Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

//             // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
//             PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom }); //새로운 방을 만들어준다.
//         }

//         public override void OnJoinedRoom() //방안에 들어갔을 때
//         {
//             Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
//             // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
//             if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
//             {
//                 Debug.Log("We load the 'Room for 1' ");

//                 // #Critical
//                 // Load the Room Level.
//                 PhotonNetwork.LoadLevel("Room for 1");
//             }
//         }
//         #endregion

//     }
// }

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // private Dictionary<string, RoomInfo> _CachedRoomList = new Dictionary<string, RoomInfo>();

    // RoomInfo _RoomInfo;

    void Awake()
    {
        if (_Inst != null) { Destroy(gameObject); return; }
        _Inst = this;
        DontDestroyOnLoad(gameObject);

        GameEvents.OnRequestJoinRoom += RequestJoinRoom;
        GameEvents.OnConnect += Connect;
    }

    void OnDestroy()
    {
        GameEvents.OnRequestJoinRoom -= RequestJoinRoom;
        GameEvents.OnConnect -= Connect;
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
        Debug.Log("✔️ Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✔️ Joined Lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"❌ Disconnected: {cause}");

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
        // SceneListSO에 맵이 하나도 없으면 에러
        if (_GameSceneListSO == null || _GameSceneListSO._SceneList.Count == 0)
        {
            Debug.LogError("씬 리스트가 비어 있습니다! SceneListSO를 확인하세요.");
            return;
        }

        // 랜덤으로 맵 하나 선택
        int idx = Random.Range(0, _GameSceneListSO._SceneList.Count);
        string sceneToLoad = _GameSceneListSO._SceneList[idx].SceneName;
        Debug.Log($"랜덤 맵 로드: {sceneToLoad}");

        // 선택된 씬 로드
        PhotonNetwork.LoadLevel(sceneToLoad);

        // 방 입장 이벤트
        GameEvents.RaiseJoinRoomSuccess();
        
        Debug.Log("✅ 방 입장 완료");
    }

    // 입장 실패 호출
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ 방 입장 실패: {message}");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("✅ 방 생성 완료");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"❌ 방 생성 실패: {message}");

        if (returnCode == ErrorCode.GameIdAlreadyExists) // 32766
        {
            GameEvents.RaiseShowWarning("This room name already exists.", 2f);
        }
        else
        {
            GameEvents.RaiseShowWarning($"Failed to create room : {message}", 2f);
        }
    }

    #endregion

}
