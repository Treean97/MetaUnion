using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float _HoverScale = 1.08f;
    [SerializeField] float _ScaleTime  = 0.10f;

    RectTransform _RT;
    Vector3 _BaseScale;
    Coroutine _ScaleCo;

    void Awake()
    {
        _RT = (RectTransform)transform;
    }

    void OnEnable()
    {
        _BaseScale = _RT.localScale;
    }

    void OnDisable()
    {
        if (_ScaleCo != null) StopCoroutine(_ScaleCo);
        _RT.localScale = _BaseScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        var button = GetComponent<Button>();
        if (!button.interactable) return;
        
        StartScale(_BaseScale * _HoverScale);
    }

    public void OnPointerExit(PointerEventData e)
    {        
        StartScale(_BaseScale);
    }

    void StartScale(Vector3 target)
    {
        if (_ScaleCo != null) StopCoroutine(_ScaleCo);
        _ScaleCo = StartCoroutine(CoScaleTo(target, _ScaleTime));
    }


    IEnumerator CoScaleTo(Vector3 target, float duration)
    {
        Vector3 start = _RT.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            float eased = Mathf.SmoothStep(0f, 1f, u);
            _RT.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }
        _RT.localScale = target;
    }


}
