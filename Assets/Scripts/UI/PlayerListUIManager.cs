using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class PlayerListUIManager : MonoBehaviour, IPlayerListUI
{
    [SerializeField] GameObject _PlayerListSlot;
    [SerializeField] Transform _Content;

    private Player[] _Players;
    public bool IsOpen => gameObject.activeSelf;

    void OnEnable()
    {
        UIRouter._Inst?.RegisterAs<IPlayerListUI>(this);
        UpdatePlayerList();
    }

    void OnDisable()
    {
        UIRouter._Inst?.UnregisterAs<IPlayerListUI>(this);
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

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
