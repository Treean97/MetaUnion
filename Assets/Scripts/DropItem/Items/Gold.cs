using Photon.Pun;
using UnityEngine;

public class Gold : ItemBase
{
    private int _Amount;

    protected override void ProcessInstantiationData(object[] data)
    {
        if (data[0] is int amount)
            _Amount = amount;
    }

    public override void OnInteract()
    {
        // 골드 획득 로직
        GameEvents.RaiseRequestAddCurrency(_Amount);
        OnDefocus();
        // 네트워크 상에서도 파괴
        PhotonNetwork.Destroy(photonView);
    }
}
