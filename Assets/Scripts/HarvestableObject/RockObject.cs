using System;
using Photon.Pun;
using UnityEngine;

public class RockObject : MonoBehaviourPun, IDamageable, IDestructible, IDropSource, IRespawnable
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


    void Start()
    {
        RespawnManager._Inst?.Register(this);
        _IsDead = false;
        _CurHP = (_Data != null) ? _Data.Durability : 1f;
    }

    
    public void Damaged(DamageInfo damageInfo)
    {        
        if (_IsDead) return;
        if ( (_Data.AvailableTool & damageInfo.tool) == 0 ) return;

        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(damageInfo.damage);         // 마스터면 즉시 적용
        }
        else
        {
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, damageInfo.damage); // 비마스터는 요청만
        }
    }
    
    void ApplyDamage(float power)
    {
        if (_IsDead) return;

        // 데미지 폰트 처리
        DamagePopManager._Inst.Show(transform.position, (int)power);

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
