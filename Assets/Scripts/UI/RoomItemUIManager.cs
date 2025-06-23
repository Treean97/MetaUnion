using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomItemUIManager : MonoBehaviour
{
    [SerializeField] private Button _RommItemBtn;
    [SerializeField] private TMP_Text _RoomNameText;
    [SerializeField] private TMP_Text _PlayerCountText;

    private RoomInfo _RoomInfo;


    void OnEnable()
    {
        _RommItemBtn.onClick.AddListener(() => OnSelectRoom());
    }

    public void SetInfo(RoomInfo info)
    {
        _RoomInfo = info;
        _RoomNameText.text = info.Name;
        _PlayerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
    }


    public void OnSelectRoom()
    {
        GameEvents.RaiseRoomSelect(_RoomInfo);
    }
}