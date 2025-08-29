using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomItemUIManager : MonoBehaviour
{
    [Header("UI Set")]
    [SerializeField] private Button _RommItemBtn;
    [SerializeField] private TMP_Text _RoomNameText;
    [SerializeField] private TMP_Text _PlayerCountText;

    [Header("Colors")]
    [SerializeField] private Color _JoinableColor;     // 입장 가능 색
    [SerializeField] private Color _UnjoinableColor;   // 입장 불가(정원 꽉참) 색

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

        // 정원 여부에 따라 색상 적용
        bool isFull = info.PlayerCount >= info.MaxPlayers;
        ApplyJoinableColor(!isFull);
    }


    public void OnSelectRoom()
    {
        GameEvents.RaiseRoomSelect(_RoomInfo);
    }

    private void ApplyJoinableColor(bool joinable)
    {
        var g = _RommItemBtn ? _RommItemBtn.targetGraphic : null;
        if (g == null) return; // 버튼에 타겟 그래픽이 없으면 아무 것도 하지 않음

        g.color = joinable ? _JoinableColor : _UnjoinableColor;
    }
}