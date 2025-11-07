using UnityEngine;

[RequireComponent(typeof(ButtonActionRunner))]
public class CloseUIAction : MonoBehaviour, IButtonAction
{
    [SerializeField] GameObject[] _Targets;

    public void Execute()
    {
        if (_Targets == null || _Targets.Length == 0) return;

        foreach (var target in _Targets)
        {
            UIFX.Hide(target);
        }
    }
}
