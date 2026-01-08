using UnityEngine;

[CreateAssetMenu(menuName = "Mount/Horse Data", fileName = "HorseData")]
public class HorseDataSO : MountDataSO
{
    [Header("Speed (km/h)")]
    public int MaxSpeed = 45;          // 차량과 최대한 비슷하게 기본값 맞춤
    public int MaxReverseSpeed = 20;

    [Header("Acceleration (m/s^2)")]
    public float Accel = 8f;           // 전진 가속
    public float ReverseAccel = 6f;    // 후진 가속
    public float BrakeDecel = 18f;     // 브레이크 감속

    [Header("Steering (deg/sec)")]
    public float TurnDegPerSec = 120f;

    [Header("Animator Params")]
    public string AnimParamSpeed = "Speed";  // -1~1 (후진 음수)
    public string AnimParamTurn  = "Turn";   // -1~1
    public string AnimParamBrake = "Brake";  // bool
    public float AnimDampTime = 0.12f;
}
