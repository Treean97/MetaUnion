using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{

    [SerializeField] private Button _StartButton;

    void Awake()
    {
        GameEvents.OnSetActive += HandleBtnActive;
        GameEvents.OnBtnSetInteractable += HandleBtnInteractable;
    }

    void OnDestroy()
    {
        GameEvents.OnSetActive -= HandleBtnActive;
        GameEvents.OnBtnSetInteractable -= HandleBtnInteractable;
    }

    private void HandleBtnInteractable(UIID btnID, bool enable)
    {
        if (btnID != UIID.Start)
        {
            return;
        }

        _StartButton.interactable = enable;
    }
    

    private void HandleBtnActive(UIID btnID, bool enable)
    {
        if (btnID != UIID.Start)
        {
            return;
        }

        _StartButton.gameObject.SetActive(enable);
    } 
}
