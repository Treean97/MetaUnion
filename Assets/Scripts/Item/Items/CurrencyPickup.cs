using Photon.Pun;
using UnityEngine;

public class CurrencyPickup : ItemBase
{
    /// <summary>
    /// Instantiate 시 전달된 데이터 처리 (예: amount)
    /// </summary>
    protected override void ProcessInstantiationData(object[] data)
    {
        if (data.Length > 0 && data[0] is int amount)
            _Amount = amount;
    }

    /// <summary>
    /// 플레이어가 상호작용 키를 눌렀을 때 호출
    /// </summary>
    public override void OnInteract()
    {
        // 획득
        GameEvents.RaiseRequestCurrencyGain(_ItemData.ID, _Amount);

        // UI 해제
        OnDefocus();

        // 아이템 파괴
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
