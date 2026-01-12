using System;
using Photon.Pun;
using UnityEngine;

public class HarvestableObject : MonoBehaviourPun, IDamageable, IDestructible, IDropSource
{
    [Header("Data")]
    [SerializeField] private HarvestableDataSO _Data;
    public HarvestableDataSO Data => _Data;


    [Header("Sound")]
    [SerializeField] private string _Hitkey;

    // 드롭 테이블은 외부(루팅 시스템 등)에서 접근
    public DropItemTableSO DropTable => _Data ? _Data.DropTable : null;

    private float _Hp;
    private bool _Dead;

    public event Action OnDestroyed; // RespawnManager가 구독


    void Start()
    {
        _Dead = false;
        _Hp = _Data ? _Data.Durability : 1f;        
    }


    public void Damaged(DamageInfo info)
    {
        if (_Dead || !_Data) return;

        // 도구 체크
        if ((_Data.AvailableTool & info.tool) == 0) return;

        // 사운드 효과
        AudioManager._Inst.PlayLocalByKey(_Hitkey);

        if (PhotonNetwork.IsMasterClient)
        {
            ApplyDamage(info.damage);
        }
        else
        {
            // 권한 일관성 위해 마스터에 위임
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, info.damage);
        }
    }

    void ApplyDamage(float dmg)
    {
        if (_Dead) return;

        // 데미지 팝업
        photonView.RPC(nameof(RPC_ShowPopup), RpcTarget.All, transform.position, (int)dmg);

        _Hp -= dmg;
        if (_Hp <= 0f)
        {
            _Dead = true;
            photonView.RPC(nameof(RPC_BroadcastDestroyed), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(dmg);
    }

    [PunRPC]
    void RPC_BroadcastDestroyed()
    {
        OnDestroyed?.Invoke(); 
    }

    [PunRPC]
    void RPC_ShowPopup(Vector3 pos, int amount)
    {
        DamagePopManager._Inst?.Show(pos, amount);
    }
}
