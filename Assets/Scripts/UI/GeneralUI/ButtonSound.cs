using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        var button = gameObject.GetComponent<Button>();
        if (!button.interactable) return;

        AudioManager._Inst.PlayUIHover();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var button = gameObject.GetComponent<Button>();
        if (!button.interactable) return;

        AudioManager._Inst.PlayUIClick();
    }

    
}
