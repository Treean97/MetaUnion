using UnityEngine;
using TMPro;
using Photon.Realtime;

public class RoomItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _RoomNameText;
    [SerializeField] private TMP_Text _PlayerCountText;

    private string _RoomName;

    public void SetInfo(RoomInfo info)
    {
        _RoomName = info.Name;
        _RoomNameText.text = info.Name;
        _PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
    }

    public void OnClick()
    {
        FindAnyObjectByType<Launcher>().JoinRoom(_RoomName);
    }
}