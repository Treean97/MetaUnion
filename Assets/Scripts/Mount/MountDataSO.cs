using UnityEngine;

public abstract class MountDataSO : ScriptableObject
{
    [SerializeField] private InfoDataSO _InfoData;
    public InfoDataSO InfoData => _InfoData;

    [Header("운전자 없을 때 감속 설정")]
    public float NoDriverDecel = 8f;
    public float NoDriverStopSpeed = 0.1f;
}