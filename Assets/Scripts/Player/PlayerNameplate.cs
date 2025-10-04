using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(NameplateSpawner))]
public class PlayerNameplate : MonoBehaviour, INameplate, INameplateVisibility
{
    private PhotonView _PV;
    void Awake() => _PV = GetComponent<PhotonView>();

    public string GetDisplayName()
    {
        if (_PV && _PV.Owner != null && !string.IsNullOrEmpty(_PV.Owner.NickName))
        {
            return _PV.Owner.NickName;
        }

        return "Player";
    }

    public bool HideForLocal() => _PV && _PV.IsMine;
}
