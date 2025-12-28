using System;
using Photon.Pun;
using UnityEngine;

public class PlayerMountController : MonoBehaviourPun
{
    public MountEntity CurrentMount { get; private set; }
    public bool IsDriver { get; private set; }

    public bool IsDriving => CurrentMount != null && IsDriver;

    private IDisposable _FocusToken;

    // MountEntity가 탑승/하차 때 호출
    public void SetMount(MountEntity mount, bool isDriver)
    {
        CurrentMount = mount;
        IsDriver = isDriver;

        // UI는 로컬만
        if (!photonView.IsMine)
        return;

        // 탑승 시작
        if (mount != null)
        {
            if (_FocusToken == null)
                _FocusToken = FocusUIBlockManager.AcquireBlockToken("Mounting");

            return;
        }

        // 하차
        _FocusToken?.Dispose();
        _FocusToken = null;
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

    void OnDestroy()
    {
        // 씬 이동/파괴 누수 방지
        _FocusToken?.Dispose();
        _FocusToken = null;
    }
}
