using Photon.Pun;
using UnityEngine;

public class MountInteractable : MonoBehaviour, IInteractable
{
    public InfoDataSO GetObjectInfo()
    {
        var mount = GetComponent<MountEntity>();
        return mount != null && mount.Data != null ? mount.Data.InfoData : null;
    }

    public void OnFocus()
    {
        var info = GetObjectInfo();
        if (info != null) GameEvents.RaiseFocus(info);
    }

    public void OnDefocus()
    {
        GameEvents.RaiseDefocus();
    }

    public void OnInteract()
    {
        Debug.Log("MountInteractable.OnInteract called", this);

        var mount = GetComponent<MountEntity>();
        if (mount == null) 
        {
            Debug.Log("mount is null"); 
            return;
        }

        var riderGO = PlayerSetup._LocalPlayer;
        if (riderGO == null)        
        {
            Debug.Log("riderGO is null"); 
            return;
        }

        var riderPv = riderGO.GetComponent<PhotonView>();
        if (riderPv == null || !riderPv.IsMine)   
        {
            Debug.Log("riderPv is null"); 
            return;
        }

        mount.TryMount(riderGO); // 앞 인덱스부터 빈 좌석 배치
    }
}
