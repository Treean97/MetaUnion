using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Clips")]
    [SerializeField] AudioClip _HoverClip;
    [SerializeField] AudioClip _ClickClip;

    public void OnPointerDown(PointerEventData eventData)
    {
        var button = gameObject.GetComponent<Button>();
        if (!button.interactable) return;

        SoundManager._Inst.PlaySFX(_ClickClip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var button = gameObject.GetComponent<Button>();
        if (!button.interactable) return;

        SoundManager._Inst.PlaySFX(_HoverClip);
    }

    
}
