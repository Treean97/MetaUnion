using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Clips")]
    [SerializeField] AudioClip _HoverClip;
    [SerializeField] AudioClip _ClickClip;

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager._Inst.PlaySFX(_ClickClip);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager._Inst.PlaySFX(_HoverClip);
    }
}
