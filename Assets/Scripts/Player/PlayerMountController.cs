using Photon.Pun;
using UnityEngine;

public class PlayerMountController : MonoBehaviourPun
{
    public MountEntity CurrentMount { get; private set; }
    public bool IsDriving => CurrentMount != null;

    // MountEntity가 탑승/하차 때 호출
    public void SetMount(MountEntity mount)
    {
        CurrentMount = mount;
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
