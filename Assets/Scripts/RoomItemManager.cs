using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomItemManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _RoomTitleText;
    [SerializeField] private TMP_Text _PlayerCountText;
    [SerializeField] private RoomItemListener _RoomItemListener;

    public void Init(RoomInfo info)
    {
        // UI 표시 역할
        _RoomTitleText.text = info.Name;
        _PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";

        // RoomItemListener 에 RoomName 전달
        _RoomItemListener.SetRoomName(info.Name);
    }
}
