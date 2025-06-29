using UnityEngine;
using Photon.Pun;               
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _RoomListContent;
    [SerializeField] private GameObject  _RoomItemPrefab;
    [SerializeField] private Button      _JoinRoomBtn;
    [SerializeField] private Button      _CreateRoomBtn;
    [SerializeField] private Button      _RefreshBtn;

    private RoomInfo _SelectedRoomInfo;

    // OnEnable 오버라이드
    public override void OnEnable()
    {
        base.OnEnable();  // ← Photon 콜백 등록

        GameEvents.RaiseOpenLobbyUI();
        GameEvents.OnSelectRoom      += HandleSelectRoom;
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

        _RefreshBtn.onClick.RemoveAllListeners();
        _RefreshBtn.onClick.AddListener(OnRefreshButtonClicked);
    }

    // OnDisable 오버라이드
    public override void OnDisable()
    {
        base.OnDisable(); // ← Photon 콜백 해제

        GameEvents.OnSelectRoom      -= HandleSelectRoom;
        GameEvents.OnRoomListUpdated -= HandleUpdateRoomList;

        _RefreshBtn.onClick.RemoveListener(OnRefreshButtonClicked);
    }

    private void OnRefreshButtonClicked()
    {
        foreach (Transform child in _RoomListContent)
            Destroy(child.gameObject);

        CachedRoomList.SetRoomList(new List<RoomInfo>());
        _SelectedRoomInfo = null;
        _JoinRoomBtn.interactable = false;

        PhotonNetwork.LeaveLobby();
    }

    public override void OnLeftLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    private void HandleSelectRoom(RoomInfo info)
    {
        _SelectedRoomInfo         = info;
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
            var item    = Instantiate(_RoomItemPrefab, _RoomListContent);
            var manager = item.GetComponent<RoomItemUIManager>();
            manager.SetInfo(info);
        }
    }

    private void OnCreateRoomButtonClicked()
    {
        GameEvents.RaiseRequestOpenCreateRoomUI();
    }

    private void OnJoinRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(_SelectedRoomInfo.Name))
            GameEvents.RaiseRequestJoinRoom(_SelectedRoomInfo);
    }
}
