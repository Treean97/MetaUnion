using UnityEngine;

public class OpenUIAction : MonoBehaviour, IButtonAction
{
    [SerializeField] GameObject _Target;
    public void Execute() { if (_Target) _Target.SetActive(true); }
}

