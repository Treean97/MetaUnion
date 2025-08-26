using System;
using Photon.Pun;
using UnityEngine;

public class TreeObject : MonoBehaviourPun, IChoppable, IDestructible, IDropSource, IRespawnable
{
    [SerializeField] HarvestableDataSO _HarvestableObjectData;
    public DropItemTableSO DropTable => _HarvestableObjectData.DropTable;


    private SpawnPoint _owner;
    private bool _IsDead;
    private float _CurDurability;


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

    public void Chop(float power)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(power);         // 마스터면 즉시 적용
        }
        else
        {
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, power); // 비마스터는 요청만
        }
    }

    void ApplyDamage(float power)
    {
        if (_IsDead) return;

        _CurDurability -= power;
        if (_CurDurability <= 0f)
        {
            _IsDead = true;
            OnDestroyed?.Invoke();  // 스폰포인트(마스터)에서 파괴/리스폰 처리
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
