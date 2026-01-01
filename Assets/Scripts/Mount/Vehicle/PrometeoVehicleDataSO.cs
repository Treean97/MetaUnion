using UnityEngine;

[CreateAssetMenu(menuName = "Mount/Vehicle Data", fileName = "VehicleData")]
public class PrometeoVehicleDataSO  : MountDataSO
{
    public int MaxSpeed = 90;
    public int MaxReverseSpeed = 45;
    public int AccelerationMultiplier = 2;

    public int MaxSteeringAngle = 27;
    public float SteeringSpeed = 0.5f;

    public int BrakeForce = 350;
    public int DecelerationMultiplier = 2;
    public int HandbrakeDriftMultiplier = 5;

    public Vector3 BodyMassCenter = new Vector3(0f, 0.5f, 0f);
}
