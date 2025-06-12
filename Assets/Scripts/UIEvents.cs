using System;
using System.Collections.Generic;
using Photon.Realtime;

public static class UIEvents
{
    // Start 버튼 클릭 → Lobby UI 활성화 요청
    public static event Action OnOpenLobbyUI;

    // 방 생성 팝업 열기 요청
    public static event Action OnOpenCreateRoomPopup;

    // 방 생성 요청 (이름, 최대 인원)
    public static event Action<string, byte> OnCreateRoom;

    // 방 입장 요청
    public static event Action<RoomInfo> OnJoinRoom;

    // 업데이트된 방 목록 전달
    public static event Action<List<RoomInfo>> OnRoomListUpdated;

    // 방 생성 또는 입장 성공
    public static event Action OnRoomCreated;

    public static event Action<RoomInfo> OnSelectRoom;


    // === 이벤트 발생용 ===
    public static void RaiseOpenLobbyUI() => OnOpenLobbyUI?.Invoke();
    public static void RaiseOpenCreateRoomPopup() => OnOpenCreateRoomPopup?.Invoke();
    public static void RaiseCreateRoom(string name, byte m) => OnCreateRoom?.Invoke(name, m);
    public static void RaiseJoinRoom(RoomInfo info) => OnJoinRoom?.Invoke(info);
    public static void RaiseRoomListUpdated(List<RoomInfo> l) => OnRoomListUpdated?.Invoke(l);
    public static void RaiseRoomCreated() => OnRoomCreated?.Invoke();
    public static void RaiseRoomSelect(RoomInfo info) => OnSelectRoom?.Invoke(info);
}
