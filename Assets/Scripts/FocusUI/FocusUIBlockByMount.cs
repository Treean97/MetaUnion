using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerMountController))]
public class FocusUIBlockByMount : MonoBehaviourPun, IFocusUIBlockFlag
{
    public event Action Changed;
    public string DebugName => "Mounting";

    private PlayerMountController _Mount;

    public bool Value => _Mount != null && _Mount.IsDriving;

    void Awake()
    {
        _Mount = GetComponent<PlayerMountController>();
    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        if (_Mount != null)
            _Mount.OnMountStateChanged += HandleMountChanged;

        FocusUIBlockAuto.Register(this);
        Changed?.Invoke(); // 초기 1회 반영
    }

    void OnDisable()
    {
        if (_Mount != null)
            _Mount.OnMountStateChanged -= HandleMountChanged;

        FocusUIBlockAuto.Unregister(this);
    }

    void OnDestroy() => OnDisable();

    private void HandleMountChanged()
    {
        Changed?.Invoke();
    }
}
