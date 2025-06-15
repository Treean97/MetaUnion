using System;
using System.Collections.Generic;
using Photon.Realtime;

public static class UIEvents
{
    // Start 버튼 클릭 → Lobby UI 활성화 요청
    public static event Action OnOpenLobbyUI;
    // 방 생성 팝업 열기 요청
    public static event Action OnOpenCreateRoomUI;
    // 방 생성 요청 (이름, 최대 인원)
    public static event Action<string, byte> OnCreateRoom;
    // 방 입장 요청
    public static event Action<RoomInfo> OnJoinRoom;
    // 업데이트된 방 목록 전달
    public static event Action<List<RoomInfo>> OnRoomListUpdated;
    // 방 생성 또는 입장 성공
    public static event Action OnRoomCreated;
    // 방 선택
    public static event Action<RoomInfo> OnSelectRoom;
    // 방 나가기
    public static event Action OnLeaveRoom;
    public static event Action OnStartBtnActive;
    public static event Action OnStartBtnInactive;


    // === 이벤트 발생용 ===
    // 시작 버튼 활성
    public static void RaiseStartBtnActive() => OnStartBtnActive?.Invoke();
    // 시작 버튼 비활성
    public static void RaiseStartBtnInactive() => OnStartBtnInactive?.Invoke();
    // 로비 UI 오픈
    public static void RaiseOpenLobbyUI() => OnOpenLobbyUI?.Invoke();
    // 방생성 UI 오픈
    public static void RaiseOpenCreateRoomUI() => OnOpenCreateRoomUI?.Invoke();
    // 방 생성
    public static void RaiseCreateRoom(string name, byte m) => OnCreateRoom?.Invoke(name, m);
    // 방 입장
    public static void RaiseJoinRoom(RoomInfo info) => OnJoinRoom?.Invoke(info);
    // 방 선택
    public static void RaiseRoomSelect(RoomInfo info) => OnSelectRoom?.Invoke(info);
    // 방 나가기
    public static void RaiseLeaveRoom() => OnLeaveRoom?.Invoke();

}
