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
    public static void RaiseRequestOpenCreateRoomUI()
    => OnRequestOpenCreateRoomUI?.Invoke();

    // 방 생성 UI 열림
    public static event Action OnOpenCreateRoomUI;
    // 방 생성 UI 열림
    public static void RaiseOpenCreateRoomUI() => OnOpenCreateRoomUI?.Invoke();

    // 방 입장 요청
    public static event Action<RoomInfo> OnRequestJoinRoom;
    // 방 입장 요청
    public static void RaiseRequestJoinRoom(RoomInfo info)
    => OnRequestJoinRoom?.Invoke(info);

    // 방 입장 성공
    public static event Action OnJoinRoomSuccess;
    // 방 입장 성공
    public static void RaiseJoinRoomSuccess() => OnJoinRoomSuccess?.Invoke();

    // 업데이트된 방 목록 전달
    public static event Action<List<RoomInfo>> OnRoomListUpdated;
    // 방 목록 갱신
    public static void RaiseRoomListUpdate(List<RoomInfo> roomList)
    => OnRoomListUpdated?.Invoke(roomList);

    // 방 선택
    public static event Action<RoomInfo> OnSelectRoom;
    // 방 선택
    public static void RaiseRoomSelect(RoomInfo info)
    => OnSelectRoom?.Invoke(info);

    // 방 나가기
    public static event Action OnLeaveRoom;
    // 방 나가기
    public static void RaiseLeaveRoom() => OnLeaveRoom?.Invoke();

    #endregion


    #region 게임 화면 이벤트  
    // Focus UI 호출
    public static event Action<ItemInfoSO> OnFocus;
    public static void RaiseFocus(ItemInfoSO objInfo)
    => OnFocus?.Invoke(objInfo);

    // Focus UI 해제
    public static event Action OnDefocus;
    public static void RaiseDefocus() => OnDefocus?.Invoke();

    // Customize UI 열기 요청
    public static event Action OnRequestOpenCustomizeUI;
    public static void RaiseRequestOpenCustomizeUI()
    => OnRequestOpenCustomizeUI?.Invoke();

    // Shop UI 열기 요청
    public static event Action OnRequestOpenShopUI;
    public static void RaiseRequestOpenShopUI()
    => OnRequestOpenShopUI?.Invoke();

    // 잠긴(locked) 커스터마이즈 아이템 리스트 요청 (상점용)
    public static event Action<ItemType> OnRequestLockedItems;
    public static void RaiseRequestLockedItems(ItemType type)
    => OnRequestLockedItems?.Invoke(type);

    // 잠긴(locked) 커스터마이즈 아이템 리스트 제공 (상점용)
    public static event Action<List<CustomizeItemSO>> OnProvideLockedItems;
    public static void RaiseProvideLockedItems(List<CustomizeItemSO> items)
    => OnProvideLockedItems?.Invoke(items);

    // 해금된(unlocked) 커스터마이즈 아이템 리스트 요청 (커스터마이즈용)
    public static event Action<ItemType> OnRequestUnlockedItems;
    public static void RaiseRequestUnlockedItems(ItemType type)
    => OnRequestUnlockedItems?.Invoke(type);

    // 해금된(unlocked) 커스터마이즈 아이템 리스트 제공 (커스터마이즈용)
    public static event Action<List<CustomizeItemSO>> OnProvideUnlockedItems;
    public static void RaiseProvideUnlockedItems(List<CustomizeItemSO> items)
    => OnProvideUnlockedItems?.Invoke(items);

    // 커스터마이즈 아이템 해금(구매) 요청
    public static event Action<CustomizeItemSO> OnRequestUnlockItem;
    public static void RaiseRequestUnlockItem(CustomizeItemSO item)
    => OnRequestUnlockItem?.Invoke(item);

    public static event Action OnItemPurchaseSuccess;
    public static void RaiseItemPurchaseSuccess()
    => OnItemPurchaseSuccess?.Invoke();

    // 커스터마이즈 아이템 장착 요청
    public static event Action<CustomizeItemSO> OnRequestEquipItem;
    public static void RaiseRequestEquipItem(CustomizeItemSO item)
    => OnRequestEquipItem?.Invoke(item);

    // 커런시 획득
    public static event Action<int, int> OnRequestCurrencyGain;
    public static void RaiseRequestCurrencyGain(int currencyId, int amount)
    => OnRequestCurrencyGain?.Invoke(currencyId, amount);

    // 커런시 소비
    public static event Func<int, int, bool> OnRequestCurrencySpend;
    public static bool RaiseRequestCurrencySpend(int currencyId, int amount)
    => OnRequestCurrencySpend?.Invoke(currencyId, amount) ?? false;

    // 커런시 UI 갱신
    public static event Action<int, int> OnRequestUpdateCurrency;
    public static void RaiseRequestUpdateCurrency(int currencyId, int newValue)
    => OnRequestUpdateCurrency?.Invoke(currencyId, newValue);

    // 아이템 획득
    public static event Func<int, int, bool> OnRequestItemGain;
    public static bool RaiseRequestItemGain(int itemId, int amount)
    {
        bool success = OnRequestItemGain?.Invoke(itemId, amount) ?? false;
        if (success)
        {
            OnRequestUpdateInventory?.Invoke();
        }
        return success;
    }

    // 아이템 소비
    public static event Func<int, int, bool> OnRequestItemSpend;
    public static bool RaiseRequestItemSpend(int itemId, int amount)
    {
        bool success = OnRequestItemSpend?.Invoke(itemId, amount) ?? false;
        if (success)
        {
            OnRequestUpdateInventory?.Invoke();
        }
        return success;
    }

    // 슬롯머신 UI 요청
    public static event Action OnRequestOpenSlotMachineUI;
    public static void RaiseRequestOpenSlotMachineUI()
    => OnRequestOpenSlotMachineUI?.Invoke();

    // 인벤토리 UI 열고 닫기 요청
    public static event Action OnRequestToggleInventoryUI;
    public static void RaiseRequestToggleInventoryUI()
    => OnRequestToggleInventoryUI?.Invoke();

    // 인벤토리 업데이트 요청
    public static event Action OnRequestUpdateInventory;
    public static void RaiseRequestUpdateInventory()
    => OnRequestUpdateInventory?.Invoke();

    // 인벤토리 상태 요청 id, amount
    public static event Func<InventoryItem[]> OnRequestInventoryStatus;
    public static InventoryItem[] RaiseRequestInventoryStatus()
    => OnRequestInventoryStatus?.Invoke();

    // 인벤토리 슬롯 수 요청
    public static event Func<int> OnRequestInventorySlotCount;
    public static int RaiseRequestInventorySlotCount()
    => OnRequestInventorySlotCount?.Invoke() ?? 0;

    // 인벤토리 슬롯 교환 요청
    public static event Action<int, int> OnRequestSwapSlot;
    public static void RaiseRequestSwapSlot(int from, int to)
    => OnRequestSwapSlot?.Invoke(from, to);


    // 플레이어 리스트 UI
    public static event Action OnRequestOpenPlayerListUI;
    public static void RaiseRequestOpenPlayerListUI()
    => OnRequestOpenPlayerListUI?.Invoke();

    public static event Action OnRequestClosePlayerListUI;
    public static void RaiseRequestClosePlayerListUI()
    => OnRequestClosePlayerListUI?.Invoke();  

    #endregion
}
