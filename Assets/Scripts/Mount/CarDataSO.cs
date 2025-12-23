using UnityEngine;

[CreateAssetMenu(menuName = "Mount/Vehicle Data", fileName = "VehicleData")]
public class CarDataSO : MountDataSO
{
    [Header("Car")]
    [SerializeField] float _Accel;
    public float Accel => _Accel;
    [SerializeField] float _TurnDegPerSec;
    public float TurnDegPerSec => _TurnDegPerSec;
    [SerializeField] float _BrakeDrag;
    public float BrakeDrag => _BrakeDrag;
}
