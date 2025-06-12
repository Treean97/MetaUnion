using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _RoomNameText;
    [SerializeField] private TMP_Text _PlayerCountText;

    private RoomInfo _RoomInfo;


    public void SetInfo(RoomInfo info)
    {
        _RoomInfo = info;
        _RoomNameText.text = info.Name;
        _PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
    }


    public void OnClick()
    {
        UIEvents.RaiseRoomSelect(_RoomInfo);
    }
}