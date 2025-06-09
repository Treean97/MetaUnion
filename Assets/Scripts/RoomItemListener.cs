using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System;

public class RoomItemListener : MonoBehaviour
{
    [SerializeField] private Button _SelectButton;
    private string _RoomName;

    // 외부에서 OnSelected 델리게이트로 콜백 받을 수 있게!
    public Action<string> OnSelected;

    private void Awake()
    {
        _SelectButton.onClick.AddListener(OnSelectButtonClicked);
    }

    public void SetRoomName(string roomName)
    {
        _RoomName = roomName;
    }

    private void OnSelectButtonClicked()
    {
        OnSelected?.Invoke(_RoomName);
    }
}
