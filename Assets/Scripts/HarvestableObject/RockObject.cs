using System;
using Photon.Pun;
using UnityEngine;

public class RockObject : MonoBehaviourPun, IMineable, IDestructible, IDropSource, IRespawnable
{
    [Header("Respawn/Prefab")]
    [SerializeField] private string _PrefabName;
    [SerializeField] private float  _RespawnSeconds = 30f;
    [SerializeField] private Transform _RespawnAnchor;

    [Header("Stats")]
    [SerializeField] HarvestableDataSO _Data;

    public DropItemTableSO DropTable => _Data.DropTable;
  
    private float _CurHP;
    private bool _IsDead;

    public event Action OnDestroyed;

    public string PrefabName => _PrefabName;
    public float  RespawnDelay => _RespawnSeconds;
    public Transform RespawnAnchor => _RespawnAnchor != null ? _RespawnAnchor : transform;
    public void OnRegistered() { }
    public void OnSpawned()    { }

    void Start()
    {
        RespawnManager._Inst?.Register(this);
        _IsDead = false;
        _CurHP = (_Data != null) ? _Data.Durability : 1f;
    }

    
    public void Mine(float power)
    {        
        if (_IsDead) return;

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

        _CurHP -= power;

        if (_CurHP <= 0f)
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
