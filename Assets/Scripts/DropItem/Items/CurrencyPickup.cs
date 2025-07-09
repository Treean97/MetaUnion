using Photon.Pun;
using UnityEngine;

public class CurrencyPickup : ItemBase
{
    [Header("통화 타입")]
    [SerializeField] private CurrencyType _CurrencyType;

    private int _Amount;

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
        GameEvents.RaiseRequestCurrencyGain(_CurrencyType, _Amount);
        
        // UI 해제
        OnDefocus();

        // 아이템 파괴
        PhotonNetwork.Destroy(photonView);
    }
}
