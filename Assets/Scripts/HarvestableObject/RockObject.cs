using System;
using Photon.Pun;
using UnityEngine;

public class RockObject : MonoBehaviourPun, IMineable, IDestructible, IDropSource, IRespawnable
{
    [SerializeField] HarvestableDataSO _HarvestableObjectData;

    public DropItemTableSO DropTable => _HarvestableObjectData.DropTable;

    private SpawnPoint _owner;    
    private float _CurDurability;
    private bool _IsDead;

    public event Action OnDestroyed;

    void Start()
    {
        _CurDurability = _HarvestableObjectData.Durability;
    }

    public void Init(SpawnPoint owner)
    {
        _owner = owner;
        _CurDurability = _HarvestableObjectData.Durability;
    }

    public float GetRespawnDelay()
    {
        return _owner != null ? _owner.GetRespawnDelayFor(this) : 5f;
    }
    
    public void OnSpawned() { /* 스폰 직후 초기화 */ }

    public void Mine(float power)
    {        
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(power);
        }
        else
        {
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, power);
        }
    }
    
    void ApplyDamage(float power)
    {
        if (_IsDead) return;

        _CurDurability -= power;

        if (_CurDurability <= 0f)
        {
            _IsDead = true;
            OnDestroyed?.Invoke();  // 마스터의 SpawnPoint가 파괴/리스폰
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(float power)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(power);
    }

    [PunRPC]
    void RPC_DespawnSceneObject()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject); // 필요 시 SetActive(false) 가능
    }
}
