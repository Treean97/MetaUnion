using UnityEngine;

public abstract class MountDataSO : ScriptableObject
{
    [SerializeField] private InfoDataSO _InfoData;
    public InfoDataSO InfoData => _InfoData;

    [Header("운전자 없을 때 감속 설정")]
    public float NoDriverLinearDamp = 2.5f;
    public float NoDriverAngularDamp = 2.5f;
    public float StopSpeed = 0.15f;
    public float StopAngular = 0.15f;
}