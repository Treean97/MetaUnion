using UnityEngine;
using UnityEngine.UI;

public class StartBtnManager : MonoBehaviour
{

    [SerializeField] private Button _StartButton;

    void Awake()
    {
        GameEvents.OnBtnActive += HandleBtnActive;
        GameEvents.OnBtnInteractable += HandleBtnInteractable;
    }

    void OnDestroy()
    {
        GameEvents.OnBtnActive -= HandleBtnActive;
        GameEvents.OnBtnInteractable -= HandleBtnInteractable;
    }

    private void HandleBtnInteractable(UIButtonID btnID, bool enable)
    {
        if (btnID != UIButtonID.Start)
        {
            return;
        }

        _StartButton.interactable = enable;
    }
    

    private void HandleBtnActive(UIButtonID btnID, bool enable)
    {
        if (btnID != UIButtonID.Start)
        {
            return;
        }

        _StartButton.gameObject.SetActive(enable);
    } 
}
