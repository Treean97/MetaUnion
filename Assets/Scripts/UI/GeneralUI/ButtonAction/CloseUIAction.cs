using UnityEngine;


public class CloseUIAction : MonoBehaviour, IButtonAction
{
    [SerializeField] GameObject _Target;
    public void Execute() { if (_Target) _Target.SetActive(false); }
}
