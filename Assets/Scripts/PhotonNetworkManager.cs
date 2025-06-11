using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    private bool _requestedLobbyUI = false;

    private void Awake()
    {
        UIEvents.OnOpenLobbyUI += HandleOpenLobby;
        UIEvents.OnCreateRoom  += HandleCreateRoom;
        UIEvents.OnJoinRoom    += HandleJoinRoom;
    }

    private void OnDestroy()
    {
        UIEvents.OnOpenLobbyUI -= HandleOpenLobby;
        UIEvents.OnCreateRoom  -= HandleCreateRoom;
        UIEvents.OnJoinRoom    -= HandleJoinRoom;
    }

    private void HandleOpenLobby()
    {
        var state = PhotonNetwork.NetworkClientState;

        if (state == ClientState.Disconnected)
        {
            Debug.Log("[Photon] 상태: Disconnected → ConnectUsingSettings 호출");
            _requestedLobbyUI = true;
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (state == ClientState.ConnectedToMasterServer && !PhotonNetwork.InLobby)
        {
            Debug.Log("[Photon] Master 서버 연결됨 → JoinLobby 호출");
            _requestedLobbyUI = true;
            PhotonNetwork.JoinLobby();
        }
        else if (PhotonNetwork.InLobby)
        {
            Debug.Log("[Photon] 이미 로비에 있음 → UI 표시");
            UIEvents.RaiseRoomListUpdated(CachedRoomList.GetRoomList());
        }
        else
        {
            Debug.Log($"[Photon] 현재 상태: {state} → 대기 중");
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Photon] OnConnectedToMaster → JoinLobby 호출");

        if (_requestedLobbyUI)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[Photon] OnJoinedLobby → 방 목록 UI 갱신");
        UIEvents.RaiseRoomListUpdated(CachedRoomList.GetRoomList());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        CachedRoomList.SetRoomList(roomList);
        UIEvents.RaiseRoomListUpdated(roomList);
    }

    private void HandleCreateRoom(string roomName, byte maxPlayers)
    {
        if (!PhotonNetwork.InLobby)
        {
            Debug.LogError("[Photon] CreateRoom 실패 - 현재 상태: " + PhotonNetwork.NetworkClientState + ", 로비에 먼저 입장해야 함");
            return;
        }

        var tOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            CleanupCacheOnLeave = true
        };

        PhotonNetwork.CreateRoom(roomName, tOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("[Photon] 방 생성 완료 → GameScene 로드");
        UIEvents.RaiseRoomCreated();
        PhotonNetwork.LoadLevel("GameScene");
    }

    private void HandleJoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon] 방 입장 완료 → GameScene 로드");
        UIEvents.RaiseRoomCreated();
        PhotonNetwork.LoadLevel("GameScene");
    }
}
