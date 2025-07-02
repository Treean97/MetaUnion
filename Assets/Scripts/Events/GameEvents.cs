using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.UI;


public enum UIID {Start, Lobby, Control, CreateRoom, JoinRoom, Confirm, Cancel};

public static class GameEvents
{
    #region 전역 이벤트

    public static event Action<string, float> OnShowWarning;

    public static void RaiseShowWarning(string message, float duration = 2f)
    => OnShowWarning?.Invoke(message, duration);

    #endregion

    #region 메인 메뉴 이벤트
    // UI 활성 / 비활성
    // public static event Action<UIID, bool> OnSetActive;
    // public static void RaiseSetActive(UIID uiID, bool enable) => OnSetActive?.Invoke(uiID, enable);

    // 버튼 상호작용 유무
    //public static event Action<UIID, bool> OnBtnSetInteractable;    
    //public static void RaiseBtnSetInteractable(UIID uiID, bool enable) => OnBtnSetInteractable?.Invoke(uiID, enable);

    // 플레이어 아이디 입력란 공백
    public static event Action<bool> OnPlayerIDFieldIsNull;
    public static void RaisePlayerFieldIsNull(bool isNull) => OnPlayerIDFieldIsNull?.Invoke(isNull);

    // 서버 연결
    public static event Action OnConnect;
    public static void RaiseConnect() => OnConnect?.Invoke();

    // Lobby UI 요청
    public static event Action OnRequestOpenLobbyUI;
    public static void RaiseRequestOpenLobbyUI() => OnRequestOpenLobbyUI?.Invoke();

    // Lobby UI 열림
    public static event Action OnOpenLobbyUI;
    public static void RaiseOpenLobbyUI() => OnOpenLobbyUI?.Invoke();

    // 방 생성 UI 요청
    public static event Action OnRequestOpenCreateRoomUI;
    // 방 생성 UI 요청
    public static void RaiseRequestOpenCreateRoomUI() => OnRequestOpenCreateRoomUI?.Invoke();

    // 방 생성 UI 열림
    public static event Action OnOpenCreateRoomUI;
    // 방 생성 UI 열림
    public static void RaiseOpenCreateRoomUI() => OnOpenCreateRoomUI?.Invoke();

    // 방 입장 요청
    public static event Action<RoomInfo> OnRequestJoinRoom;
    // 방 입장 요청
    public static void RaiseRequestJoinRoom(RoomInfo info) => OnRequestJoinRoom?.Invoke(info);

    // 방 입장 성공
    public static event Action OnJoinRoomSuccess;
    // 방 입장 성공
    public static void RaiseJoinRoomSuccess() => OnJoinRoomSuccess?.Invoke();

    // 업데이트된 방 목록 전달
    public static event Action<List<RoomInfo>> OnRoomListUpdated;
    // 방 목록 갱신
    public static void RaiseRoomListUpdate(List<RoomInfo> roomList) => OnRoomListUpdated?.Invoke(roomList);

    // 방 선택
    public static event Action<RoomInfo> OnSelectRoom;
    // 방 선택
    public static void RaiseRoomSelect(RoomInfo info) => OnSelectRoom?.Invoke(info);

    // 방 나가기
    public static event Action OnLeaveRoom;
    // 방 나가기
    public static void RaiseLeaveRoom() => OnLeaveRoom?.Invoke();

    #endregion

    #region 게임 화면 이벤트

    // 채팅UI 포커스
    public static event Action<bool> OnChatIsRunning;
    public static void RaiseUIIsRunning(bool isRun) => OnChatIsRunning?.Invoke(isRun);

    // Focus UI 호출
    public static event Action<ObjectInfoSO> OnFocus;
    public static void RaiseFocus(ObjectInfoSO objInfo) => OnFocus?.Invoke(objInfo);

    // Focus UI 해제
    public static event Action OnDefocus;
    public static void RaiseDefocus() => OnDefocus?.Invoke();

    // Customize UI
    public static event Action OnRequestOpenCustomizeUI;
    public static void RaiseRequestOpenCustomizeUI() => OnRequestOpenCustomizeUI?.Invoke();

    // Customize Request
    public static event Action<CustomizeItemSO> OnRequestEquipItem;
    public static void RaiseRequestEquipItem(CustomizeItemSO item)
    => OnRequestEquipItem?.Invoke(item);

    // 재화 관리
    public static event Action<int> OnRequestAddCurrency;
    public static void RaiseRequestAddCurrency(int amount) => OnRequestAddCurrency?.Invoke(amount);

    public static event Func<int, bool> OnRequestSpendCurrency;
    public static bool RaiseRequestSpendCurrency(int amount) => OnRequestSpendCurrency?.Invoke(amount) ?? false;

    public static event Action<int> OnChangeCurrency;
    public static void RaiseChangeCurrency(int amount) => OnChangeCurrency?.Invoke(amount);

    #endregion
}
