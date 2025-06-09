using UnityEngine;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private Transform _Content;           // ScrollView Content
    [SerializeField] private GameObject _RoomItemPrefab;
    [SerializeField] private Button _JoinRoomButton;       // 입장 버튼
    [SerializeField] private Button _CreateRoomButton;

    private string _SelectedRoomName;

    private void OnEnable()
    {
        // 현재 캐시된 방 목록으로 갱신
        List<RoomInfo> currentRoomList = CachedRoomList.GetRoomList();

        UpdateRoomList(currentRoomList);

        // 입장 버튼 초기화
        _JoinRoomButton.interactable = false;
        _SelectedRoomName = null;

        // 입장 버튼 클릭 이벤트 연결
        _JoinRoomButton.onClick.RemoveAllListeners();
        _JoinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
        _CreateRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform t in _Content) Destroy(t.gameObject);
        foreach (var info in roomList)
        {
            if (info.RemovedFromList) continue;

            var item = Instantiate(_RoomItemPrefab, _Content);

            var manager = item.GetComponent<RoomItemManager>();
            manager.Init(info);

            // "RoomItem 클릭 시 선택" 이벤트 연결
            var listener = item.GetComponent<RoomItemListener>();
            listener.OnSelected = OnRoomItemSelected;
        }
    }

    private void OnRoomItemSelected(string roomName)
    {
        _SelectedRoomName = roomName;
        _JoinRoomButton.interactable = true;
    }

    private void OnCreateRoomButtonClicked()
    {
        UIEvents.RaiseOpenCreateRoomPopup();
    }

    private void OnJoinRoomButtonClicked()
    {
        if (!string.IsNullOrEmpty(_SelectedRoomName))
        {
            UIEvents.RaiseJoinRoom(_SelectedRoomName);
        }
    }
}
