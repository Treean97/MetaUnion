using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;



// public enum UIID {Start, Lobby, Control, CreateRoom, JoinRoom, Confirm, Cancel};
public enum RewardType { Item, Currency }

public static class GameEvents
{
    #region 전역 이벤트
    // 경고 문구
    public static event Action<string, float> OnShowWarning;

    public static void RaiseShowWarning(string message, float duration = 2f)
    => OnShowWarning?.Invoke(message, duration);

    public static event Action OnHideWarning;
    public static void RaiseHideWarning() => OnHideWarning?.Invoke();
    

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

    // 방 생성 UI 열림
    public static event Action OnOpenCreateRoomUI;
    // 방 생성 UI 열림
    public static void RaiseOpenCreateRoomUI() => OnOpenCreateRoomUI?.Invoke();

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
    public static event Action<InfoDataSO> OnFocus;
    public static void RaiseFocus(InfoDataSO objInfo)
    => OnFocus?.Invoke(objInfo);

    // Focus UI 해제
    public static event Action OnDefocus;
    public static void RaiseDefocus() => OnDefocus?.Invoke();

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

    // 아이템 구매
    // public static event Func<int, int, int, int, bool> OnRequestPurchaseItem;
    // public static bool RaiseRequestPurchaseItem(int itemId, int amount, int currencyId, int price)
    // {
    //     int totalCost = price * amount;        

    //     if (!RaiseRequestCurrencySpend(currencyId, totalCost))
    //         {
    //             // 충분한 재화가 없을 때
    //             RaiseShowWarning("Not Enough Money");
    //             return false;
    //         }

    //     RaiseRequestItemGain(itemId, amount);
    //     return true;
    // }

    public static bool RaiseRequestPurchaseItem(int itemId, int amount, int currencyId, int unitPrice)
    {
        if (amount <= 0) { RaiseShowWarning("Amount must be >= 1"); return false; }

        int totalCost;
        try { totalCost = checked(unitPrice * amount); }
        catch (OverflowException)
        {
            RaiseShowWarning("Price overflow");
            return false;
        }

        // 통화 차감
        bool spent = OnRequestCurrencySpend?.Invoke(currencyId, totalCost) ?? false;
        if (!spent)
        {
            RaiseShowWarning("Not Enough Money");
            return false;
        }

        // 아이템 지급
        bool gained = OnRequestItemGain?.Invoke(itemId, amount) ?? false;
        if (!gained)
        {
            // 실패 시 환불
            OnRequestCurrencyGain?.Invoke(currencyId, totalCost);
            RaiseShowWarning("Inventory Full");
            return false;
        }

        // 후처리
        OnRequestUpdateInventory?.Invoke();
        return true;
    }

    // 아이템 판매
    public static event Func<int, int, bool> OnRequestCheckItemAmount;
    public static bool RaiseRequestSellItem(int itemId, int amount, int currencyId, int price)
    {
        int totalCost = price * amount;
        bool enough = OnRequestCheckItemAmount?.Invoke(itemId, amount) ?? false;

        if (!enough)
        {
            RaiseShowWarning("Not enough item");
            return false;
        }

        bool spent = OnRequestItemSpend?.Invoke(itemId, amount) ?? false;

        if (!spent)
        {
            RaiseShowWarning("Spend failed");
            return false;
        }

        OnRequestCurrencyGain?.Invoke(currencyId, totalCost);
        OnRequestUpdateInventory?.Invoke();

        return true;
    }

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

    // 리워드 획득
    public static event Action<RewardType, int,int> OnRewardSuccess;

    public static bool RaiseRewardSuccess(RewardType type, int id, int amount)
    {
        switch (type)
        {
            case RewardType.Item:
            {
                bool success = OnRequestItemGain?.Invoke(id, amount) ?? false;
                if (!success) { OnRewardFail?.Invoke(); return false; }
                break;
            }
            case RewardType.Currency:
            {
                OnRequestCurrencyGain?.Invoke(id, amount);
                break;
            }
        }

        OnRewardSuccess?.Invoke(type, id, amount);
        return true;
    }
    
    public static event Action OnRewardFail;
    public static void RaiseRewardFail()
    {
        OnRewardFail?.Invoke();
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

    // 아이템 색 적용 요청
    public static event Action<CustomizeItemSO, Color> OnRequestApplyItemColor;
    public static void RaiseRequestApplyItemColor(CustomizeItemSO item, Color color)
        => OnRequestApplyItemColor?.Invoke(item, color);

    public static event Action<CustomizeItemSO, Color> OnRequestPreviewItemColor;
    public static void RaiseRequestPreviewItemColor(CustomizeItemSO item, Color color)
        => OnRequestPreviewItemColor?.Invoke(item, color);

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

    // 버프
    public static event Action<BuffDataSO, GameObject> OnRequestApplyBuff;
    public static void RaiseRequestApplyBuff(BuffDataSO buff, GameObject user)
    => OnRequestApplyBuff?.Invoke(buff, user);

    // 아이템 드롭
    public static event Func<int, int, GameObject, bool> OnRequestItemDrop;

    public static bool RaiseRequestItemDrop(int id, int amount, GameObject user)
    {
        // 먼저 차감
        bool spent = RaiseRequestItemSpend(id, amount);
        if (!spent) return false;

        // 실제 드롭 수행
        bool spawned = OnRequestItemDrop?.Invoke(id, amount, user) ?? false;

        if (!spawned)
        {
            // 스폰 실패 → 환불 + 경고
            RaiseRequestItemGain(id, amount);
            RaiseShowWarning("아이템을 버릴 수 없습니다.");
            return false;
        }
        
        return true;
    }


    #endregion



}
