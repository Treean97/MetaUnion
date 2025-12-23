using UnityEngine;

public abstract class MountDataSO : ScriptableObject
{
    [SerializeField] private InfoDataSO _InfoData;
    public InfoDataSO InfoData => _InfoData;
}