using System;
using Controller;
using Photon.Pun;
using UnityEngine;

public class PlayerMountController : MonoBehaviourPun
{
    public MountEntity CurrentMount { get; private set; }
    public bool IsDriver { get; private set; }
    public bool IsDriving => CurrentMount != null && IsDriver;

    public event Action OnMountStateChanged;

    PlayerInput _Input;

    void Awake()
    {
        _Input = GetComponent<PlayerInput>();
        if (_Input == null)
            Debug.LogError($"[{name}] PlayerMountController에 PlayerInput이 없습니다.", this);
    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;
        if (_Input != null) _Input.OnMountInput += HandleMountInput;
    }

    void OnDisable()
    {
        if (!photonView.IsMine) return;
        if (_Input != null) _Input.OnMountInput -= HandleMountInput;
    }

    public void SetMount(MountEntity mount, bool isDriver)
    {
        CurrentMount = mount;
        IsDriver = isDriver;

        if (!photonView.IsMine) return;

        // 운전 상태 변경 시, 마운트에 남아있는 입력을 초기화(안전)
        if (!IsDriving && CurrentMount != null)
            CurrentMount.SetDriverInput(default);

        OnMountStateChanged?.Invoke();
    }

    void HandleMountInput(MountInput input)
    {
        if (!IsDriving) return;
        if (CurrentMount == null) return;

        CurrentMount.SetDriverInput(input);
    }
}
