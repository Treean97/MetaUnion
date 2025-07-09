using System;
using Photon.Pun;
using UnityEngine;

public class RockObject : MonoBehaviourPun, IMineable, IDestructible, IDropSource
{
    [SerializeField] HarvestableDataSO _HarvestableObjectData;

    public DropItemTableSO DropTable => _HarvestableObjectData.DropTable;
    
    private float _CurDurability;

    public event Action OnDestroyed;

    void Start()
    {
        _CurDurability = _HarvestableObjectData.Durability;
    }

    public void Mine(float power)
    {
        _CurDurability -= power;

        Debug.Log($"Hit !! CurDurability : {_CurDurability}");

        if (_CurDurability <= 0f)
        {
            Debug.Log("Destroyed");

            OnDestroyed?.Invoke();
            photonView.RPC(nameof(RPC_DestroySelf), RpcTarget.All);            
        }
    }

    [PunRPC]
    void RPC_DestroySelf()
    {
        Destroy(gameObject);
    }
}
