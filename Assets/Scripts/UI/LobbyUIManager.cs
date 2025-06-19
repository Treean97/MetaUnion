using UnityEngine;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Transform _RoomListContent;           // ScrollView Content
    [SerializeField] private GameObject _RoomItemPrefab;
    [SerializeField] private Button _JoinRoomButton;       // 입장 버튼
    [SerializeField] private Button _CreateRoomButton;

    private RoomInfo _SelectedRoomInfo;

    private void OnEnable()
    {
        // 로비 열림 이벤트 호출
        GameEvents.RaiseOpenLobbyUI();

        // 기존 코드 유지
        GameEvents.OnSelectRoom += HandleSelectRoom;

        // ✅ 방 목록 수신 이벤트 구독
        GameEvents.OnRoomListUpdated += HandleUpdateRoomList;

        // 최신 방 목록 수동 초기화 (캐시 사용)
        var currentRoomList = CachedRoomList.GetRoomList();
        if (currentRoomList != null)
        {
            HandleUpdateRoomList(currentRoomList);
        }       

        _JoinRoomButton.interactable = false;
        _SelectedRoomInfo = null;

        _JoinRoomButton.onClick.RemoveAllListeners();
        _JoinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        _CreateRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
    }

    private void OnDisable()
    {
        GameEvents.OnSelectRoom -= HandleSelectRoom;
        GameEvents.OnRoomListUpdated -= HandleUpdateRoomList;
    }

    private void HandleSelectRoom(RoomInfo info)
    {
        _SelectedRoomInfo = info;
        _JoinRoomButton.interactable = true;
    }

    private void HandleUpdateRoomList(List<RoomInfo> roomList)
    {
        CachedRoomList.SetRoomList(roomList);

        ClearRoomListUI();

        foreach (var info in roomList)
        {
            if (info.RemovedFromList) continue;

            var item = Instantiate(_RoomItemPrefab, _RoomListContent);

            var manager = item.GetComponent<RoomItemUIManager>();
            manager.SetInfo(info);
        }
    }

    private void ClearRoomListUI()
    {
        foreach (Transform child in _RoomListContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnCreateRoomButtonClicked()
    {
        GameEvents.RaiseRequestOpenCreateRoomUI();
    }

    private void OnJoinRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(_SelectedRoomInfo.Name))
        {
            GameEvents.RaiseRequestJoinRoom(_SelectedRoomInfo);
        }
    }
}
