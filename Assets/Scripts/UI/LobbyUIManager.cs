using UnityEngine;
using Photon.Pun;               
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class LobbyUIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _RoomListContent;
    [SerializeField] private GameObject _RoomItemPrefab;
    [SerializeField] private Button _JoinRoomBtn;
    [SerializeField] private Button _CreateRoomBtn;
    [SerializeField] private Button _RefreshBtn;

    private ButtonHoverSpin _Effect;
    private RoomInfo _SelectedRoomInfo;

    void Awake()
    {
        _Effect = gameObject.GetComponent<ButtonHoverSpin>();
    }

    // OnEnable 오버라이드
    public override void OnEnable()
    {
        base.OnEnable();  // ← Photon 콜백 등록

        GameEvents.RaiseOpenLobbyUI();
        GameEvents.OnSelectRoom += HandleSelectRoom;
        GameEvents.OnRoomListUpdated += HandleUpdateRoomList;

        var currentRoomList = CachedRoomList.GetRoomList();
        if (currentRoomList != null)
            HandleUpdateRoomList(currentRoomList);

        _JoinRoomBtn.interactable = false;
        _SelectedRoomInfo = null;

        _JoinRoomBtn.onClick.RemoveAllListeners();
        _JoinRoomBtn.onClick.AddListener(OnJoinRoomButtonClicked);

        _CreateRoomBtn.onClick.RemoveAllListeners();
        _CreateRoomBtn.onClick.AddListener(OnCreateRoomButtonClicked);

        // _RefreshBtn.onClick.RemoveAllListeners();
        // _RefreshBtn.onClick.AddListener(OnRefreshButtonClicked);
    }

    // OnDisable 오버라이드
    public override void OnDisable()
    {
        base.OnDisable(); // ← Photon 콜백 해제

        GameEvents.OnSelectRoom -= HandleSelectRoom;
        GameEvents.OnRoomListUpdated -= HandleUpdateRoomList;
    }

    public override void OnLeftLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    private void HandleSelectRoom(RoomInfo info)
    {
        _SelectedRoomInfo = info;
        _JoinRoomBtn.interactable = true;
    }

    private void HandleUpdateRoomList(List<RoomInfo> roomList)
    {
        CachedRoomList.SetRoomList(roomList);

        foreach (Transform child in _RoomListContent)
            Destroy(child.gameObject);

        foreach (var info in roomList)
        {
            if (info.RemovedFromList) continue;
            var item = Instantiate(_RoomItemPrefab, _RoomListContent);
            var manager = item.GetComponent<RoomItemUIManager>();
            manager.SetInfo(info);
        }
    }

    private void OnCreateRoomButtonClicked()
    {
        StartCoroutine(CreateRoomSequence());
    }

    IEnumerator CreateRoomSequence()
    {
        if (_Effect != null)
        {
            yield return StartCoroutine(_Effect.ClickEffect()); // 이펙트 완료까지 대기
        }

        GameEvents.RaiseRequestOpenCreateRoomUI();
    }

    private void OnJoinRoomButtonClicked()
    {
        StartCoroutine(JoinRoomSequence());
    }

    IEnumerator JoinRoomSequence()
    {
        if (_Effect != null)
        {
            yield return StartCoroutine(_Effect.ClickEffect()); // 이펙트 완료까지 대기
        }

        if (!string.IsNullOrEmpty(_SelectedRoomInfo.Name))
        {
            GameEvents.RaiseRequestJoinRoom(_SelectedRoomInfo);
        }

    }
    
    // private void OnRefreshButtonClicked()
    // {
    //     foreach (Transform child in _RoomListContent)
    //         Destroy(child.gameObject);

    //     CachedRoomList.SetRoomList(new List<RoomInfo>());
    //     _SelectedRoomInfo = null;
    //     _JoinRoomBtn.interactable = false;

    //     PhotonNetwork.LeaveLobby();
    // }
}
