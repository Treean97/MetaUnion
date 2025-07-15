using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : MonoBehaviour
{
    [SerializeField] GameObject _PlayerListSlot;
    [SerializeField] Transform _Content;

    private Player[] _Players;

    void OnEnable()
    {
        UpdatePlayerList();

    }

    void UpdatePlayerList()
    {
        ClearContent();

        _Players = PhotonNetwork.PlayerList;

        for (int i = 0; i < _Players.Length; i++)
        {
            GameObject slot = Instantiate(_PlayerListSlot, _Content);
            slot.GetComponentInChildren<TMP_Text>().text = _Players[i].NickName;
        }
    }

    void ClearContent()
    {
        for (int i = 0; i < _Content.childCount; i++)
        {
            GameObject child = _Content.GetChild(i).gameObject;
            Destroy(child);
        }
    }
}
