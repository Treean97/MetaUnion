using UnityEngine;

[CreateAssetMenu(menuName = "Mount/Vehicle Config", fileName = "VehicleConfig")]
public class VehicleConfigSO : ScriptableObject
{
    [Header("Car")]
    public float Accel;
    public float TurnDegPerSec;
    public float BrakeDrag;
}
