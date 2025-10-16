using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float _HoverScale = 1.08f;
    [SerializeField] float _ScaleTime  = 0.10f;
    [SerializeField] Ease  _Ease       = Ease.InOutQuad; // SmoothStep에 가까운 감속/가속

    RectTransform _RT;
    Vector3 _BaseScale;
    Tweener _ScaleTween;

    void Awake() => _RT = (RectTransform)transform;

    void OnEnable() => _BaseScale = _RT.localScale;

    void OnDisable()
    {
        _ScaleTween?.Kill();
        _RT.localScale = _BaseScale;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        var button = GetComponent<Button>();
        if (!button || !button.interactable) return;

        StartScale(_BaseScale * _HoverScale);
    }

    public void OnPointerExit(PointerEventData e)
    {
        StartScale(_BaseScale);
    }

    void StartScale(Vector3 target)
    {
        _ScaleTween?.Kill();
        _ScaleTween = _RT
            .DOScale(target, _ScaleTime)
            .SetEase(_Ease)       // SmoothStep 느낌
            .SetUpdate(false);    // 기존과 동일: Time.deltaTime 기반(타임스케일 영향)
    }
}
