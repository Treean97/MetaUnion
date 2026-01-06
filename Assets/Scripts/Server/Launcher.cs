using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

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

    [Header("CCU 초과 재시도")]
    [SerializeField] private bool _RetryOnMaxCcu = true;
    [SerializeField] private float _RetryDelayMin = 5f;
    [SerializeField] private float _RetryDelayMax = 60f;
    [SerializeField] private float _RetryJitterMax = 1.5f;

    private Coroutine _RetryCcuCo;
    private float _RetryDelay;

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
        StopRetryMaxCcu();
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

        // CCU 초과일 때만 대기 후 재시도
        if (_RetryOnMaxCcu && cause == DisconnectCause.MaxCcuReached)
        {
            StartRetryMaxCcu();
            return;
        }

    }
    private void StartRetryMaxCcu()
    {
        if (_RetryCcuCo != null) return;

        // 지수 백오프 시작값
        if (_RetryDelay <= 0f) _RetryDelay = _RetryDelayMin;

        _RetryCcuCo = StartCoroutine(CoRetryMaxCcu());
    }

    private void StopRetryMaxCcu()
    {
        if (_RetryCcuCo != null)
        {
            StopCoroutine(_RetryCcuCo);
            _RetryCcuCo = null;
        }
        _RetryDelay = 0f;
    }

    private IEnumerator CoRetryMaxCcu()
    {
        while (true)
        {
            float jitter = Random.Range(0f, _RetryJitterMax);
            float wait = Mathf.Min(_RetryDelay + jitter, _RetryDelayMax);

            // 만석 안내 UI (원하면 메시지 바꾸기)
            GameEvents.RaiseShowWarning($"서버가 만석입니다. {wait:0}초 후 재시도합니다.", 2f);

            yield return new WaitForSeconds(wait);

            // 재시도 전에 완전 끊긴 상태인지 체크
            if (PhotonNetwork.IsConnected)
            {
                // 연결이 살아나 있으면 로비로
                PhotonNetwork.JoinLobby();
                break;
            }

            PhotonNetwork.GameVersion = _GameVersion;
            PhotonNetwork.ConnectUsingSettings();

            // 다음 대기시간(지수 증가)
            _RetryDelay = Mathf.Min(_RetryDelay * 2f, _RetryDelayMax);

            // 여기서 바로 성공 여부를 알 수 없으니,
            // 성공하면 OnConnectedToMaster/OnJoinedLobby로 흐름 진행
            // 실패하면 OnDisconnected가 다시 호출되어 루프 유지
            // 다만 코루틴 중복 방지 위해, 여기서는 잠깐 대기 후 다음 루프
            yield return new WaitForSeconds(1f);
        }

        _RetryCcuCo = null;
        _RetryDelay = 0f;
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
