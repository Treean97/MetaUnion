using UnityEngine;

public class FocusUIListener : MonoBehaviour
{
    [SerializeField] FocusUIManager _FocusUIPanel;
    // private bool _IsBlocked;

    void OnEnable()
    {
        // Focus UI
        GameEvents.OnFocus += HandleFocus;
        GameEvents.OnDefocus += HandleDefocus;
    }

    void OnDisable()
    {
        // Focus UI
        GameEvents.OnFocus -= HandleFocus;
        GameEvents.OnDefocus -= HandleDefocus;
    }

    private void HandleFocus(InfoDataSO objInfo)
    {
        // if(_IsBlocked) return;
        
        _FocusUIPanel.Setup(objInfo);
        UIFX.Show(_FocusUIPanel.gameObject);
    }

    private void HandleDefocus()
    {
        UIFX.Hide(_FocusUIPanel.gameObject);
    }
    
}
