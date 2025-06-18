using UnityEngine;

public class ControlPanelUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _ControlPanelUI;

    void OnEnable()
    {
        GameEvents.OnSetActive += HandleControlUI;
    }

    void OnDisable()
    {
        GameEvents.OnSetActive -= HandleControlUI;
    }


    void HandleControlUI(UIID uiID, bool enable)
    {
        if (uiID != UIID.Control)
        {
            return;
        }

        _ControlPanelUI.SetActive(enable);
    }

}
