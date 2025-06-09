using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
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

    // Start 버튼 → Connect 시작
    private void HandleOpenLobby()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 캐싱 (선택적 사용 가능)
        CachedRoomList.SetRoomList(roomList);

        // UI 업데이트용 이벤트 발생
        UIEvents.RaiseRoomListUpdated(roomList);
    }

    private void HandleCreateRoom(string roomName, byte maxPlayers)
    {
        var tOptions = new RoomOptions { MaxPlayers = maxPlayers, CleanupCacheOnLeave = true };
        PhotonNetwork.CreateRoom(roomName, tOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        UIEvents.RaiseRoomCreated();
        PhotonNetwork.LoadLevel("GameScene");
    }

    private void HandleJoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        UIEvents.RaiseRoomCreated();
        PhotonNetwork.LoadLevel("GameScene");
    }
}
