using UnityEngine;

public class WarningUIListener : MonoBehaviour
{
    [SerializeField] private WarningUIManager _WarningUIManager;

    private void OnEnable()
    {
        GameEvents.OnShowWarning += ShowWarning;
    }

    private void OnDisable()
    {
        GameEvents.OnShowWarning -= ShowWarning;
    }

    private void ShowWarning(string message, float duration)
    {
        _WarningUIManager.Show(message, duration);
    }
}
