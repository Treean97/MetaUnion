using Photon.Pun;
using UnityEngine;

public class MountSeat : MonoBehaviour
{
    public void OnInteract()
    {
        MountEntity mount = GetComponentInParent<MountEntity>();
        if (mount == null) return;

        GameObject riderGO = PlayerSetup._LocalPlayer;
        PhotonView riderPv = riderGO.GetComponent<PhotonView>();
        if (riderPv == null || !riderPv.IsMine) return;

        mount.TryMount(riderGO); // 차량이 빈 좌석 자동배치
    }
}
