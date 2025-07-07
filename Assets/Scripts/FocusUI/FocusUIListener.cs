using UnityEngine;

public class FocusUIListener : MonoBehaviour
{
    [SerializeField] FocusUIManager _FocusUIPanel;
    private bool _IsBlocked;

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

    private void HandleFocus(ObjectInfoSO objInfo)
    {
        if(_IsBlocked) return;
        
        _FocusUIPanel.Show(objInfo);
        _FocusUIPanel.gameObject.SetActive(true);
    }

    private void HandleDefocus()
    {
        _FocusUIPanel.gameObject.SetActive(false);
    }
    
}
