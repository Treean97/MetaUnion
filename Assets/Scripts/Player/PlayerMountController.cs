using System;
using Photon.Pun;
using UnityEngine;

public class PlayerMountController : MonoBehaviourPun
{
    public MountEntity CurrentMount { get; private set; }
    public bool IsDriver { get; private set; }
    public bool IsDriving => CurrentMount != null && IsDriver;

    public event Action OnMountStateChanged;

    public void SetMount(MountEntity mount, bool isDriver)
    {
        CurrentMount = mount;
        IsDriver = isDriver;

        if (!photonView.IsMine) return;

        OnMountStateChanged?.Invoke();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (!IsDriving) return;

        MountInput input = new MountInput
        {
            Throttle = Input.GetAxisRaw("Vertical"),
            Steer = Input.GetAxisRaw("Horizontal"),
            Brake = Input.GetKey(KeyCode.Space),
        };

        CurrentMount.SetDriverInput(input);
    }
}
