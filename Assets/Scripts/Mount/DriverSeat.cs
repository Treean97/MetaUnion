using Photon.Pun;
using UnityEngine;

public class DriverSeat : MonoBehaviour, IInteractable
{
    [SerializeField]
    InfoDataSO _ObjInfo;

    public InfoDataSO GetObjectInfo()
    {
        return _ObjInfo;
    }

    public void OnDefocus()
    {
        GameEvents.RaiseDefocus();
    }

    public void OnFocus()
    {
        GameEvents.RaiseFocus(_ObjInfo);
    }

    public void OnInteract()
    {
        MountEntity mount = GetComponentInParent<MountEntity>();
        if (mount == null) return;

        // 로컬 플레이어 탑승 시도
        GameObject riderGO = PlayerSetup._LocalPlayer;
        PhotonView riderPv = riderGO.GetComponent<PhotonView>();
        if (riderPv == null || !riderPv.IsMine) return;

        mount.TryMount(riderGO);
    }

}
