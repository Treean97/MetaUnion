using Photon.Pun;
using UnityEngine;

public class ItemPickup : ItemBase
{
    [SerializeField] private ItemInfoSO _ItemData;
    [SerializeField] private int _Amount;

    // 네트워크 Instantiate 시 전송된 수량 데이터 처리
    protected override void ProcessInstantiationData(object[] data)
    {
        if (data.Length > 0 && data[0] is int amount)
            _Amount = amount;
    }

    // 상호작용 키를 눌렀을 때 호출됩니다.
    public override void OnInteract()
    {
        // 아이템 획득 로직
        
        // 네트워크 상에서 오브젝트 파괴
        PhotonNetwork.Destroy(photonView);
    }
}
