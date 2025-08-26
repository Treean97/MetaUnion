using System;
using Photon.Pun;
using UnityEngine;

public class TreeObject : MonoBehaviourPun, IChoppable, IDestructible, IDropSource, IRespawnable
{
    [Header("Respawn/Prefab")]
    [SerializeField] private string _PrefabName;     // Resources 프리팁 이름
    [SerializeField] private float  _RespawnSeconds = 30f;
    [SerializeField] private Transform _RespawnAnchor; // 없으면 자기 Transform 사용

    [Header("Stats")]
    [SerializeField] HarvestableDataSO _Data;
    public DropItemTableSO DropTable => _Data.DropTable;

    private float _CurHP;
    private bool _IsDead;

    public event Action OnDestroyed;

    public string PrefabName    => _PrefabName;
    public float  RespawnDelay  => _RespawnSeconds;
    public Transform RespawnAnchor => _RespawnAnchor != null ? _RespawnAnchor : transform;

    void Start()
    {
        // 전역 매니저에 자기 자신 등록
        RespawnManager._Inst?.Register(this);

        // 초기화
        _IsDead = false;
        _CurHP = (_Data != null) ? _Data.Durability : 1f;
    }

    public void Chop(float power)
    {
        if (_IsDead) return;

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

        _CurHP -= power;
        if (_CurHP <= 0f)
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

    // 씬 배치 PV 파괴용
    [PunRPC]
    void RPC_DespawnSceneObject()
    {
        if (this != null && gameObject != null)
            Destroy(gameObject); // 필요 시 SetActive(false) 가능
    }

}
