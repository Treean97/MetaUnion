using System;
using Photon.Pun;
using UnityEngine;

public class RockObject : MonoBehaviourPun, IDamageable, IDestructible, IDropSource
{
        [Header("Stats")]
    [SerializeField] private HarvestableDataSO _Data;
    public DropItemTableSO DropTable => _Data.DropTable;

    private float _CurHP;
    private bool _IsDead;

    public event Action OnDestroyed;

    void Start()
    {
        _IsDead = false;
        _CurHP = (_Data != null) ? _Data.Durability : 1f;
    }

    public void Damaged(DamageInfo damageInfo)
    {
        if (_IsDead) return;
        if ((_Data.AvailableTool & damageInfo.tool) == 0) return;

        if (PhotonNetwork.IsMasterClient)
            ApplyDamage(damageInfo.damage);
        else
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, damageInfo.damage);
    }

    void ApplyDamage(float power)
    {
        if (_IsDead) return;

        photonView.RPC(nameof(RPC_ShowPopup), RpcTarget.All, transform.position, (int)power);

        _CurHP -= power;
        if (_CurHP <= 0f)
        {
            _IsDead = true;
            OnDestroyed?.Invoke();
        }
    }

    [PunRPC] void RPC_ApplyDamage(float power)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        ApplyDamage(power);
    }

    [PunRPC] void RPC_ShowPopup(Vector3 pos, int amount)
    {
        DamagePopManager._Inst?.Show(pos, amount);
    }
}
