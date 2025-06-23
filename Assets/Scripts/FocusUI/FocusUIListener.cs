using UnityEngine;

public class FocusUIListener : MonoBehaviour
{
    [SerializeField] FocusUIManager _FocusUIPanel;

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
        _FocusUIPanel.Show(objInfo);
        _FocusUIPanel.gameObject.SetActive(true);
    }

    private void HandleDefocus()
    {
        _FocusUIPanel.gameObject.SetActive(false);
    }
    
}
