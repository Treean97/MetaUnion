using UnityEngine;
using Photon.Pun;               
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LobbyUIManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _RoomListContent;
    [SerializeField] private GameObject _RoomItemPrefab;
    [SerializeField] private Button _JoinRoomButton;

    [SerializeField] private Button _StartBtn;
    [SerializeField] private TMP_Text _StatusText;

    private RoomInfo _SelectedRoomInfo;

    // OnEnable 오버라이드
    public override void OnEnable()
    {
        base.OnEnable();  // ← Photon 콜백 등록

        // 로비 화면 출력 시 로그인 상태 텍스트 Off
        _StatusText.gameObject.SetActive(false);

        GameEvents.OnSelectRoom += HandleSelectRoom;
        GameEvents.OnRoomListUpdated += HandleUpdateRoomList;

        var currentRoomList = CachedRoomList.GetRoomList();
        if (currentRoomList != null)
            HandleUpdateRoomList(currentRoomList);

        _JoinRoomButton.interactable = false;
        _SelectedRoomInfo = null;

        _JoinRoomButton.onClick.RemoveListener(OnJoinRoomButtonClicked);
        _JoinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
    }

    // OnDisable 오버라이드
    public override void OnDisable()
    {
        base.OnDisable(); // ← Photon 콜백 해제

        GameEvents.OnSelectRoom -= HandleSelectRoom;
        GameEvents.OnRoomListUpdated -= HandleUpdateRoomList;
    }

    IEnumerator WaitSequence(Button button)
    {
        var sequence = button.GetComponent<ButtonSequence>();
        if (sequence)
        {
            yield return sequence.RunSequence();
        }
    }

    public override void OnLeftLobby()
    {
        PhotonNetwork.JoinLobby();
    }

    private void HandleSelectRoom(RoomInfo info)
    {
        _SelectedRoomInfo = info;
        _JoinRoomButton.interactable = true;
    }

    private void HandleUpdateRoomList(List<RoomInfo> roomList)
    {
        CachedRoomList.SetRoomList(roomList);

        foreach (Transform child in _RoomListContent)
            Destroy(child.gameObject);

        foreach (var info in roomList)
        {
            if (info.RemovedFromList) continue;
            var item = Instantiate(_RoomItemPrefab, _RoomListContent);
            var manager = item.GetComponent<RoomItemUIManager>();
            manager.SetInfo(info);
        }
    }

    private void OnJoinRoomButtonClicked()
    {
        StartCoroutine(JoinRoomButtonSequence());
    }

    IEnumerator JoinRoomButtonSequence()
    {
        yield return WaitSequence(_JoinRoomButton);

        if (!string.IsNullOrEmpty(_SelectedRoomInfo.Name))
        {
            // GameEvents.RaiseRequestJoinRoom(_SelectedRoomInfo);
            Launcher._Inst.RequestJoinRoom(_SelectedRoomInfo);
        }
    }

    // private void OnRefreshButtonClicked()
    // {
    //     foreach (Transform child in _RoomListContent)
    //         Destroy(child.gameObject);

    //     CachedRoomList.SetRoomList(new List<RoomInfo>());
    //     _SelectedRoomInfo = null;
    //     _JoinRoomBtn.interactable = false;

    //     PhotonNetwork.LeaveLobby();
    // }

}
