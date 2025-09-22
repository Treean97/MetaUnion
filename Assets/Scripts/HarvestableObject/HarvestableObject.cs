using System;
using Photon.Pun;
using UnityEngine;

public class HarvestableObject : MonoBehaviourPun, IDamageable, IDestructible, IDropSource
{
    [Header("Data")]
    [SerializeField] private HarvestableDataSO _Data;
    public HarvestableDataSO Data => _Data;

    // 드롭 테이블은 외부(루팅 시스템 등)에서 접근
    public DropItemTableSO DropTable => _Data ? _Data.DropTable : null;

    private float _Hp;
    private bool _Dead;

    public event Action OnDestroyed;    // RespawnManager가 구독

    void Start()
    {
        _Dead = false;
        _Hp   = _Data ? _Data.Durability : 1f;
        // 리스폰 등록은 Respawnable.Start()에서 자동 수행(이 스크립트는 관여 X)
    }

    public void Damaged(DamageInfo info)
    {
        if (_Dead || !_Data) return;

        // 도구 체크: 허용되지 않으면 무시
        if ( (_Data.AvailableTool & info.tool) == 0 ) return;

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

        // (선택) 데미지 팝업 등 클라 연출
        photonView.RPC(nameof(RPC_ShowPopup), RpcTarget.All, transform.position, (int)dmg);

        _Hp -= dmg;
        if (_Hp <= 0f)
        {
            _Dead = true;
            OnDestroyed?.Invoke();   // 여기서 RespawnManager가 파괴/리스폰 스케줄
        }
    }

    [PunRPC]
    void RPC_ApplyDamage(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(dmg);
    }

    [PunRPC]
    void RPC_ShowPopup(Vector3 pos, int amount)
    {
        DamagePopManager._Inst?.Show(pos, amount);
    }
}
