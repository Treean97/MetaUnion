using System;
using Photon.Pun;
using UnityEngine;

public class ItemPickup : ItemBase
{
    public static event Action OnItemPickUp;

    // 네트워크 Instantiate 시 전송된 수량 데이터 처리
    protected override void ProcessInstantiationData(object[] data)
    {
        if (data.Length > 0 && data[0] is int amount)
            _Amount = amount;
    }

    // 상호작용 키를 눌렀을 때 호출됩니다.
    public override void OnInteract()
    {
        // 아이템 줍기 이벤트
        OnItemPickUp?.Invoke();

        // 아이템 획득 이벤트
        GameEvents.RaiseRequestItemGain(_ItemData.ID, _Amount);

        // 네트워크 상에서 오브젝트 파괴
        TryDestroy();
    }
    
    void TryDestroy()
    {
        if (photonView.IsMine || PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // 내가 삭제 권한 없으면, MasterClient에게 삭제 요청
            photonView.RPC(nameof(RequestDestroyByMaster), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    void RequestDestroyByMaster()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
