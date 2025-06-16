using System;
using System.Collections.Generic;
using Photon.Realtime;

public static class GameEvents
{
    #region 전역 이벤트

    public static event Action<string, float> OnShowWarning;

    public static void RaiseShowWarning(string message, float duration = 2f)
    => OnShowWarning?.Invoke(message, duration);

    #endregion

    #region 메인 메뉴 이벤트
    // 시작 버튼 활성
    public static event Action OnStartBtnActive;
    // 시작 버튼 비활성
    public static event Action OnStartBtnInactive;
    // Lobby UI 활성화 요청
    public static event Action OnOpenLobbyUI;
    // 서버 연결
    public static event Action OnConnect;
    // 방 생성 팝업 열기 요청
    public static event Action OnOpenCreateRoomUI;
    // 방 생성 요청 (이름, 최대 인원)
    public static event Action<string, byte> OnCreateRoom;
    // 방 입장 요청
    public static event Action<RoomInfo> OnRequestJoinRoom;
    // 방 입장 성공
    public static event Action OnJoinRoomSuccess;
    // 업데이트된 방 목록 전달
    public static event Action<List<RoomInfo>> OnRoomListUpdated;
    // 방 선택
    public static event Action<RoomInfo> OnSelectRoom;
    // 방 나가기
    public static event Action OnLeaveRoom;
    


    // === 이벤트 발생용 ===
    // 시작 버튼 활성
    public static void RaiseStartBtnActive() => OnStartBtnActive?.Invoke();
    // 시작 버튼 비활성
    public static void RaiseStartBtnInactive() => OnStartBtnInactive?.Invoke();
    // 서버 연결
    public static void RaiseConnect() => OnConnect?.Invoke();
    // 로비 UI 오픈
    public static void RaiseOpenLobbyUI() => OnOpenLobbyUI?.Invoke();
    // 방 목록 갱신
    public static void RaiseRoomListUpdate(List<RoomInfo> roomList) => OnRoomListUpdated?.Invoke(roomList);
    // 방생성 UI 오픈
    public static void RaiseOpenCreateRoomUI() => OnOpenCreateRoomUI?.Invoke();
    // 방 입장 요청
    public static void RaiseRequestJoinRoom(RoomInfo info) => OnRequestJoinRoom?.Invoke(info);
    // 방 입장 성공
    public static void RaiseJoinRoomSuccess() => OnJoinRoomSuccess?.Invoke();
    // 방 선택
    public static void RaiseRoomSelect(RoomInfo info) => OnSelectRoom?.Invoke(info);
    // 방 나가기
    public static void RaiseLeaveRoom() => OnLeaveRoom?.Invoke();
    
    #endregion
}
